namespace ClamCalendar;

public sealed record CalendarDayAppearance
{
    public Color? Background { get; init; }

    public Color? DateNumberColor { get; init; }

    public Color? DateNumberBackground { get; init; }

    public Color? LabelColor { get; init; }

    public Color? BorderColor { get; init; }

    public double? BorderWidth { get; init; }
}
