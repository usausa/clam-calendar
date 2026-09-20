namespace ClamCalendar.Platform;

internal interface ICalendarPlatformBridge : IDisposable
{
    void SetParentIntercept(bool allow);
}
