namespace Example.Modules.Calendar;

public sealed partial class CalendarStyleViewModel : AppViewModelBase
{
    private static readonly (string Name, CalendarStyle Style, bool WeekNumber)[] Themes =
    [
        ("Default", new CalendarStyle(), false),
        ("Dark", CreateDarkStyle(), false),
        ("Accent", CreateAccentStyle(), true),
        ("Minimal", CreateMinimalStyle(), false)
    ];

    private int themeIndex;

    [ObservableProperty]
    public partial bool IsSettingOpen { get; set; }

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
    public partial bool WeekNumberVisible { get; set; }

    [ObservableProperty]
    public partial bool HeaderVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool AppHeaderVisible { get; set; }

    [ObservableProperty]
    public partial bool TodayButtonVisible { get; set; } = true;

    [ObservableProperty]
    public partial string ThemeName { get; set; } = Themes[0].Name;

    [ObservableProperty]
    public partial string HeaderText { get; set; } = "Built-in header";

    [ObservableProperty]
    public partial string Status { get; set; } = "Theme  Default";

    public IObserveCommand DisplayDateChangedCommand { get; }

    public IObserveCommand PrevCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand SelectThemeCommand { get; }

    public IObserveCommand SelectNameFormatCommand { get; }

    public IObserveCommand ToggleHeaderCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public CalendarStyleViewModel()
    {
        DisplayDateChangedCommand = MakeDelegateCommand<CalendarDisplayDateChangedEventArgs>(CommandMode.Simple, Load);
        PrevCommand = MakeDelegateCommand(() => DisplayDate = DisplayDate.AddMonths(-1));
        NextCommand = MakeDelegateCommand(() => DisplayDate = DisplayDate.AddMonths(1));
        SelectThemeCommand = MakeDelegateCommand<string>(SelectTheme);
        SelectNameFormatCommand = MakeDelegateCommand<CalendarWeekdayNameFormat>(SelectNameFormat);
        ToggleHeaderCommand = MakeDelegateCommand(ToggleHeader);
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

    private void SelectTheme(string name)
    {
        themeIndex = Array.FindIndex(Themes, x => x.Name == name);
        var (_, style, weekNumber) = Themes[themeIndex];
        CalendarStyle = style;
        WeekNumberVisible = weekNumber;
        ThemeName = name;
        Status = $"Theme  {name}";
        IsSettingOpen = false;
    }

    private void SelectNameFormat(CalendarWeekdayNameFormat format)
    {
        WeekdayNameFormat = format;
        Status = $"Weekday  {format}";
    }

    // Swaps the built-in header for the one of the page
    private void ToggleHeader()
    {
        HeaderVisible = !HeaderVisible;
        AppHeaderVisible = !HeaderVisible;
        HeaderText = HeaderVisible ? "Built-in header" : "Page header";
        Status = HeaderText;
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
        StampColor = Color.FromArgb("#E0E0E0"),
        HeaderBackground = Color.FromArgb("#1F1F1F"),
        HeaderTextColor = Colors.White,
        NavigationButtonColor = Color.FromArgb("#E0E0E0"),
        WeekdayHeaderBackground = Color.FromArgb("#1F1F1F"),
        WeekdayHeaderTextColor = Color.FromArgb("#BDBDBD"),
        SaturdayHeaderTextColor = Color.FromArgb("#64B5F6"),
        SundayHeaderTextColor = Color.FromArgb("#EF5350"),
        MonthIndicatorColor = Color.FromArgb("#10FFFFFF")
    };

    private static CalendarStyle CreateMinimalStyle() => new()
    {
        GridLineMode = CalendarGridLineMode.None,
        EventDisplayMode = CalendarEventDisplayMode.Dots,
        DateNumberShape = CalendarDateNumberShape.Circle,
        TodayMarkStyle = CalendarMarkStyle.Outline,
        TodayMarkOnSelected = true,
        TodayTextColor = Color.FromArgb("#1F1F1F"),
        DateNumberBold = false,
        WeekendBackground = Colors.Transparent,
        HolidayBackground = Colors.Transparent
    };

    private static CalendarStyle CreateAccentStyle() => new()
    {
        GridLineMode = CalendarGridLineMode.Horizontal,
        GridLineColor = Color.FromArgb("#E3E6F5"),
        DateNumberShape = CalendarDateNumberShape.Circle,
        TodayMarkStyle = CalendarMarkStyle.Fill,
        SelectedMarkStyle = CalendarMarkStyle.Outline,
        TodayMarkOnSelected = true,
        EventCornerRadius = 8,
        SaturdayTextColor = Color.FromArgb("#5C6BC0"),
        SundayTextColor = Color.FromArgb("#3F51B5"),
        HolidayTextColor = Color.FromArgb("#3F51B5"),
        OutsideMonthTextColor = Color.FromArgb("#C5CAE9"),
        OutsideMonthBackground = Colors.White,
        WeekendBackground = Color.FromArgb("#F6F7FC"),
        HolidayBackground = Color.FromArgb("#EDEFF9"),
        TodayBackground = Color.FromArgb("#3F51B5"),
        TodayTextColor = Colors.White,
        SelectedDayBackground = Color.FromArgb("#3F51B5"),
        SelectedDayTextColor = Color.FromArgb("#3F51B5"),
        RangeBackground = Color.FromArgb("#E8EAF6"),
        DayLabelTextColor = Color.FromArgb("#7986CB"),
        OverflowTextColor = Color.FromArgb("#7986CB"),
        StampColor = Color.FromArgb("#3F51B5"),
        HeaderTextColor = Color.FromArgb("#3F51B5"),
        NavigationButtonColor = Color.FromArgb("#3F51B5"),
        WeekdayHeaderBackground = Color.FromArgb("#EEF0FA"),
        WeekdayHeaderTextColor = Color.FromArgb("#5C6BC0"),
        SaturdayHeaderTextColor = Color.FromArgb("#5C6BC0"),
        SundayHeaderTextColor = Color.FromArgb("#3F51B5"),
        MonthIndicatorColor = Color.FromArgb("#0D3F51B5")
    };
}
