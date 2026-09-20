namespace ClamCalendar;

public sealed class CalendarMonth
{
    public required int Year { get; init; }

    public required int Month { get; init; }

    public required DateOnly Today { get; init; }

    public required IReadOnlyList<CalendarWeek> Weeks { get; init; }

    // First and last date drawn, including the days of the neighboring months
    public required DateOnly FirstDate { get; init; }

    public required DateOnly LastDate { get; init; }

    public CalendarDay? FindDay(DateOnly date)
    {
        if ((date < FirstDate) || (date > LastDate))
        {
            return null;
        }

        var offset = date.DayNumber - FirstDate.DayNumber;
        return Weeks[offset / 7].Days[offset % 7];
    }
}
