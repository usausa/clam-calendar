namespace ClamCalendar.Platform;

using Android.Views;

internal sealed class CalendarPlatformBridge : ICalendarPlatformBridge
{
    private readonly View view;
    private readonly Action cancel;
    private ViewTreeObserver? windowObserver;

    public CalendarPlatformBridge(View view, Action cancel)
    {
        this.view = view;
        this.cancel = cancel;
        view.ViewAttachedToWindow += OnAttached;
        view.ViewDetachedFromWindow += OnDetached;
        if (view.IsAttachedToWindow)
        {
            AttachWindowObserver();
        }
    }

    void ICalendarPlatformBridge.SetParentIntercept(bool allow) => view.Parent?.RequestDisallowInterceptTouchEvent(!allow);

    public void Dispose()
    {
        DetachWindowObserver();
        view.ViewAttachedToWindow -= OnAttached;
        view.ViewDetachedFromWindow -= OnDetached;
    }

    private void OnAttached(object? sender, View.ViewAttachedToWindowEventArgs e) => AttachWindowObserver();

    private void OnDetached(object? sender, View.ViewDetachedFromWindowEventArgs e)
    {
        DetachWindowObserver();
        cancel();
    }

    private void AttachWindowObserver()
    {
        DetachWindowObserver();
        if (view.ViewTreeObserver is { IsAlive: true } observer)
        {
            windowObserver = observer;
            observer.WindowFocusChange += OnWindowFocusChange;
        }
    }

    private void DetachWindowObserver()
    {
        // Detaching the view swaps its ViewTreeObserver, so unsubscribe from the observer that was subscribed
        if (windowObserver is { IsAlive: true } observer)
        {
            observer.WindowFocusChange -= OnWindowFocusChange;
        }

        windowObserver = null;
    }

    private void OnWindowFocusChange(object? sender, ViewTreeObserver.WindowFocusChangeEventArgs e)
    {
        if (!e.HasFocus)
        {
            cancel();
        }
    }
}
