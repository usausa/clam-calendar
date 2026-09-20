namespace Example.Modules.Calendar;

public sealed partial class CalendarStampsViewModel : AppViewModelBase
{
    private static readonly string[] Glyphs = ["⭐", "🍀", "🎂", "🏃", "📚", "🎵"];

    private bool stampsVisible = true;

    private bool labelsVisible = true;

    private CalendarDateRange range;

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Observable so that stamps added by a tap redraw the calendar without replacing the collection
    public ObservableCollection<CalendarStamp> Stamps { get; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, string>? DayLabels { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Tap a day to add a stamp, long press to remove its stamps.";

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand DayLongPressedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarStampsViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(Load, CommandBehavior.AllowBusyExecution);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(AddStamp);
        DayLongPressedCommand = MakeDelegateCommand<CalendarDayEventArgs>(RemoveStamps);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.CalendarMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        stampsVisible = !stampsVisible;
        LoadStamps();
        Status = stampsVisible ? "Sample stamps shown." : "Sample stamps hidden.";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        labelsVisible = !labelsVisible;
        LoadLabels();
        Status = labelsVisible ? "Holiday labels shown." : "Holiday labels hidden.";
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

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        range = new CalendarDateRange(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        LoadStamps();
        LoadLabels();
    }

    private void LoadStamps()
    {
        Stamps.Clear();
        if (stampsVisible)
        {
            foreach (var stamp in SampleSchedule.CreateStamps(range.Start, range.End))
            {
                Stamps.Add(stamp);
            }
        }
    }

    private void LoadLabels()
    {
        DayLabels = labelsVisible ? SampleHolidays.GetLabels(range.Start, range.End) : null;
    }

    private void AddStamp(CalendarDayEventArgs e)
    {
        var count = Stamps.Count(x => x.Date == e.Date);
        var glyph = Glyphs[count % Glyphs.Length];
        var position = (count % 3) switch { 0 => CalendarStampPosition.BottomRight, 1 => CalendarStampPosition.BottomLeft, _ => CalendarStampPosition.Center };
        Stamps.Add(new CalendarStamp { Date = e.Date, Glyph = glyph, Position = position, FontSize = 20 });
        Status = $"Added {glyph} to {e.Date:MM/dd}";
    }

    private void RemoveStamps(CalendarDayEventArgs e)
    {
        foreach (var stamp in Stamps.Where(x => x.Date == e.Date).ToArray())
        {
            Stamps.Remove(stamp);
        }

        Status = $"Removed the stamps of {e.Date:MM/dd}";
    }
}
