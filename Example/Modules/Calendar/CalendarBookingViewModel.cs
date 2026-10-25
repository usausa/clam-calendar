namespace Example.Modules.Calendar;

// Hotel booking: a stay is a DateRange limited to MaxNights with the sold-out days disabled,
// a day use is a SingleDate that cannot be deselected
public sealed partial class CalendarBookingViewModel : AppViewModelBase
{
    private const int MaxNights = 7;

    private const int BookableMonths = 3;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private static readonly CalendarDayAppearance SoldOutAppearance = new() { Background = Color.FromArgb("#FFE0E0") };

    private static readonly CalendarDayAppearance HighRateAppearance = new() { BorderColor = Color.FromArgb("#FFB300"), BorderWidth = 2 };

    private bool stay = true;

    private bool limited = true;

    // State after the last tap, used to explain a range that the view started over
    private DateOnly? lastStart;

    private bool awaitingCheckOut;

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    [ObservableProperty]
    public partial string PlanButtonText { get; set; } = "Stay";

    [ObservableProperty]
    public partial string LimitButtonText { get; set; } = "Limit on";

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = Today;

    [ObservableProperty]
    public partial DateOnly? MinDate { get; set; } = Today;

    [ObservableProperty]
    public partial DateOnly? MaxDate { get; set; } = Today.AddMonths(BookableMonths).AddDays(-1);

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> SoldOutDates { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<CalendarStamp> Stamps { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial CalendarSelectionMode SelectionMode { get; set; } = CalendarSelectionMode.DateRange;

    [ObservableProperty]
    public partial int MaxSelectableDays { get; set; } = MaxNights + 1;

    [ObservableProperty]
    public partial DateOnly? SelectedDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedStartDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedEndDate { get; set; }

    [ObservableProperty]
    public partial string PlanText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DatesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Status { get; set; } = String.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand TogglePlanCommand { get; }

    public IObserveCommand ToggleLimitCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarBookingViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(OnDayTapped);
        TogglePlanCommand = MakeDelegateCommand(TogglePlan);
        ToggleLimitCommand = MakeDelegateCommand(ToggleLimit);

        // The view writes the selection back through the TwoWay bindings
        SubscribeSelectedDate(_ => UpdateSummary());
        SubscribeSelectedStartDate(_ => UpdateSummary());
        SubscribeSelectedEndDate(x =>
        {
            // A check-out on the check-in date is not a stay: keep waiting for the check-out
            if ((x is not null) && (x == SelectedStartDate))
            {
                SelectedEndDate = null;
                return;
            }

            UpdateSummary();
        });

        UpdateSummary();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync()
    {
        if (IsSettingOpen)
        {
            IsSettingOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.CalendarMenu);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsSettingOpen = !IsSettingOpen;
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Setting
    //--------------------------------------------------------------------------------

    // Stay (DateRange) or day use (SingleDate)
    private void TogglePlan()
    {
        stay = !stay;
        PlanButtonText = stay ? "Stay" : "Day use";
        ClearSelection();
        SelectionMode = stay ? CalendarSelectionMode.DateRange : CalendarSelectionMode.SingleDate;
        UpdateSummary();
        Status = String.Empty;
    }

    // Limit of the stay length
    private void ToggleLimit()
    {
        limited = !limited;
        LimitButtonText = limited ? "Limit on" : "Limit off";
        MaxSelectableDays = limited ? MaxNights + 1 : 0;
        ClearSelection();
        UpdateSummary();
        Status = String.Empty;
    }

    protected override Task OnNotifyFunction4()
    {
        ClearSelection();
        UpdateSummary();
        Status = "No dates";
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        // Only the bookable period has availability; the days before today are disabled by MinDate
        var start = e.FirstDate < MinDate ? MinDate.Value : e.FirstDate;
        var end = e.LastDate > MaxDate ? MaxDate.Value : e.LastDate;
        SoldOutDates = start <= end ? SampleBooking.GetSoldOutDates(start, end) : [];
        Stamps = SoldOutDates.Select(static x => new CalendarStamp { Date = x, Glyph = "🈵", Position = CalendarStampPosition.BottomRight, FontSize = 14 }).ToArray();
        DayAppearances = start <= end ? CreateAppearances(start, end) : null;
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
    }

    private static Dictionary<DateOnly, CalendarDayAppearance> CreateAppearances(DateOnly start, DateOnly end)
    {
        var appearances = new Dictionary<DateOnly, CalendarDayAppearance>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (SampleBooking.IsSoldOut(date))
            {
                appearances[date] = SoldOutAppearance;
            }
            else if (SampleBooking.IsHighRate(date))
            {
                appearances[date] = HighRateAppearance;
            }
        }

        return appearances;
    }

    // Raised after the view updated the selection
    private void OnDayTapped(CalendarDayEventArgs e)
    {
        if (!stay)
        {
            Status = $"Day use {Format(e.Date)}";
            return;
        }

        if (SelectedEndDate is not null)
        {
            Status = $"{Nights()} nights";
        }
        else if (awaitingCheckOut && (e.Date == lastStart))
        {
            Status = $"Check-out after {Format(e.Date)}";
        }
        else if (awaitingCheckOut)
        {
            // The view started over at the tapped date because the stay was too long or crossed a sold-out day
            Status = limited
                ? $"Too long  Check-in {Format(e.Date)}"
                : $"Sold out  Check-in {Format(e.Date)}";
        }
        else
        {
            Status = $"Check-in {Format(e.Date)}";
        }

        lastStart = SelectedStartDate;
        awaitingCheckOut = (SelectedStartDate is not null) && (SelectedEndDate is null);
    }

    private void ClearSelection()
    {
        SelectedDate = null;
        SelectedStartDate = null;
        SelectedEndDate = null;
        lastStart = null;
        awaitingCheckOut = false;
    }

    private void UpdateSummary()
    {
        if (!stay)
        {
            PlanText = $"Day use  ¥{SampleBooking.DayUseRate:N0} / day";
            DatesText = $"Date  {(SelectedDate is { } date ? Format(date) : "-")}";
            PriceText = SelectedDate is not null ? $"Total  ¥{SampleBooking.DayUseRate:N0}" : "Total  -";
            return;
        }

        PlanText = limited ? $"Stay  up to {MaxNights} nights" : "Stay  no limit";
        DatesText = $"Check-in  {(SelectedStartDate is { } checkIn ? Format(checkIn) : "-")}    Check-out  {(SelectedEndDate is { } checkOut ? Format(checkOut) : "-")}";
        PriceText = (SelectedStartDate is { } start) && (SelectedEndDate is { } end)
            ? $"{Nights()} nights  ¥{SampleBooking.GetStayTotal(start, end):N0}"
            : "Total  -";
    }

    private int Nights() =>
        (SelectedStartDate is { } checkIn) && (SelectedEndDate is { } checkOut) ? checkOut.DayNumber - checkIn.DayNumber : 0;

    private static string Format(DateOnly date) => date.ToString("MM/dd (ddd)", CultureInfo.InvariantCulture);
}
