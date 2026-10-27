namespace Example.Modules.Calendar;

public sealed partial class CalendarStyleViewModel : AppViewModelBase
{
    private static readonly (string Name, string? Key, bool WeekNumber)[] Themes =
    [
        ("Default", null, false),
        ("Dark", "DarkCalendar", false),
        ("Accent", "AccentCalendar", true),
        ("Minimal", "MinimalCalendar", false)
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
    public partial Style? CalendarTheme { get; set; }

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
        var (_, key, weekNumber) = Themes[themeIndex];
        CalendarTheme = key is null ? null : (Style)Application.Current!.Resources[key];
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
}
