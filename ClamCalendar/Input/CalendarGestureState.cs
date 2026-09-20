namespace ClamCalendar.Input;

internal enum CalendarGestureState
{
    Idle,
    Pressed,
    Moved,
    Swiping,
    Yielded,
    LongPressed,
    Completed,
    Blocked
}
