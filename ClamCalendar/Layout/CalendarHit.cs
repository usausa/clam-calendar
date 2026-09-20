namespace ClamCalendar.Layout;

internal enum CalendarHitKind
{
    None,
    Header,
    PrevButton,
    NextButton,
    TodayButton,
    WeekdayHeader,
    Day,
    Event,
    Overflow
}

internal readonly record struct CalendarHit(CalendarHitKind Kind, int Week = -1, int Column = -1, CalendarEventPlacement? Placement = null)
{
    public static CalendarHit None => new(CalendarHitKind.None);
}
