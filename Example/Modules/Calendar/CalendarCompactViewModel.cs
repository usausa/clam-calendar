namespace Example.Modules.Calendar;

public sealed partial class CalendarCompactViewModel : AppViewModelBase
{
    private static readonly DateOnly FixedToday = new(2026, 1, 15);

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial DateOnly? SelectedDate { get; set; }

    [ObservableProperty]
    public partial bool WeekdayHeaderVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool NavigationButtonsVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool SwipeEnabled { get; set; } = true;

    [ObservableProperty]
    public partial DateOnly? Today { get; set; }

    [ObservableProperty]
    public partial string TodayText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StateText { get; set; } = string.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand ToggleTodayCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarCompactViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(_ => UpdateState());
        ToggleTodayCommand = MakeDelegateCommand(ToggleToday);

        UpdateToday();
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

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = SampleSchedule.CreateEvents(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        DayAppearances = SampleSchedule.CreateDayAppearances(e.FirstDate, e.LastDate);
    }

    private void ToggleToday()
    {
        Today = Today is null ? FixedToday : null;
        if (Today is { } fixedToday)
        {
            DisplayDate = fixedToday;
        }

        UpdateToday();
    }

    private void UpdateToday() =>
        TodayText = Today is { } date ? $"Today {date:MM/dd}" : "Today sys";

    private void UpdateState() =>
        StateText = SelectedDate is { } date ? $"Selected  {date:yyyy/MM/dd}" : "Selected  -";
}
