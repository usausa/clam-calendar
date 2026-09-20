namespace ClamCalendar;

public sealed class CalendarDisplayDateChangedEventArgs(DateOnly displayDate, DateOnly firstDate, DateOnly lastDate) : EventArgs
{
    public DateOnly DisplayDate { get; } = displayDate;

    // First and last date drawn, so the application can load the events of the whole visible range
    public DateOnly FirstDate { get; } = firstDate;

    public DateOnly LastDate { get; } = lastDate;
}
