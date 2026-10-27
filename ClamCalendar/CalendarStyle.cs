namespace ClamCalendar;

internal sealed record CalendarStyle
{
    public string FontFamily { get; set; } = "sans-serif";

    public double HeaderFontSize { get; set; } = 20;

    public double WeekdayHeaderFontSize { get; set; } = 12;

    public double DateNumberFontSize { get; set; } = 14;

    public double EventFontSize { get; set; } = 11;

    public double DayLabelFontSize { get; set; } = 9;

    public double WeekNumberFontSize { get; set; } = 9;

    public double MonthIndicatorFontSize { get; set; } = 120;

    public double HeaderHeight { get; set; } = 48;

    public double WeekdayHeaderHeight { get; set; } = 32;

    public double DateRowHeight { get; set; } = 28;

    public double SlotRowHeight { get; set; } = 18;

    public double EventRowHeight { get; set; } = 16;

    public double DateNumberSize { get; set; } = 24;

    public double DateNumberMargin { get; set; } = 4;

    public double DateNumberMarkWidth { get; set; } = 2;

    public double StampMarginEdge { get; set; } = 2;

    public double EventCornerRadius { get; set; } = 4;

    public double EventPadding { get; set; } = 4;

    public double EventDotSize { get; set; } = 6;

    public double EventDotSpacing { get; set; } = 3;

    public double NavigationButtonWidth { get; set; } = 44;

    public double TodayButtonWidth { get; set; } = 64;

    public double GridLineWidth { get; set; } = 0.5;

    public double WeekNumberWidth { get; set; } = 24;

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

    public Color StampColor { get; set; } = Colors.Black;

    public Color WeekNumberTextColor { get; set; } = Color.FromArgb("#757575");

    public Color HeaderBackground { get; set; } = Colors.White;

    public Color HeaderTextColor { get; set; } = Colors.Black;

    public Color NavigationButtonColor { get; set; } = Color.FromArgb("#333333");

    public Color WeekdayHeaderBackground { get; set; } = Colors.White;

    public Color WeekdayHeaderTextColor { get; set; } = Color.FromArgb("#333333");

    public Color SaturdayHeaderTextColor { get; set; } = Color.FromArgb("#2196F3");

    public Color SundayHeaderTextColor { get; set; } = Color.FromArgb("#E53935");

    public Color MonthIndicatorColor { get; set; } = Color.FromArgb("#10000000");

    public CalendarMarkStyle TodayMarkStyle { get; set; } = CalendarMarkStyle.Fill;

    public CalendarMarkStyle SelectedMarkStyle { get; set; } = CalendarMarkStyle.Fill;

    public bool TodayMarkOnSelected { get; set; }

    public CalendarDateNumberShape DateNumberShape { get; set; } = CalendarDateNumberShape.RoundedSquare;

    public bool DateNumberBold { get; set; } = true;

    public CalendarGridLineMode GridLineMode { get; set; } = CalendarGridLineMode.All;

    public CalendarEventDisplayMode EventDisplayMode { get; set; } = CalendarEventDisplayMode.Bars;

    public int MaxVisibleDots { get; set; } = 4;
}
