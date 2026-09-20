namespace ClamCalendar;

public sealed class CalendarOverflowEventArgs(CalendarDay day) : EventArgs
{
    public CalendarDay Day { get; } = day;

    public int HiddenCount => Day.HiddenEventCount;

    // Every event of the day, the hidden ones included
    public IReadOnlyList<CalendarEvent> Events => Day.Events;

    public bool Handled { get; set; }
}
