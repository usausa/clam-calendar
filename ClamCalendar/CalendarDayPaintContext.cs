namespace ClamCalendar;

using SkiaSharp;

public readonly record struct CalendarDayPaintContext(SKRect Bounds, SKRect DateNumberBounds, CalendarDay Day, bool IsSelected, bool IsDisabled, Color TextColor);
