namespace ClamCalendar;

using ClamCalendar.Platform;

public partial class ClamCalendarView
{
    partial void AttachPlatform()
    {
        if (!disposed && (Handler?.PlatformView is Android.Views.View view))
        {
            platform = new CalendarPlatformBridge(view, () => CancelInput(true));
        }
    }
}
