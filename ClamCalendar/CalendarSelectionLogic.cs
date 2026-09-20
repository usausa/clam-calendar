namespace ClamCalendar;

// Selection rules of ClamCalendarView kept free of bindable properties so they can be unit tested
internal static class CalendarSelectionLogic
{
    public static bool IsDisabled(DateOnly date, DateOnly? min, DateOnly? max, IReadOnlySet<DateOnly>? disabled) =>
        ((min is { } lower) && (date < lower)) || ((max is { } upper) && (date > upper)) || ((disabled is not null) && disabled.Contains(date));

    // The same date again clears the selection when deselecting is allowed
    public static DateOnly? TapSingle(DateOnly? current, DateOnly date, bool allowDeselect) =>
        current == date ? (allowDeselect ? null : date) : date;

    // Toggles the date; adding stops at the limit
    public static bool TapMultiple(IList<DateOnly> dates, DateOnly date, int maxSelectableDays)
    {
        ArgumentNullException.ThrowIfNull(dates);
        if (dates.Remove(date))
        {
            return true;
        }

        if ((maxSelectableDays > 0) && (dates.Count >= maxSelectableDays))
        {
            return false;
        }

        dates.Add(date);
        return true;
    }

    // The first tap starts a range, the second completes it in either order; a range that is too long or crosses a disabled date starts over at the tapped date
    public static (DateOnly? Start, DateOnly? End) TapRange(DateOnly? start, DateOnly? end, DateOnly date, int maxSelectableDays, Func<DateOnly, bool> isDisabled)
    {
        ArgumentNullException.ThrowIfNull(isDisabled);
        if ((start is not { } first) || (end is not null))
        {
            return (date, null);
        }

        var (rangeStart, rangeEnd) = Normalize(first, date);
        var days = (rangeEnd.DayNumber - rangeStart.DayNumber) + 1;
        if ((maxSelectableDays > 0) && (days > maxSelectableDays))
        {
            return (date, null);
        }

        for (var current = rangeStart; current <= rangeEnd; current = current.AddDays(1))
        {
            if (isDisabled(current))
            {
                return (date, null);
            }
        }

        return (rangeStart, rangeEnd);
    }

    public static (DateOnly Start, DateOnly End) Normalize(DateOnly start, DateOnly end) =>
        start <= end ? (start, end) : (end, start);

    // Inclusive of both ends. A range with only its start tapped selects that day so the first tap is visible
    public static bool IsInRange(DateOnly date, DateOnly? start, DateOnly? end)
    {
        if (start is not { } first)
        {
            return false;
        }

        if (end is not { } last)
        {
            return date == first;
        }

        var (rangeStart, rangeEnd) = Normalize(first, last);
        return (date >= rangeStart) && (date <= rangeEnd);
    }
}
