namespace Example.Modules.Calendar;

public sealed partial class CalendarStampsViewModel : AppViewModelBase
{
    private static readonly string[] Glyphs = ["⭐", "🍀", "🎂", "🏃", "📚", "🎵"];

    private CalendarDateRange range;

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Observable so that stamps added by a tap redraw the calendar without replacing the collection
    public ObservableCollection<CalendarStamp> Stamps { get; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, CalendarDayAppearance>? DayAppearances { get; set; }

    [ObservableProperty]
    public partial IReadOnlyDictionary<DateOnly, string>? DayLabels { get; set; }

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

    public bool StampsVisible
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                RaisePropertyChanged(nameof(StampsVisible));
                LoadStamps();
            }
        }
    }
        = true;

    public bool LabelsVisible
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                RaisePropertyChanged(nameof(LabelsVisible));
                LoadLabels();
            }
        }
    }
        = true;

    [ObservableProperty]
    public partial string Status { get; set; } = String.Empty;

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand DayTappedCommand { get; }

    public IObserveCommand DayLongPressedCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarStampsViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        DayTappedCommand = MakeDelegateCommand<CalendarDayEventArgs>(AddStamp);
        DayLongPressedCommand = MakeDelegateCommand<CalendarDayEventArgs>(RemoveStamps);
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

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        range = new CalendarDateRange(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
        DayAppearances = SampleSchedule.CreateDayAppearances(e.FirstDate, e.LastDate);
        LoadStamps();
        LoadLabels();
    }

    private void LoadStamps()
    {
        Stamps.Clear();
        if (StampsVisible)
        {
            foreach (var stamp in SampleSchedule.CreateStamps(range.Start, range.End))
            {
                Stamps.Add(stamp);
            }
        }
    }

    private void LoadLabels()
    {
        DayLabels = LabelsVisible ? SampleHolidays.GetLabels(range.Start, range.End) : null;
    }

    private void AddStamp(CalendarDayEventArgs e)
    {
        var count = Stamps.Count(x => x.Date == e.Date);
        var glyph = Glyphs[count % Glyphs.Length];
        var position = (count % 3) switch { 0 => CalendarStampPosition.BottomRight, 1 => CalendarStampPosition.BottomLeft, _ => CalendarStampPosition.Center };
        Stamps.Add(new CalendarStamp { Date = e.Date, Glyph = glyph, Position = position, FontSize = 20 });
        Status = $"{e.Date:MM/dd}  + {glyph}";
    }

    private void RemoveStamps(CalendarDayEventArgs e)
    {
        foreach (var stamp in Stamps.Where(x => x.Date == e.Date).ToArray())
        {
            Stamps.Remove(stamp);
        }

        Status = $"{e.Date:MM/dd}  cleared";
    }
}
