namespace Example.Modules.Calendar;

public sealed partial class CalendarOverlayViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<CalendarOverlayBadge> Badges { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<CalendarOverlayBadge> TodayMarks { get; set; } = [];

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    [ObservableProperty]
    public partial bool OverlayVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool AnimationEnabled { get; set; } = true;

    [ObservableProperty]
    public partial string Statistics { get; set; } = String.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarOverlayViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
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
        DisplayDate = DateOnly.FromDateTime(DateTime.Today);
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    public void UpdateStatistics(CalendarFrameEventArgs e) =>
        Statistics = $"{e.ElapsedMilliseconds:F2}ms / {e.DayCount} days / {e.TextMeasurements} measures{(e.Animating ? " / sliding" : String.Empty)}";

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        var events = SampleSchedule.CreateEvents(e.FirstDate, e.LastDate);
        Events = events;
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        DayAppearances = SampleSchedule.CreateDayAppearances(e.FirstDate, e.LastDate);
        Badges = CreateBadges(e.DisplayDate, events);

        var today = DateOnly.FromDateTime(DateTime.Today);
        TodayMarks = (today.Year == e.DisplayDate.Year) && (today.Month == e.DisplayDate.Month)
            ? [new CalendarOverlayBadge(today, String.Empty)]
            : [];
    }

    private static List<CalendarOverlayBadge> CreateBadges(DateOnly displayDate, IReadOnlyList<CalendarEvent> events)
    {
        var badges = new List<CalendarOverlayBadge>();
        var daysInMonth = DateTime.DaysInMonth(displayDate.Year, displayDate.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(displayDate.Year, displayDate.Month, day);
            var count = events.Count(x => (x.StartDate <= date) && (x.EndDate >= date));
            if (count > 0)
            {
                badges.Add(new CalendarOverlayBadge(date, count.ToString(CultureInfo.InvariantCulture)));
            }
        }

        return badges;
    }
}
