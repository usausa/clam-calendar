namespace ClamCalendar;

public sealed class CalendarEventEventArgs(CalendarEvent calendarEvent, CalendarDay day) : EventArgs
{
    public CalendarEvent Event { get; } = calendarEvent;

    // Day column under the tap
    public CalendarDay Day { get; } = day;

    public bool Handled { get; set; }
}
