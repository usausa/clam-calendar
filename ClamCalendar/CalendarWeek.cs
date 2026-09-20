namespace ClamCalendar;

public sealed class CalendarWeek
{
    public required IReadOnlyList<CalendarDay> Days { get; init; }

    // Visible placements only; hidden events are counted on the days
    public required IReadOnlyList<CalendarEventPlacement> Placements { get; init; }

    public required int SlotCount { get; init; }
}
