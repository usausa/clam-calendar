namespace ClamCalendar;

// Position of an event inside one week row: columns are days from the first day of the week and the slot is the row below the date numbers
public sealed class CalendarEventPlacement
{
    public required CalendarEvent Event { get; init; }

    public required int StartColumn { get; init; }

    public required int ColumnSpan { get; init; }

    public required int Slot { get; init; }

    public bool ContinuesFromPreviousWeek { get; init; }

    public bool ContinuesToNextWeek { get; init; }

    public int EndColumn => (StartColumn + ColumnSpan) - 1;
}
