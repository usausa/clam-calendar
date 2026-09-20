namespace ClamCalendar;

public sealed class CalendarEvent
{
    public string? Key { get; init; }

    public required string Title { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public CalendarEventStyle Style { get; init; } = CalendarEventStyle.Filled;

    public Color BackgroundColor { get; init; } = Colors.LightGray;

    public Color TextColor { get; init; } = Colors.White;

    // Drawn before the title, for example an emoji
    public string? LeadingGlyph { get; init; }

    // Drawn before the title as "H:mm" when set
    public TimeSpan? StartTime { get; init; }

    public object? Tag { get; init; }

    public int DurationDays => Math.Max(1, (EndDate.DayNumber - StartDate.DayNumber) + 1);

    public bool IsMultiDay => DurationDays > 1;
}
