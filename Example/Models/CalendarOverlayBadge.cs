namespace Example.Models;

public sealed record CalendarOverlayBadge(DateOnly OverlayDate, string Text) : ICalendarOverlayItem;
