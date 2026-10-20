namespace ClamCalendar;

public sealed class CalendarStamp
{
    public string? Key { get; init; }

    public required DateOnly Date { get; init; }

    public required string Glyph { get; init; }

    public CalendarStampPosition Position { get; init; } = CalendarStampPosition.Center;

    public double FontSize { get; init; } = 28;

    public Color? Color { get; init; }

    public double Opacity { get; init; } = 1;

    public object? Tag { get; init; }
}
