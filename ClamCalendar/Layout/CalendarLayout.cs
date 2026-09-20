namespace ClamCalendar.Layout;

// Geometry of the header, the weekday row and the day cells in DIP; independent of fonts so it can be unit tested
internal sealed class CalendarLayout
{
    public const int DaysPerWeek = CalendarEventPlacer.DaysPerWeek;

    public CalendarMonth Month { get; }

    public CalendarStyle Style { get; }

    public double Width { get; }

    public double Height { get; }

    public bool HeaderVisible { get; }

    public bool NavigationButtonsVisible { get; }

    public bool TodayButtonVisible { get; }

    public bool WeekdayHeaderVisible { get; }

    public CalendarRect HeaderBounds { get; }

    public CalendarRect PrevButtonBounds { get; }

    public CalendarRect NextButtonBounds { get; }

    public CalendarRect TodayButtonBounds { get; }

    public CalendarRect TitleBounds { get; }

    public CalendarRect WeekdayHeaderBounds { get; }

    public CalendarRect BodyBounds { get; }

    public int WeekCount => Month.Weeks.Count;

    public double ColumnWidth => BodyBounds.Width / DaysPerWeek;

    public double WeekHeight => WeekCount == 0 ? 0 : BodyBounds.Height / WeekCount;

    public CalendarLayout(CalendarMonth month, CalendarStyle style, double width, double height, bool headerVisible = true, bool navigationButtonsVisible = true, bool todayButtonVisible = false, bool weekdayHeaderVisible = true)
    {
        ArgumentNullException.ThrowIfNull(month);
        ArgumentNullException.ThrowIfNull(style);
        RequireDimension(width, nameof(width));
        RequireDimension(height, nameof(height));
        Month = month;
        Style = style;
        Width = width;
        Height = height;
        HeaderVisible = headerVisible;
        NavigationButtonsVisible = headerVisible && navigationButtonsVisible;
        TodayButtonVisible = headerVisible && todayButtonVisible;
        WeekdayHeaderVisible = weekdayHeaderVisible;

        var y = 0d;
        if (headerVisible)
        {
            var headerHeight = Math.Min(style.HeaderHeight, height);
            HeaderBounds = new CalendarRect(0, 0, width, headerHeight);
            var left = 0d;
            var right = width;
            if (navigationButtonsVisible)
            {
                var buttonWidth = Math.Min(style.NavigationButtonWidth, width / 2);
                PrevButtonBounds = new CalendarRect(0, 0, buttonWidth, headerHeight);
                NextButtonBounds = new CalendarRect(width - buttonWidth, 0, buttonWidth, headerHeight);
                left = PrevButtonBounds.Right;
                right = NextButtonBounds.X;
            }

            if (todayButtonVisible)
            {
                var todayWidth = Math.Min(style.TodayButtonWidth, Math.Max(0, right - left));
                TodayButtonBounds = new CalendarRect(right - todayWidth, 0, todayWidth, headerHeight);
                right = TodayButtonBounds.X;
            }

            TitleBounds = new CalendarRect(left, 0, Math.Max(0, right - left), headerHeight);
            y = headerHeight;
        }

        if (weekdayHeaderVisible)
        {
            var weekdayHeight = Math.Min(style.WeekdayHeaderHeight, Math.Max(0, height - y));
            WeekdayHeaderBounds = new CalendarRect(0, y, width, weekdayHeight);
            y += weekdayHeight;
        }

        BodyBounds = new CalendarRect(0, y, width, Math.Max(0, height - y));
    }

    public CalendarRect GetWeekdayHeaderCellBounds(int column) =>
        !WeekdayHeaderVisible || (column < 0) || (column >= DaysPerWeek)
            ? default
            : new CalendarRect(WeekdayHeaderBounds.X + (column * ColumnWidth), WeekdayHeaderBounds.Y, ColumnWidth, WeekdayHeaderBounds.Height);

    public CalendarRect GetWeekBounds(int week) =>
        (week < 0) || (week >= WeekCount)
            ? default
            : new CalendarRect(BodyBounds.X, BodyBounds.Y + (week * WeekHeight), BodyBounds.Width, WeekHeight);

    public CalendarRect GetCellBounds(int week, int column) =>
        (week < 0) || (week >= WeekCount) || (column < 0) || (column >= DaysPerWeek)
            ? default
            : new CalendarRect(BodyBounds.X + (column * ColumnWidth), BodyBounds.Y + (week * WeekHeight), ColumnWidth, WeekHeight);

    // Upper part of the cell that holds the date number and the label; the range background is drawn here
    public CalendarRect GetDateRowBounds(int week, int column)
    {
        var cell = GetCellBounds(week, column);
        return cell.IsEmpty ? default : cell with { Height = Math.Min(Style.DateRowHeight, cell.Height) };
    }

    public CalendarRect GetDateNumberBounds(int week, int column)
    {
        var cell = GetCellBounds(week, column);
        return cell.IsEmpty ? default : new CalendarRect(cell.X + Style.DateNumberMargin, cell.Y + Style.DateNumberMargin, Style.DateNumberSize, Style.DateNumberSize).Intersect(cell);
    }

    // Event row below the date numbers, clipped to the week so rows that do not fit are cut off
    public CalendarRect GetSlotBounds(int week, int slot, int startColumn, int columnSpan)
    {
        var bounds = GetWeekBounds(week);
        if (bounds.IsEmpty || (slot < 0) || (startColumn < 0) || (columnSpan <= 0) || (startColumn + columnSpan > DaysPerWeek))
        {
            return default;
        }

        var rect = new CalendarRect(bounds.X + (startColumn * ColumnWidth), bounds.Y + Style.DateRowHeight + (slot * Style.SlotRowHeight) + 1, columnSpan * ColumnWidth, Style.EventRowHeight);
        return rect.Intersect(bounds);
    }

    public CalendarRect GetEventBounds(int week, CalendarEventPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        return GetSlotBounds(week, placement.Slot, placement.StartColumn, placement.ColumnSpan);
    }

    // Row that shows +N, placed right below the visible slots of the week
    public CalendarRect GetOverflowBounds(int week, int column)
    {
        if ((week < 0) || (week >= WeekCount) || (column < 0) || (column >= DaysPerWeek) || (Month.Weeks[week].Days[column].HiddenEventCount == 0))
        {
            return default;
        }

        return GetSlotBounds(week, Month.Weeks[week].SlotCount, column, 1);
    }

    public CalendarHit HitTest(double x, double y)
    {
        if (!Double.IsFinite(x) || !Double.IsFinite(y) || !new CalendarRect(0, 0, Width, Height).Contains(x, y))
        {
            return CalendarHit.None;
        }

        if (HeaderVisible && HeaderBounds.Contains(x, y))
        {
            if (PrevButtonBounds.Contains(x, y))
            {
                return new CalendarHit(CalendarHitKind.PrevButton);
            }

            if (NextButtonBounds.Contains(x, y))
            {
                return new CalendarHit(CalendarHitKind.NextButton);
            }

            if (TodayButtonBounds.Contains(x, y))
            {
                return new CalendarHit(CalendarHitKind.TodayButton);
            }

            return new CalendarHit(CalendarHitKind.Header);
        }

        if (WeekdayHeaderVisible && WeekdayHeaderBounds.Contains(x, y))
        {
            return new CalendarHit(CalendarHitKind.WeekdayHeader, -1, ToColumn(x));
        }

        if ((WeekCount == 0) || !BodyBounds.Contains(x, y))
        {
            return CalendarHit.None;
        }

        var week = Math.Min(WeekCount - 1, (int)((y - BodyBounds.Y) / WeekHeight));
        var column = ToColumn(x);
        foreach (var placement in Month.Weeks[week].Placements)
        {
            if (GetEventBounds(week, placement).Contains(x, y))
            {
                return new CalendarHit(CalendarHitKind.Event, week, column, placement);
            }
        }

        if (GetOverflowBounds(week, column).Contains(x, y))
        {
            return new CalendarHit(CalendarHitKind.Overflow, week, column);
        }

        return new CalendarHit(CalendarHitKind.Day, week, column);
    }

    private int ToColumn(double x) => Math.Clamp((int)((x - BodyBounds.X) / ColumnWidth), 0, DaysPerWeek - 1);

    private static void RequireDimension(double value, string name)
    {
        if (!Double.IsFinite(value) || (value < 0))
        {
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
