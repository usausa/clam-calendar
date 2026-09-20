namespace ClamCalendar;

using System.Diagnostics;
using System.Windows.Input;

using ClamCalendar.Input;
using ClamCalendar.Layout;

using Microsoft.Maui.Dispatching;

using SkiaSharp.Views.Maui;

public partial class ClamCalendarView
{
    public static readonly BindableProperty DayTappedCommandProperty = BindableProperty.Create(nameof(DayTappedCommand), typeof(ICommand), typeof(ClamCalendarView));
    public static readonly BindableProperty DayLongPressedCommandProperty = BindableProperty.Create(nameof(DayLongPressedCommand), typeof(ICommand), typeof(ClamCalendarView));
    public static readonly BindableProperty EventTappedCommandProperty = BindableProperty.Create(nameof(EventTappedCommand), typeof(ICommand), typeof(ClamCalendarView));
    public static readonly BindableProperty OverflowTappedCommandProperty = BindableProperty.Create(nameof(OverflowTappedCommand), typeof(ICommand), typeof(ClamCalendarView));

    private const int InputTickMilliseconds = 50;

    private readonly CalendarGestureController gesture = new();
    private IDispatcherTimer? inputTimer;
    private CalendarHit pressHit;
    private long inputGeneration;

    public event EventHandler<CalendarDayEventArgs>? DayTapped;

    public event EventHandler<CalendarDayEventArgs>? DayLongPressed;

    public event EventHandler<CalendarEventEventArgs>? EventTapped;

    public event EventHandler<CalendarOverflowEventArgs>? OverflowTapped;

    public ICommand? DayTappedCommand
    {
        get => (ICommand?)GetValue(DayTappedCommandProperty);
        set => SetValue(DayTappedCommandProperty, value);
    }

    public ICommand? DayLongPressedCommand
    {
        get => (ICommand?)GetValue(DayLongPressedCommandProperty);
        set => SetValue(DayLongPressedCommandProperty, value);
    }

    public ICommand? EventTappedCommand
    {
        get => (ICommand?)GetValue(EventTappedCommandProperty);
        set => SetValue(EventTappedCommandProperty, value);
    }

    public ICommand? OverflowTappedCommand
    {
        get => (ICommand?)GetValue(OverflowTappedCommandProperty);
        set => SetValue(OverflowTappedCommandProperty, value);
    }

    private static double InputTime => Stopwatch.GetTimestamp() * 1000d / Stopwatch.Frequency;

    public void CancelInteraction()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        CancelInput();
    }

    private void OnCalendarTouch(object? sender, SKTouchEventArgs e)
    {
        if (disposed || !IsEnabled || (Width <= 0) || (Height <= 0) || (CanvasSize.Width <= 0) || (CanvasSize.Height <= 0))
        {
            return;
        }

        var point = new Point(e.Location.X * Width / CanvasSize.Width, e.Location.Y * Height / CanvasSize.Height);
        e.Handled = true;
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                if (gesture.State == CalendarGestureState.Idle)
                {
                    pressHit = HitTest(point.X, point.Y);
                }

                if (gesture.Press(e.Id, point, InputTime, SwipeEnabled).Action == CalendarGestureAction.Canceled)
                {
                    CancelInput();
                }
                else
                {
                    StartInputTimer();
                    SetParentIntercept(false);
                }

                break;
            case SKTouchAction.Moved:
                ApplyMovement(gesture.Move(e.Id, point));
                break;
            case SKTouchAction.Released:
                var inside = new CalendarRect(0, 0, Width, Height).Contains(point.X, point.Y);
                var result = gesture.Release(e.Id, point, inside && (HitTest(point.X, point.Y) == pressHit));
                StopInputTimer();
                SetParentIntercept(true);
                if (result.Action == CalendarGestureAction.Tap)
                {
                    Activate(pressHit, false);
                }
                else if (result.Action == CalendarGestureAction.Swipe)
                {
                    Swipe(result.Direction);
                }

                break;
            case SKTouchAction.Cancelled:
                CancelInput(true);
                break;
        }
    }

    private void ApplyMovement(CalendarGestureResult result)
    {
        if (result.Action == CalendarGestureAction.Swipe)
        {
            StopInputTimer();
            SetParentIntercept(true);
            Swipe(result.Direction);
        }
        else if (result.Action == CalendarGestureAction.Yield)
        {
            StopInputTimer();
            SetParentIntercept(true);
        }
    }

    private void Swipe(int direction)
    {
        if (SwipeEnabled && CanNavigate(direction))
        {
            Navigate(direction);
        }
    }

    private void Activate(CalendarHit hit, bool longPress)
    {
        if (disposed || !IsEnabled)
        {
            return;
        }

        switch (hit.Kind)
        {
            case CalendarHitKind.PrevButton when !longPress:
                Navigate(-1);
                break;
            case CalendarHitKind.NextButton when !longPress:
                Navigate(1);
                break;
            case CalendarHitKind.TodayButton when !longPress:
                GoToToday();
                break;
            case CalendarHitKind.Day:
            case CalendarHitKind.Event:
            case CalendarHitKind.Overflow:
                ActivateDay(hit, longPress);
                break;
        }
    }

    private void ActivateDay(CalendarHit hit, bool longPress)
    {
        if ((hit.Week < 0) || (hit.Week >= Month.Weeks.Count) || (hit.Column < 0) || (hit.Column >= CalendarLayout.DaysPerWeek))
        {
            return;
        }

        var day = Month.Weeks[hit.Week].Days[hit.Column];
        if ((!day.IsCurrentMonth && !OutsideMonthDaysVisible) || IsDateDisabled(day.Date))
        {
            return;
        }

        var generation = inputGeneration;
        if (longPress)
        {
            Raise(DayLongPressed, DayLongPressedCommand, new CalendarDayEventArgs(day), generation);
        }
        else if ((hit.Kind == CalendarHitKind.Event) && (hit.Placement is { } placement))
        {
            Raise(EventTapped, EventTappedCommand, new CalendarEventEventArgs(placement.Event, day), generation);
        }
        else if (hit.Kind == CalendarHitKind.Overflow)
        {
            Raise(OverflowTapped, OverflowTappedCommand, new CalendarOverflowEventArgs(day), generation);
        }
        else
        {
            SelectDay(day.Date);
            Raise(DayTapped, DayTappedCommand, new CalendarDayEventArgs(day), generation);
        }
    }

    private void Raise<T>(EventHandler<T>? handler, ICommand? command, T args, long generation)
        where T : EventArgs
    {
        handler?.Invoke(this, args);
        if ((generation != inputGeneration) || IsHandled(args))
        {
            return;
        }

        if ((command?.CanExecute(args) ?? false) && (generation == inputGeneration))
        {
            command.Execute(args);
        }
    }

    private static bool IsHandled(EventArgs args) => args switch
    {
        CalendarDayEventArgs day => day.Handled,
        CalendarEventEventArgs calendarEvent => calendarEvent.Handled,
        CalendarOverflowEventArgs overflow => overflow.Handled,
        _ => false
    };

    private void StartInputTimer()
    {
        if (inputTimer is null)
        {
            inputTimer = Dispatcher.CreateTimer();
            inputTimer.Interval = TimeSpan.FromMilliseconds(InputTickMilliseconds);
            inputTimer.Tick += OnInputTick;
        }

        inputTimer.Start();
    }

    private void OnInputTick(object? sender, EventArgs e)
    {
        if (disposed || (Handler is null) || !IsVisible || !IsEnabled)
        {
            CancelInput();
            return;
        }

        var result = gesture.Tick(InputTime);
        if (result.Action == CalendarGestureAction.LongPress)
        {
            StopInputTimer();
            Activate(pressHit, true);
        }
        else if (gesture.State != CalendarGestureState.Pressed)
        {
            StopInputTimer();
        }
    }

    private void StopInputTimer() => inputTimer?.Stop();

    internal void CancelInput(bool releasePointers = false)
    {
        inputGeneration++;
        if (releasePointers)
        {
            gesture.Cancel();
        }
        else
        {
            gesture.Block();
        }

        StopInputTimer();
        SetParentIntercept(true);
    }
}
