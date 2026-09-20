namespace ClamCalendar.Input;

// Direction is 1 for a swipe to the left (next month) and -1 for a swipe to the right (previous month)
internal readonly record struct CalendarGestureResult(CalendarGestureAction Action, int Direction = 0);
