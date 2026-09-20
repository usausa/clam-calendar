namespace Example.Modules.Calendar;

// Hotel booking: a stay is a DateRange limited to MaxNights with the sold-out days disabled,
// a day use is a SingleDate that cannot be deselected
public sealed partial class CalendarBookingViewModel : AppViewModelBase
{
    private const int MaxNights = 7;

    private const int BookableMonths = 3;

    private const string StayGuide = "Tap the check-in date, then the check-out date. 🈵 days are sold out.";

    private const string DayUseGuide = "Tap the date. Tapping it again keeps it because a booking needs a date.";

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private bool stay = true;

    private bool limited = true;

    // State after the last tap, used to explain a range that the view started over
    private DateOnly? lastStart;

    private bool awaitingCheckOut;

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
    public partial string Status { get; set; } = StayGuide;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarBookingViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(Load, CommandBehavior.AllowBusyExecution);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(OnDayTapped);

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.CalendarMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    // Stay (DateRange) or day use (SingleDate)
    protected override Task OnNotifyFunction2()
    {
        stay = !stay;
        ClearSelection();
        SelectionMode = stay ? CalendarSelectionMode.DateRange : CalendarSelectionMode.SingleDate;
        UpdateSummary();
        Status = stay ? StayGuide : DayUseGuide;
        return Task.CompletedTask;
    }

    // Limit of the stay length
    protected override Task OnNotifyFunction3()
    {
        limited = !limited;
        MaxSelectableDays = limited ? MaxNights + 1 : 0;
        ClearSelection();
        UpdateSummary();
        Status = limited ? $"Stays are limited to {MaxNights} nights." : "The stay length is not limited.";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        ClearSelection();
        UpdateSummary();
        Status = "Selection cleared.";
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
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
    }

    // Raised after the view updated the selection
    private void OnDayTapped(CalendarDayEventArgs e)
    {
        if (!stay)
        {
            Status = $"Day use on {Format(e.Date)}.";
            return;
        }

        if (SelectedEndDate is not null)
        {
            Status = $"{Nights()} nights. Tap a date to start another stay.";
        }
        else if (awaitingCheckOut && (e.Date == lastStart))
        {
            Status = $"Tap a check-out date after {Format(e.Date)}.";
        }
        else if (awaitingCheckOut)
        {
            // The view started over at the tapped date because the stay was too long or crossed a sold-out day
            Status = limited
                ? $"That stay is longer than {MaxNights} nights or crosses a sold-out day. Check-in moved to {Format(e.Date)}."
                : $"That stay crosses a sold-out day. Check-in moved to {Format(e.Date)}.";
        }
        else
        {
            Status = $"Check-in {Format(e.Date)}. Tap the check-out date.";
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

        PlanText = limited ? $"Stay  up to {MaxNights} nights" : "Stay";
        DatesText = $"Check-in  {(SelectedStartDate is { } checkIn ? Format(checkIn) : "-")}    Check-out  {(SelectedEndDate is { } checkOut ? Format(checkOut) : "-")}";
        PriceText = (SelectedStartDate is { } start) && (SelectedEndDate is { } end)
            ? $"{Nights()} nights  ¥{SampleBooking.GetStayTotal(start, end):N0}"
            : "Total  -";
    }

    private int Nights() =>
        (SelectedStartDate is { } checkIn) && (SelectedEndDate is { } checkOut) ? checkOut.DayNumber - checkIn.DayNumber : 0;

    private static string Format(DateOnly date) => date.ToString("MM/dd (ddd)", CultureInfo.InvariantCulture);
}
