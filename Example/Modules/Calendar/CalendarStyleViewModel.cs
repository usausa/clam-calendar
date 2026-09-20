namespace Example.Modules.Calendar;

public sealed partial class CalendarStyleViewModel : AppViewModelBase
{
    private static readonly (string Name, CalendarStyle Style)[] Themes =
    [
        ("Default", new CalendarStyle()),
        ("Dark", CreateDarkStyle()),
        ("Contrast", CreateContrastStyle())
    ];

    private int themeIndex;

    [ObservableProperty]
    public partial DateOnly DisplayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    public partial IReadOnlyList<CalendarEvent> Events { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<DateOnly> Holidays { get; set; } = [];

    [ObservableProperty]
    public partial DateOnly? SelectedDate { get; set; }

    [ObservableProperty]
    public partial CalendarStyle CalendarStyle { get; set; } = Themes[0].Style;

    [ObservableProperty]
    public partial CalendarWeekdayNameFormat WeekdayNameFormat { get; set; }

    [ObservableProperty]
    public partial bool HeaderVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool AppHeaderVisible { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Theme: Default";

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand PrevCommand { get; }

    public IObserveCommand NextCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarStyleViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(Load, CommandBehavior.AllowBusyExecution);
        PrevCommand = MakeDelegateCommand(() => DisplayDate = DisplayDate.AddMonths(-1));
        NextCommand = MakeDelegateCommand(() => DisplayDate = DisplayDate.AddMonths(1));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.CalendarMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        themeIndex = (themeIndex + 1) % Themes.Length;
        var (name, style) = Themes[themeIndex];
        CalendarStyle = style;
        Status = $"Theme: {name}";
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        WeekdayNameFormat = WeekdayNameFormat switch
        {
            CalendarWeekdayNameFormat.Initial => CalendarWeekdayNameFormat.Abbreviated,
            CalendarWeekdayNameFormat.Abbreviated => CalendarWeekdayNameFormat.Full,
            _ => CalendarWeekdayNameFormat.Initial
        };
        Status = $"Weekday names: {WeekdayNameFormat}";
        return Task.CompletedTask;
    }

    // Swaps the built-in header for the one of the page
    protected override Task OnNotifyFunction4()
    {
        HeaderVisible = !HeaderVisible;
        AppHeaderVisible = !HeaderVisible;
        Status = HeaderVisible ? "Built-in header" : "Application header";
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load(CalendarDisplayDateChangedEventArgs e)
    {
        Events = SampleSchedule.CreateEvents(e.FirstDate, e.LastDate);
        Holidays = SampleHolidays.GetHolidays(e.FirstDate, e.LastDate);
    }

    private static CalendarStyle CreateDarkStyle() => new()
    {
        Background = Color.FromArgb("#121212"),
        GridLineColor = Color.FromArgb("#333333"),
        WeekdayTextColor = Color.FromArgb("#E0E0E0"),
        SaturdayTextColor = Color.FromArgb("#64B5F6"),
        SundayTextColor = Color.FromArgb("#EF5350"),
        HolidayTextColor = Color.FromArgb("#EF5350"),
        OutsideMonthTextColor = Color.FromArgb("#616161"),
        OutsideMonthBackground = Color.FromArgb("#1A1A1A"),
        WeekendBackground = Color.FromArgb("#1E1E24"),
        HolidayBackground = Color.FromArgb("#2A1E1E"),
        TodayBackground = Colors.White,
        TodayTextColor = Colors.Black,
        SelectedDayBackground = Color.FromArgb("#90CAF9"),
        SelectedDayTextColor = Colors.Black,
        RangeBackground = Color.FromArgb("#263238"),
        DisabledDayTextColor = Color.FromArgb("#424242"),
        DayLabelTextColor = Color.FromArgb("#9E9E9E"),
        OverflowTextColor = Color.FromArgb("#9E9E9E"),
        HeaderBackground = Color.FromArgb("#1F1F1F"),
        HeaderTextColor = Colors.White,
        NavigationButtonColor = Color.FromArgb("#E0E0E0"),
        WeekdayHeaderBackground = Color.FromArgb("#1F1F1F"),
        WeekdayHeaderTextColor = Color.FromArgb("#BDBDBD"),
        SaturdayHeaderTextColor = Color.FromArgb("#64B5F6"),
        SundayHeaderTextColor = Color.FromArgb("#EF5350"),
        MonthIndicatorColor = Color.FromArgb("#10FFFFFF")
    };

    private static CalendarStyle CreateContrastStyle() => new()
    {
        HeaderFontSize = 22,
        DateNumberFontSize = 16,
        EventFontSize = 12,
        GridLineColor = Colors.Black,
        WeekdayTextColor = Colors.Black,
        SaturdayTextColor = Color.FromArgb("#0000FF"),
        SundayTextColor = Color.FromArgb("#C00000"),
        HolidayTextColor = Color.FromArgb("#C00000"),
        OutsideMonthTextColor = Color.FromArgb("#808080"),
        OutsideMonthBackground = Color.FromArgb("#E0E0E0"),
        WeekendBackground = Color.FromArgb("#FFF8C0"),
        HolidayBackground = Color.FromArgb("#FFD6D6"),
        TodayBackground = Colors.Black,
        TodayTextColor = Colors.Yellow,
        SelectedDayBackground = Color.FromArgb("#0000A0"),
        SelectedDayTextColor = Colors.White,
        RangeBackground = Color.FromArgb("#C0C0FF"),
        DisabledDayTextColor = Color.FromArgb("#A0A0A0"),
        HeaderBackground = Colors.Black,
        HeaderTextColor = Colors.White,
        NavigationButtonColor = Colors.White,
        WeekdayHeaderBackground = Colors.Black,
        WeekdayHeaderTextColor = Colors.White,
        SaturdayHeaderTextColor = Color.FromArgb("#80B0FF"),
        SundayHeaderTextColor = Color.FromArgb("#FF8080")
    };
}
