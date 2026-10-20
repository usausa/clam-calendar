namespace Example.Modules.Calendar;

public sealed partial class CalendarAgendaViewModel : AppViewModelBase
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
    public partial DateOnly? SelectedDate { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> DayEvents { get; set; } = [];

    [ObservableProperty]
    public partial string StateText { get; set; } = string.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarAgendaViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(SelectDay);

        UpdateState();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.CalendarMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4()
    {
        DisplayDate = DateOnly.FromDateTime(DateTime.Today);
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
        SelectedDate = null;
        DayEvents = [];
        UpdateState();
    }

    private void SelectDay(CalendarDayEventArgs e)
    {
        DayEvents = e.Day.Events;
        UpdateState();
    }

    private void UpdateState() =>
        StateText = SelectedDate is { } date
            ? $"{date:yyyy/MM/dd}    {DayEvents.Count} event{(DayEvents.Count == 1 ? String.Empty : "s")}"
            : "No day selected";
}
