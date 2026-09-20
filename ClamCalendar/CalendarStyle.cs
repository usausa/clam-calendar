namespace ClamCalendar;

// Can be a XAML resource; treat it as immutable once assigned and replace it with a with expression
public sealed record CalendarStyle
{
    public string FontFamily { get; set; } = "sans-serif";

    public float HeaderFontSize { get; set; } = 20;

    public float WeekdayHeaderFontSize { get; set; } = 12;

    public float DateNumberFontSize { get; set; } = 14;

    public float EventFontSize { get; set; } = 11;

    public float DayLabelFontSize { get; set; } = 9;

    public float MonthIndicatorFontSize { get; set; } = 120;

    public float HeaderHeight { get; set; } = 48;

    public float WeekdayHeaderHeight { get; set; } = 32;

    public float DateRowHeight { get; set; } = 28;

    public float SlotRowHeight { get; set; } = 18;

    public float EventRowHeight { get; set; } = 16;

    public float DateNumberSize { get; set; } = 24;

    public float DateNumberMargin { get; set; } = 4;

    public float StampMarginEdge { get; set; } = 2;

    public float EventCornerRadius { get; set; } = 4;

    public float EventPadding { get; set; } = 4;

    public float NavigationButtonWidth { get; set; } = 44;

    public float TodayButtonWidth { get; set; } = 64;

    public Color Background { get; set; } = Colors.White;

    public Color GridLineColor { get; set; } = Color.FromArgb("#E0E0E0");

    public Color WeekdayTextColor { get; set; } = Color.FromArgb("#1F1F1F");

    public Color SaturdayTextColor { get; set; } = Color.FromArgb("#2196F3");

    public Color SundayTextColor { get; set; } = Color.FromArgb("#E53935");

    public Color HolidayTextColor { get; set; } = Color.FromArgb("#E53935");

    public Color OutsideMonthTextColor { get; set; } = Color.FromArgb("#BDBDBD");

    public Color OutsideMonthBackground { get; set; } = Color.FromArgb("#F2F2F2");

    public Color WeekendBackground { get; set; } = Color.FromArgb("#FFF1F1");

    public Color HolidayBackground { get; set; } = Color.FromArgb("#FFF1F1");

    public Color TodayBackground { get; set; } = Colors.Black;

    public Color TodayTextColor { get; set; } = Colors.White;

    public Color SelectedDayBackground { get; set; } = Color.FromArgb("#1A73E8");

    public Color SelectedDayTextColor { get; set; } = Colors.White;

    public Color RangeBackground { get; set; } = Color.FromArgb("#BDD7F5");

    public Color DisabledDayTextColor { get; set; } = Color.FromArgb("#C0C0C0");

    public Color DayLabelTextColor { get; set; } = Color.FromArgb("#757575");

    public Color OverflowTextColor { get; set; } = Color.FromArgb("#757575");

    public Color HeaderBackground { get; set; } = Colors.White;

    public Color HeaderTextColor { get; set; } = Colors.Black;

    public Color NavigationButtonColor { get; set; } = Color.FromArgb("#333333");

    public Color WeekdayHeaderBackground { get; set; } = Colors.White;

    public Color WeekdayHeaderTextColor { get; set; } = Color.FromArgb("#333333");

    public Color SaturdayHeaderTextColor { get; set; } = Color.FromArgb("#2196F3");

    public Color SundayHeaderTextColor { get; set; } = Color.FromArgb("#E53935");

    public Color MonthIndicatorColor { get; set; } = Color.FromArgb("#10000000");

    internal void Validate()
    {
        if (!IsPositive(HeaderFontSize) || !IsPositive(WeekdayHeaderFontSize) || !IsPositive(DateNumberFontSize) || !IsPositive(EventFontSize) || !IsPositive(DayLabelFontSize) || !IsPositive(MonthIndicatorFontSize))
        {
            throw new ArgumentException("Font sizes must be positive finite values.");
        }

        if (!IsPositive(HeaderHeight) || !IsPositive(WeekdayHeaderHeight) || !IsPositive(DateRowHeight) || !IsPositive(SlotRowHeight) || !IsPositive(EventRowHeight) || !IsPositive(DateNumberSize) || !IsPositive(NavigationButtonWidth) || !IsPositive(TodayButtonWidth))
        {
            throw new ArgumentException("Heights and widths must be positive finite values.");
        }

        if (!IsNonNegative(DateNumberMargin) || !IsNonNegative(StampMarginEdge) || !IsNonNegative(EventCornerRadius) || !IsNonNegative(EventPadding))
        {
            throw new ArgumentException("Margins and paddings must be finite values of zero or more.");
        }
    }

    private static bool IsPositive(float value) => Single.IsFinite(value) && (value > 0);

    private static bool IsNonNegative(float value) => Single.IsFinite(value) && (value >= 0);
}
