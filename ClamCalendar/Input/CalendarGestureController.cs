namespace ClamCalendar.Input;

// Classifies one pointer as tap, long press or horizontal swipe; vertical movement yields the gesture to the parent
internal sealed class CalendarGestureController
{
    public const double MovementThreshold = 8;
    public const double SwipeThreshold = 36;
    public const double FlickThreshold = 20;
    public const double HorizontalBias = 1.25;
    public const double LongPressMilliseconds = 500;

    private readonly HashSet<long> pointers = [];
    private long primary;
    private Point origin;
    private Point previous;
    private double pressedAt;
    private bool swipeEnabled;

    public CalendarGestureState State { get; private set; }

    public CalendarGestureResult Press(long id, Point point, double milliseconds, bool swipe = true)
    {
        if (!pointers.Add(id) || (pointers.Count > 1))
        {
            Block();
            return new CalendarGestureResult(CalendarGestureAction.Canceled);
        }

        primary = id;
        origin = point;
        previous = point;
        pressedAt = milliseconds;
        swipeEnabled = swipe;
        State = CalendarGestureState.Pressed;
        return default;
    }

    public CalendarGestureResult Move(long id, Point point)
    {
        if ((id != primary) || !pointers.Contains(id) || (State is not (CalendarGestureState.Pressed or CalendarGestureState.Moved or CalendarGestureState.Swiping)))
        {
            return default;
        }

        var totalX = point.X - origin.X;
        var totalY = point.Y - origin.Y;
        var stepX = point.X - previous.X;
        previous = point;
        var absX = Math.Abs(totalX);
        var absY = Math.Abs(totalY);
        if (State != CalendarGestureState.Swiping)
        {
            if ((absX < MovementThreshold) && (absY < MovementThreshold))
            {
                return default;
            }

            if (absX >= absY * HorizontalBias)
            {
                if (!swipeEnabled)
                {
                    State = CalendarGestureState.Yielded;
                    return new CalendarGestureResult(CalendarGestureAction.Yield);
                }

                State = CalendarGestureState.Swiping;
            }
            else if (absY >= absX * HorizontalBias)
            {
                State = CalendarGestureState.Yielded;
                return new CalendarGestureResult(CalendarGestureAction.Yield);
            }
            else
            {
                State = CalendarGestureState.Moved;
                return default;
            }
        }

        if ((absX >= SwipeThreshold) || (Math.Abs(stepX) >= FlickThreshold))
        {
            State = CalendarGestureState.Completed;
            return new CalendarGestureResult(CalendarGestureAction.Swipe, totalX < 0 ? 1 : -1);
        }

        return default;
    }

    public CalendarGestureResult Tick(double milliseconds)
    {
        if ((State != CalendarGestureState.Pressed) || ((milliseconds - pressedAt) < LongPressMilliseconds))
        {
            return default;
        }

        State = CalendarGestureState.LongPressed;
        return new CalendarGestureResult(CalendarGestureAction.LongPress);
    }

    public CalendarGestureResult Release(long id, Point point, bool inside)
    {
        if (!pointers.Contains(id))
        {
            return default;
        }

        var result = default(CalendarGestureResult);
        if ((id == primary) && (State != CalendarGestureState.Blocked))
        {
            var moved = Move(id, point);
            if (moved.Action == CalendarGestureAction.Swipe)
            {
                result = moved;
            }
            else if ((State == CalendarGestureState.Pressed) && inside)
            {
                result = new CalendarGestureResult(CalendarGestureAction.Tap);
            }
        }

        pointers.Remove(id);
        if (pointers.Count == 0)
        {
            Cancel();
        }

        return result;
    }

    public void Block()
    {
        State = pointers.Count > 0 ? CalendarGestureState.Blocked : CalendarGestureState.Idle;
    }

    public void Cancel()
    {
        pointers.Clear();
        State = CalendarGestureState.Idle;
    }
}
