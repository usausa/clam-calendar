namespace ClamCalendar;

public sealed class CalendarDay
{
    public required DateOnly Date { get; init; }

    public required bool IsCurrentMonth { get; init; }

    public required bool IsToday { get; init; }

    public required CalendarDayKind Kind { get; init; }

    public IReadOnlyList<CalendarStamp> Stamps { get; init; } = [];

    // Small text drawn next to the date number, for example the holiday name
    public string? Label { get; init; }

    // Every event that covers the day, including the ones hidden by MaxVisibleSlots
    public IReadOnlyList<CalendarEvent> Events { get; init; } = [];

    public int HiddenEventCount { get; init; }
}
