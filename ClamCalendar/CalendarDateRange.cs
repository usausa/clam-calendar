namespace ClamCalendar;

public readonly record struct CalendarDateRange(DateOnly Start, DateOnly End)
{
    public int Days => Math.Max(0, (End.DayNumber - Start.DayNumber) + 1);

    public bool Contains(DateOnly date) => (date >= Start) && (date <= End);
}
