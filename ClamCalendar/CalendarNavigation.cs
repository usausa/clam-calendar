namespace ClamCalendar;

using ClamCalendar.Layout;

// Month arithmetic of ClamCalendarView kept free of bindable properties so it can be unit tested
internal static class CalendarNavigation
{
    private const int DaysPerWeek = CalendarEventPlacer.DaysPerWeek;

    public static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public static int MonthIndex(DateOnly date) => (date.Year * 12) + date.Month;

    // First day of the month that is months away, or null when the calendar cannot represent it
    public static DateOnly? OffsetMonth(DateOnly date, int months)
    {
        var index = MonthIndex(date) - 1 + months;
        if ((index < MonthIndex(DateOnly.MinValue) - 1) || (index > MonthIndex(DateOnly.MaxValue) - 1))
        {
            return null;
        }

        return new DateOnly(index / 12, (index % 12) + 1, 1);
    }

    // First day of the nearest month inside the limits
    public static DateOnly ClampMonth(DateOnly date, DateOnly? min, DateOnly? max)
    {
        var first = FirstOfMonth(date);
        if ((min is { } lower) && (first < FirstOfMonth(lower)))
        {
            return FirstOfMonth(lower);
        }

        if ((max is { } upper) && (first > FirstOfMonth(upper)))
        {
            return FirstOfMonth(upper);
        }

        return first;
    }

    public static bool IsMonthWithinLimits(DateOnly date, DateOnly? min, DateOnly? max) =>
        ClampMonth(date, min, max) == FirstOfMonth(date);

    public static (int Week, int Column)? LocatePosition(CalendarDateRange range, DateOnly date)
    {
        if (!range.Contains(date))
        {
            return null;
        }

        var offset = date.DayNumber - range.Start.DayNumber;
        return (offset / DaysPerWeek, offset % DaysPerWeek);
    }

    public static TimeSpan NextDateChangeDelay(DateTime now) =>
        (TimeSpan.FromDays(1) - now.TimeOfDay) + TimeSpan.FromSeconds(1);
}
