namespace Example.Modules.Calendar;

public sealed partial class CalendarBasicViewModel : AppViewModelBase
{
    private static readonly CultureInfo Japanese = CultureInfo.GetCultureInfo("ja-JP");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<CalendarStamp> Stamps { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial CalendarSelectionMode SelectionMode { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedDate { get; set; }

    public ObservableCollection<DateOnly> SelectedDates { get; } = [];

    [ObservableProperty]
    public partial DateOnly? SelectedStartDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedEndDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? MinDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? MaxDate { get; set; }

    [ObservableProperty]
    public partial DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    [ObservableProperty]
    public partial CultureInfo Culture { get; set; } = Japanese;

    [ObservableProperty]
    public partial bool FixedWeekRows { get; set; } = true;

    [ObservableProperty]
    public partial bool OutsideMonthDaysVisible { get; set; } = true;

    [ObservableProperty]
    public partial int MaxVisibleSlots { get; set; }

    [ObservableProperty]
    public partial bool TodayButtonVisible { get; set; } = true;

    [ObservableProperty]
    public partial string StartText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CultureText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LimitText { get; set; } = "Limit off";

    [ObservableProperty]
    public partial string Status { get; set; } = String.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand DayLongPressedCommand { get; }

    public IObserveCommand EventTappedCommand { get; }

    public IObserveCommand OverflowTappedCommand { get; }

    public IObserveCommand SelectModeCommand { get; }

    public IObserveCommand ToggleStartCommand { get; }

    public IObserveCommand ToggleCultureCommand { get; }

    public IObserveCommand ToggleSlotsCommand { get; }

    public IObserveCommand ToggleLimitCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarBasicViewModel()
    {
        // The view reports the visible range whenever the month changes; the events of that range are loaded here
        // The first report arrives while the navigation still holds the busy state, so the command must run while busy
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(x => Status = $"{x.Date:MM/dd} {x.Day.Kind}  {x.Day.Events.Count} events");
        DayLongPressedCommand = MakeDelegateCommand<CalendarDayEventArgs>(x => Status = $"Hold {x.Date:MM/dd}");
        EventTappedCommand = MakeDelegateCommand<CalendarEventEventArgs>(x => Status = $"{x.Day.Date:MM/dd} {x.Event.Title}");
        OverflowTappedCommand = MakeDelegateCommand<CalendarOverflowEventArgs>(x => Status = $"{x.Day.Date:MM/dd} +{x.HiddenCount} hidden");
        SelectModeCommand = MakeDelegateCommand<CalendarSelectionMode>(SelectMode);
        ToggleStartCommand = MakeDelegateCommand(() =>
        {
            FirstDayOfWeek = FirstDayOfWeek == DayOfWeek.Monday ? DayOfWeek.Sunday : DayOfWeek.Monday;
            UpdateState();
        });
        ToggleCultureCommand = MakeDelegateCommand(() =>
        {
            Culture = Culture.Equals(Japanese) ? English : Japanese;
            UpdateState();
        });
        ToggleSlotsCommand = MakeDelegateCommand(() => MaxVisibleSlots = MaxVisibleSlots == 0 ? 2 : 0);
        ToggleLimitCommand = MakeDelegateCommand(ToggleLimit);

        UpdateState();
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

    protected override Task OnNotifyFunction4()
    {
        ClearSelection();
        Status = "No selection";
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = SampleSchedule.CreateEvents(e.FirstDate, e.LastDate);
        Stamps = SampleSchedule.CreateStamps(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        DayAppearances = SampleSchedule.CreateDayAppearances(e.FirstDate, e.LastDate);
        Status = $"{e.DisplayDate:yyyy/MM}  {Events.Count} events";
    }

    private void SelectMode(CalendarSelectionMode mode)
    {
        SelectionMode = mode;
        ClearSelection();
        UpdateState();
        IsSettingOpen = false;
    }

    private void UpdateState()
    {
        StartText = FirstDayOfWeek == DayOfWeek.Monday ? "Start Mon" : "Start Sun";
        CultureText = Culture.Name;
    }

    private void ClearSelection()
    {
        SelectedDate = null;
        SelectedDates.Clear();
        SelectedStartDate = null;
        SelectedEndDate = null;
    }

    // Limits the selectable dates and the month navigation to the current month and the next one
    private void ToggleLimit()
    {
        if (MinDate is null)
        {
            var first = new DateOnly(DisplayDate.Year, DisplayDate.Month, 1);
            MinDate = first;
            MaxDate = first.AddMonths(2).AddDays(-1);
            LimitText = "Limit on";
            Status = $"Limit {MinDate:MM/dd} - {MaxDate:MM/dd}";
        }
        else
        {
            MinDate = null;
            MaxDate = null;
            LimitText = "Limit off";
            Status = "No limit";
        }
    }
}
