namespace ClamCalendar;

public sealed class CalendarDayEventArgs(CalendarDay day) : EventArgs
{
    public CalendarDay Day { get; } = day;

    public DateOnly Date => Day.Date;

    public bool Handled { get; set; }
}
