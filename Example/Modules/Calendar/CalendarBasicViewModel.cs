namespace Example.Modules.Calendar;

public sealed partial class CalendarBasicViewModel : AppViewModelBase
{
    private static readonly CultureInfo Japanese = CultureInfo.GetCultureInfo("ja-JP");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<CalendarStamp> Stamps { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

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
    public partial string LimitText { get; set; } = "Limit off";

    [ObservableProperty]
    public partial string Status { get; set; } = "Tap a day, long press for the date, swipe to change the month.";

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand DayLongPressedCommand { get; }

    public IObserveCommand EventTappedCommand { get; }

    public IObserveCommand OverflowTappedCommand { get; }

    public IObserveCommand SelectModeCommand { get; }

    public IObserveCommand ToggleFixedCommand { get; }

    public IObserveCommand ToggleOutsideCommand { get; }

    public IObserveCommand ToggleSlotsCommand { get; }

    public IObserveCommand ToggleLimitCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarBasicViewModel()
    {
        // The view reports the visible range whenever the month changes; the events of that range are loaded here
        // The first report arrives while the navigation still holds the busy state, so the command must run while busy
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(Load, CommandBehavior.AllowBusyExecution);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(x => Status = $"Tapped {x.Date:yyyy/MM/dd} ({x.Day.Kind}) events={x.Day.Events.Count}");
        DayLongPressedCommand = MakeDelegateCommand<CalendarDayEventArgs>(x => Status = $"Long pressed {x.Date:yyyy/MM/dd}");
        EventTappedCommand = MakeDelegateCommand<CalendarEventEventArgs>(x => Status = $"Event {x.Event.Title} ({x.Event.StartDate:MM/dd} - {x.Event.EndDate:MM/dd}) on {x.Day.Date:MM/dd}");
        OverflowTappedCommand = MakeDelegateCommand<CalendarOverflowEventArgs>(x => Status = $"{x.Day.Date:MM/dd}: {x.HiddenCount} hidden of {String.Join(", ", x.Events.Select(static e => e.Title))}");
        SelectModeCommand = MakeDelegateCommand<CalendarSelectionMode>(SelectMode);
        ToggleFixedCommand = MakeDelegateCommand(() => FixedWeekRows = !FixedWeekRows);
        ToggleOutsideCommand = MakeDelegateCommand(() => OutsideMonthDaysVisible = !OutsideMonthDaysVisible);
        ToggleSlotsCommand = MakeDelegateCommand(() => MaxVisibleSlots = MaxVisibleSlots == 0 ? 2 : 0);
        ToggleLimitCommand = MakeDelegateCommand(ToggleLimit);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.CalendarMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        FirstDayOfWeek = FirstDayOfWeek == DayOfWeek.Monday ? DayOfWeek.Sunday : DayOfWeek.Monday;
        Status = $"First day of week: {FirstDayOfWeek}";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Culture = Culture.Equals(Japanese) ? English : Japanese;
        Status = $"Culture: {Culture.Name}";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        ClearSelection();
        Status = "Selection cleared.";
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
        Status = $"{e.DisplayDate:yyyy/MM}: {e.FirstDate:MM/dd} - {e.LastDate:MM/dd}, {Events.Count} events";
    }

    private void SelectMode(CalendarSelectionMode mode)
    {
        SelectionMode = mode;
        ClearSelection();
        Status = $"Selection mode: {mode}";
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
            Status = $"Limited to {MinDate:MM/dd} - {MaxDate:MM/dd}";
        }
        else
        {
            MinDate = null;
            MaxDate = null;
            LimitText = "Limit off";
            Status = "Limit removed.";
        }
    }
}
