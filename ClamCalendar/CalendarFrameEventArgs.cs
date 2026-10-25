namespace ClamCalendar;

public sealed class CalendarFrameEventArgs(double elapsedMilliseconds, int dayCount, int textMeasurements, bool animating) : EventArgs
{
    public double ElapsedMilliseconds { get; } = elapsedMilliseconds;

    public int DayCount { get; } = dayCount;

    public int TextMeasurements { get; } = textMeasurements;

    public bool Animating { get; } = animating;
}
