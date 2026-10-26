namespace ClamCalendar.Rendering;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using ClamCalendar.Layout;

using SkiaExtension;

using SkiaSharp;
using SkiaSharp.Views.Maui;

internal sealed class CalendarRenderer : IDisposable
{
    private readonly CalendarStyle style;
    private readonly FontResolver fonts;
    private readonly SKPaint paint = new() { IsAntialias = true };
    private readonly SKPaint fillPaint = new() { IsAntialias = false };
    private readonly Dictionary<(DayOfWeek DayOfWeek, CultureInfo Culture, CalendarWeekdayNameFormat Format), string> weekdayNames = [];
    private readonly ConditionalWeakTable<CalendarEvent, string> eventTexts = [];
    private readonly Dictionary<int, string> overflowTexts = [];
    private readonly Dictionary<int, string> weekNumberTexts = [];
    private (int Year, int Month, CultureInfo? Culture, CalendarTextFormat? Format, Func<CalendarMonth, string>? Formatter) titleKey;
    private string title = String.Empty;

    public int Measurements => fonts.Measurements;

    public CalendarRenderer(CalendarStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        style.Validate();
        this.style = style;
        fonts = new FontResolver(style.FontFamily);
    }

    public void Dispose()
    {
        paint.Dispose();
        fillPaint.Dispose();
        fonts.Dispose();
    }

    public static string FormatTitle(CalendarMonth month, CultureInfo culture, CalendarTextFormat format, Func<CalendarMonth, string>? formatter = null)
    {
        ArgumentNullException.ThrowIfNull(month);
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(format);
        if (formatter is not null)
        {
            return formatter(month);
        }

        var date = new DateTime(month.Year, month.Month, 1);
        return format.IsDefault ? date.ToString(culture.DateTimeFormat.YearMonthPattern, culture) : format.Format(date, culture);
    }

    public static string FormatWeekdayName(DayOfWeek dayOfWeek, CultureInfo culture, CalendarWeekdayNameFormat format)
    {
        ArgumentNullException.ThrowIfNull(culture);
        switch (format)
        {
            case CalendarWeekdayNameFormat.Full:
                return culture.DateTimeFormat.GetDayName(dayOfWeek);
            case CalendarWeekdayNameFormat.Abbreviated:
                return culture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek);
            default:
                var abbreviated = culture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek);
                return abbreviated.Length > 0 ? StringInfo.GetNextTextElement(abbreviated).ToUpper(culture) : abbreviated;
        }
    }

    public static int GetWeekNumber(CalendarWeek week)
    {
        var days = week.Days;
        var offset = ((int)DayOfWeek.Thursday - (int)days[0].Date.DayOfWeek + CalendarLayout.DaysPerWeek) % CalendarLayout.DaysPerWeek;
        return ISOWeek.GetWeekOfYear(days[offset].Date.ToDateTime(TimeOnly.MinValue));
    }

    // Time, glyph and title separated by spaces
    public static string FormatEventText(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);
        if ((calendarEvent.StartTime is null) && String.IsNullOrEmpty(calendarEvent.LeadingGlyph))
        {
            return calendarEvent.Title;
        }

        var builder = new StringBuilder();
        if (calendarEvent.StartTime is { } time)
        {
            builder.Append(CultureInfo.InvariantCulture, $"{(int)time.TotalHours}:{time.Minutes:00}");
            builder.Append(' ');
        }

        if (!String.IsNullOrEmpty(calendarEvent.LeadingGlyph))
        {
            builder.Append(calendarEvent.LeadingGlyph);
            builder.Append(' ');
        }

        builder.Append(calendarEvent.Title);
        return builder.ToString();
    }

    public void Render(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(state);
        canvas.Clear(style.Background.ToSKColor());
        if (layout.HeaderVisible)
        {
            RenderHeader(canvas, layout, state);
        }

        if (layout.WeekdayHeaderVisible)
        {
            RenderWeekdayHeader(canvas, layout, state);
        }

        RenderBody(canvas, layout, state);
    }

    private void RenderHeader(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state)
    {
        var bounds = layout.HeaderBounds;
        if (bounds.IsEmpty)
        {
            return;
        }

        Fill(canvas, bounds, style.HeaderBackground);
        if (layout.NavigationButtonsVisible)
        {
            DrawText(canvas, layout.PrevButtonBounds, state.PrevButtonText, TextAlignment.Center, style.HeaderFontSize, false, state.CanNavigateBackward ? style.NavigationButtonColor : style.DisabledDayTextColor);
            DrawText(canvas, layout.NextButtonBounds, state.NextButtonText, TextAlignment.Center, style.HeaderFontSize, false, state.CanNavigateForward ? style.NavigationButtonColor : style.DisabledDayTextColor);
        }

        DrawText(canvas, layout.TitleBounds, GetTitle(layout.Month, state.Culture, state.HeaderFormat, state.HeaderFormatter), TextAlignment.Center, style.HeaderFontSize, true, style.HeaderTextColor, style.EventPadding);
        if (layout.TodayButtonVisible)
        {
            Fill(canvas, layout.TodayButtonBounds, style.HeaderBackground);
            DrawText(canvas, layout.TodayButtonBounds, state.TodayButtonText, TextAlignment.Center, style.DateNumberFontSize, false, style.NavigationButtonColor, style.EventPadding);
        }

        DrawHorizontalLine(canvas, bounds.X, bounds.Right, bounds.Bottom);
    }

    private void RenderWeekdayHeader(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state)
    {
        var bounds = layout.WeekdayHeaderBounds;
        if (bounds.IsEmpty || (layout.WeekCount == 0))
        {
            return;
        }

        Fill(canvas, bounds, style.WeekdayHeaderBackground);
        var days = layout.Month.Weeks[0].Days;
        for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
        {
            var dayOfWeek = days[column].Date.DayOfWeek;
            var color = dayOfWeek switch
            {
                DayOfWeek.Saturday => style.SaturdayHeaderTextColor,
                DayOfWeek.Sunday => style.SundayHeaderTextColor,
                _ => style.WeekdayHeaderTextColor
            };
            DrawText(canvas, layout.GetWeekdayHeaderCellBounds(column), GetWeekdayName(dayOfWeek, state.Culture, state.WeekdayNameFormat), TextAlignment.Center, style.WeekdayHeaderFontSize, false, color, 1);
        }

        DrawHorizontalLine(canvas, bounds.X, bounds.Right, bounds.Bottom);
    }

    private void RenderBody(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state)
    {
        var body = layout.BodyBounds;
        if (body.IsEmpty || (layout.WeekCount == 0))
        {
            return;
        }

        var month = layout.Month;
        var horizontalLines = style.GridLineMode is CalendarGridLineMode.All or CalendarGridLineMode.Horizontal;
        var verticalLines = style.GridLineMode is CalendarGridLineMode.All or CalendarGridLineMode.Vertical;
        canvas.Save();
        canvas.ClipRect(ToSkRect(body));
        var layered = state.SlideOpacity < 1;
        if (layered)
        {
            using var layerPaint = new SKPaint();
            layerPaint.Color = SKColors.Black.WithOpacity(state.SlideOpacity);
            canvas.SaveLayer(layerPaint);
        }

        canvas.Translate(state.SlideOffset, 0);
        if (state.MonthIndicatorVisible)
        {
            DrawText(canvas, layout.DayAreaBounds, month.Month.ToString(CultureInfo.InvariantCulture), TextAlignment.Center, style.MonthIndicatorFontSize, true, style.MonthIndicatorColor);
        }

        for (var w = 0; w < layout.WeekCount; w++)
        {
            var week = month.Weeks[w];
            var weekBounds = layout.GetWeekBounds(w);
            for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
            {
                RenderCellBackground(canvas, layout, state, week.Days[column], layout.GetCellBounds(w, column), w, column);
            }

            // Grid lines go over the cell backgrounds so weekend and holiday fills do not cover them
            if (horizontalLines)
            {
                DrawHorizontalLine(canvas, weekBounds.X, weekBounds.Right, weekBounds.Y);
            }

            if (verticalLines)
            {
                if (layout.WeekNumberWidth > 0)
                {
                    DrawVerticalLine(canvas, layout.DayAreaBounds.X, weekBounds.Y, weekBounds.Bottom);
                }

                for (var column = 0; column < CalendarLayout.DaysPerWeek - 1; column++)
                {
                    var cell = layout.GetCellBounds(w, column);
                    DrawVerticalLine(canvas, cell.Right, cell.Y, cell.Bottom);
                }
            }

            if (layout.WeekNumberVisible)
            {
                var numberBounds = layout.GetWeekNumberBounds(w);
                DrawText(canvas, numberBounds with { Height = Math.Min(style.DateRowHeight, numberBounds.Height) }, GetWeekNumberText(GetWeekNumber(week)), TextAlignment.Center, style.WeekNumberFontSize, false, style.WeekNumberTextColor);
            }

            for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
            {
                var day = week.Days[column];
                if (!day.IsCurrentMonth && !state.OutsideMonthDaysVisible)
                {
                    continue;
                }

                RenderDayBorder(canvas, day, layout.GetCellBounds(w, column));
                RenderDateNumber(canvas, layout, state, day, w, column);
                RenderStamps(canvas, day, layout.GetCellBounds(w, column));
            }

            RenderEvents(canvas, layout, week, w, weekBounds);
        }

        if (horizontalLines)
        {
            DrawHorizontalLine(canvas, body.X, body.Right, body.Y + (layout.WeekHeight * layout.WeekCount));
        }

        if (state.DayPainter is { } painter)
        {
            PaintDays(canvas, layout, state, painter);
        }

        if (layered)
        {
            canvas.Restore();
        }

        canvas.Restore();
    }

    private void RenderCellBackground(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state, CalendarDay day, CalendarRect cell, int week, int column)
    {
        var background = day.Appearance?.Background ?? (!day.IsCurrentMonth
            ? style.OutsideMonthBackground
            : day.Kind switch
            {
                CalendarDayKind.Holiday => style.HolidayBackground,
                CalendarDayKind.Saturday or CalendarDayKind.Sunday => style.WeekendBackground,
                _ => null
            });
        if (background is not null)
        {
            Fill(canvas, cell, background);
        }

        if ((state.IsInRange?.Invoke(day.Date) ?? false) && (day.IsCurrentMonth || state.OutsideMonthDaysVisible))
        {
            Fill(canvas, layout.GetDateRowBounds(week, column), style.RangeBackground);
        }
    }

    private void RenderDayBorder(SKCanvas canvas, CalendarDay day, CalendarRect cell)
    {
        if ((day.Appearance is not { BorderColor: { } color, BorderWidth: { } width }) || !Double.IsFinite(width) || (width <= 0) || cell.IsEmpty)
        {
            return;
        }

        var inset = width / 2;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = (float)width;
        paint.Color = color.ToSKColor();
        canvas.DrawRect(ToSkRect(new CalendarRect(cell.X + inset, cell.Y + inset, cell.Width - width, cell.Height - width)), paint);
    }

    private void RenderDateNumber(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state, CalendarDay day, int week, int column)
    {
        var bubble = layout.GetDateNumberBounds(week, column);
        if (bubble.IsEmpty)
        {
            return;
        }

        var disabled = state.IsDisabled?.Invoke(day.Date) ?? false;
        var selected = !disabled && (state.IsSelected?.Invoke(day.Date) ?? false);
        var (textColor, markColor, markStyle) = ResolveDateNumber(day, selected, disabled);
        if (markColor is not null)
        {
            DrawDateNumberMark(canvas, bubble, markColor, markStyle);
        }

        if (selected && day.IsToday && style.TodayMarkOnSelected)
        {
            DrawDateNumberMark(canvas, bubble, style.TodayBackground, style.TodayMarkStyle);
        }

        DrawText(canvas, bubble, day.Date.Day.ToString(CultureInfo.InvariantCulture), TextAlignment.Center, style.DateNumberFontSize, style.DateNumberBold, textColor);
        if (!String.IsNullOrEmpty(day.Label))
        {
            var cell = layout.GetCellBounds(week, column);
            var labelBounds = new CalendarRect(bubble.Right + 2, bubble.Y, cell.Right - bubble.Right - 4, bubble.Height);
            DrawText(canvas, labelBounds, day.Label, TextAlignment.Start, style.DayLabelFontSize, false, day.Appearance?.LabelColor ?? (day.IsCurrentMonth ? style.DayLabelTextColor : style.OutsideMonthTextColor));
        }
    }

    private (Color TextColor, Color? MarkColor, CalendarMarkStyle MarkStyle) ResolveDateNumber(CalendarDay day, bool selected, bool disabled)
    {
        if (disabled)
        {
            return (style.DisabledDayTextColor, null, CalendarMarkStyle.Fill);
        }

        if (selected)
        {
            return (style.SelectedDayTextColor, style.SelectedDayBackground, style.SelectedMarkStyle);
        }

        if (day.IsToday)
        {
            return (style.TodayTextColor, style.TodayBackground, style.TodayMarkStyle);
        }

        return (day.Appearance?.DateNumberColor ?? GetDateTextColor(day), day.Appearance?.DateNumberBackground, CalendarMarkStyle.Fill);
    }

    private void PaintDays(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state, Action<SKCanvas, CalendarDayPaintContext> painter)
    {
        var month = layout.Month;
        for (var w = 0; w < layout.WeekCount; w++)
        {
            var week = month.Weeks[w];
            for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
            {
                var day = week.Days[column];
                var cell = layout.GetCellBounds(w, column);
                if ((!day.IsCurrentMonth && !state.OutsideMonthDaysVisible) || cell.IsEmpty)
                {
                    continue;
                }

                var disabled = state.IsDisabled?.Invoke(day.Date) ?? false;
                var selected = !disabled && (state.IsSelected?.Invoke(day.Date) ?? false);
                var context = new CalendarDayPaintContext(ToSkRect(cell), ToSkRect(layout.GetDateNumberBounds(w, column)), day, selected, disabled, ResolveDateNumber(day, selected, disabled).TextColor);
                var count = canvas.Save();
                canvas.ClipRect(context.Bounds);
                painter(canvas, context);
                canvas.RestoreToCount(count);
            }
        }
    }

    private void DrawDateNumberMark(SKCanvas canvas, CalendarRect bubble, Color color, CalendarMarkStyle markStyle)
    {
        switch (markStyle)
        {
            case CalendarMarkStyle.Fill:
                paint.Style = SKPaintStyle.Fill;
                paint.Color = color.ToSKColor();
                DrawDateNumberShape(canvas, bubble);
                break;
            case CalendarMarkStyle.Outline:
            {
                var width = style.DateNumberMarkWidth;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = (float)width;
                paint.Color = color.ToSKColor();
                DrawDateNumberShape(canvas, new CalendarRect(bubble.X + (width / 2), bubble.Y + (width / 2), bubble.Width - width, bubble.Height - width));
                break;
            }

            case CalendarMarkStyle.Underline:
                paint.Style = SKPaintStyle.Fill;
                paint.Color = color.ToSKColor();
                canvas.DrawRect(ToSkRect(new CalendarRect(bubble.X, bubble.Bottom - style.DateNumberMarkWidth, bubble.Width, style.DateNumberMarkWidth)), paint);
                break;
            case CalendarMarkStyle.None:
            default:
                break;
        }
    }

    private void DrawDateNumberShape(SKCanvas canvas, CalendarRect rect)
    {
        switch (style.DateNumberShape)
        {
            case CalendarDateNumberShape.Circle:
                canvas.DrawCircle((float)rect.CenterX, (float)rect.CenterY, (float)(Math.Min(rect.Width, rect.Height) / 2), paint);
                break;
            case CalendarDateNumberShape.Square:
                canvas.DrawRect(ToSkRect(rect), paint);
                break;
            default:
                var radius = (float)(rect.Height / 2 * 0.3);
                canvas.DrawRoundRect(ToSkRect(rect), radius, radius, paint);
                break;
        }
    }

    private void RenderStamps(SKCanvas canvas, CalendarDay day, CalendarRect cell)
    {
        if (day.Stamps.Count == 0)
        {
            return;
        }

        var edge = style.StampMarginEdge;
        var stamps = day.Stamps;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < stamps.Count; i++)
        {
            var stamp = stamps[i];
            if (!Double.IsFinite(stamp.FontSize) || (stamp.FontSize <= 0) || String.IsNullOrEmpty(stamp.Glyph))
            {
                continue;
            }

            var text = fonts.CreateText(stamp.Glyph, (float)stamp.FontSize, false);
            if (text.Segments.Count == 0)
            {
                continue;
            }

            var metrics = text.Segments[0].Font.Metrics;
            var (x, textAlign) = stamp.Position switch
            {
                CalendarStampPosition.TopLeft or CalendarStampPosition.BottomLeft => (cell.X + edge, SKTextAlign.Left),
                CalendarStampPosition.TopRight or CalendarStampPosition.BottomRight => (cell.Right - edge, SKTextAlign.Right),
                _ => (cell.CenterX, SKTextAlign.Center)
            };
            var baseline = stamp.Position switch
            {
                CalendarStampPosition.TopLeft or CalendarStampPosition.TopCenter or CalendarStampPosition.TopRight => metrics.GetTopBaseline((float)(cell.Y + edge)),
                CalendarStampPosition.BottomLeft or CalendarStampPosition.BottomCenter or CalendarStampPosition.BottomRight => metrics.GetBottomBaseline((float)(cell.Bottom - edge)),
                _ => metrics.GetCenterBaseline((float)cell.CenterY)
            };
            paint.Style = SKPaintStyle.Fill;
            paint.Color = (stamp.Color ?? style.StampColor).ToSKColor().WithOpacity((float)stamp.Opacity);
            canvas.Save();
            canvas.ClipRect(ToSkRect(cell));
            canvas.DrawText(text, (float)x, baseline, textAlign, paint);
            canvas.Restore();
        }
    }

    private void RenderEvents(SKCanvas canvas, CalendarLayout layout, CalendarWeek week, int weekIndex, CalendarRect weekBounds)
    {
        if ((week.Placements.Count == 0) && !week.Days.Any(static x => x.HiddenEventCount > 0))
        {
            return;
        }

        canvas.Save();
        canvas.ClipRect(ToSkRect(weekBounds));
        if (style.EventDisplayMode == CalendarEventDisplayMode.Dots)
        {
            RenderEventDots(canvas, layout, week, weekIndex);
        }
        else
        {
            RenderEventBars(canvas, layout, week, weekIndex);
        }

        canvas.Restore();
    }

    private void RenderEventDots(SKCanvas canvas, CalendarLayout layout, CalendarWeek week, int weekIndex)
    {
        paint.Style = SKPaintStyle.Fill;
        for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
        {
            var day = week.Days[column];
            var bounds = layout.GetSlotBounds(weekIndex, 0, column, 1);
            if ((day.Events.Count == 0) || bounds.IsEmpty)
            {
                continue;
            }

            var count = style.MaxVisibleDots > 0 ? Math.Min(day.Events.Count, style.MaxVisibleDots) : day.Events.Count;
            var size = (float)style.EventDotSize;
            var step = size + (float)style.EventDotSpacing;
            var left = (float)bounds.CenterX - (((count * size) + ((count - 1) * (float)style.EventDotSpacing)) / 2) + (size / 2);
            var y = (float)bounds.CenterY;
            for (var i = 0; i < count; i++)
            {
                var calendarEvent = day.Events[i];
                paint.Color = (calendarEvent.Style == CalendarEventStyle.Text ? calendarEvent.TextColor : calendarEvent.BackgroundColor).ToSKColor();
                canvas.DrawCircle(left + (i * step), y, size / 2, paint);
            }
        }
    }

    private void RenderEventBars(SKCanvas canvas, CalendarLayout layout, CalendarWeek week, int weekIndex)
    {
        var placements = week.Placements;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < placements.Count; i++)
        {
            var placement = placements[i];
            var rect = layout.GetEventBounds(weekIndex, placement);
            if (rect.IsEmpty)
            {
                continue;
            }

            var calendarEvent = placement.Event;
            if (calendarEvent.Style == CalendarEventStyle.Filled)
            {
                var left = (float)(placement.ContinuesFromPreviousWeek ? 0 : style.EventCornerRadius);
                var right = (float)(placement.ContinuesToNextWeek ? 0 : style.EventCornerRadius);
                var marginLeft = placement.ContinuesFromPreviousWeek ? 0 : 1;
                var marginRight = placement.ContinuesToNextWeek ? 0 : 1;
                using var roundRect = new SKRoundRect();
                roundRect.SetRectRadii(new SKRect((float)rect.X + marginLeft, (float)rect.Y, (float)rect.Right - marginRight, (float)rect.Bottom), [new SKPoint(left, left), new SKPoint(right, right), new SKPoint(right, right), new SKPoint(left, left)]);
                fillPaint.Color = calendarEvent.BackgroundColor.ToSKColor();
                canvas.DrawRoundRect(roundRect, fillPaint);
            }

            DrawText(canvas, rect, eventTexts.GetValue(calendarEvent, FormatEventText), TextAlignment.Start, style.EventFontSize, false, calendarEvent.TextColor, style.EventPadding);
        }

        for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
        {
            var day = week.Days[column];
            if (day.HiddenEventCount > 0)
            {
                DrawText(canvas, layout.GetOverflowBounds(weekIndex, column), GetOverflowText(day.HiddenEventCount), TextAlignment.Start, style.EventFontSize, false, style.OverflowTextColor, style.EventPadding);
            }
        }
    }

    private Color GetDateTextColor(CalendarDay day)
    {
        if (!day.IsCurrentMonth)
        {
            return style.OutsideMonthTextColor;
        }

        return day.Kind switch
        {
            CalendarDayKind.Holiday => style.HolidayTextColor,
            CalendarDayKind.Sunday => style.SundayTextColor,
            CalendarDayKind.Saturday => style.SaturdayTextColor,
            _ => style.WeekdayTextColor
        };
    }

    private string GetTitle(CalendarMonth month, CultureInfo culture, CalendarTextFormat format, Func<CalendarMonth, string>? formatter)
    {
        var key = (month.Year, month.Month, culture, format, formatter);
        if (titleKey != key)
        {
            title = FormatTitle(month, culture, format, formatter);
            titleKey = key;
        }

        return title;
    }

    private string GetWeekdayName(DayOfWeek dayOfWeek, CultureInfo culture, CalendarWeekdayNameFormat format)
    {
        var key = (dayOfWeek, culture, format);
        if (!weekdayNames.TryGetValue(key, out var name))
        {
            name = FormatWeekdayName(dayOfWeek, culture, format);
            weekdayNames[key] = name;
        }

        return name;
    }

    private string GetOverflowText(int count)
    {
        if (!overflowTexts.TryGetValue(count, out var text))
        {
            text = "+" + count.ToString(CultureInfo.InvariantCulture);
            overflowTexts[count] = text;
        }

        return text;
    }

    private string GetWeekNumberText(int number)
    {
        if (!weekNumberTexts.TryGetValue(number, out var text))
        {
            text = number.ToString(CultureInfo.InvariantCulture);
            weekNumberTexts[number] = text;
        }

        return text;
    }

    private void DrawText(SKCanvas canvas, CalendarRect rect, string text, TextAlignment alignment, double size, bool bold, Color color, double padding = 0)
    {
        var available = (float)(rect.Width - (padding * 2));
        if (rect.IsEmpty || (available <= 0) || (text.Length == 0))
        {
            return;
        }

        var drawableText = fonts.CreateText(text, (float)size, bold, available);
        if (drawableText.Segments.Count == 0)
        {
            return;
        }

        var (x, textAlign) = alignment switch
        {
            TextAlignment.Center => (rect.CenterX, SKTextAlign.Center),
            TextAlignment.End => (rect.Right - padding, SKTextAlign.Right),
            _ => (rect.X + padding, SKTextAlign.Left)
        };
        var baseline = fonts.GetMetrics((float)size, bold).GetCenterBaseline((float)rect.CenterY);
        paint.Style = SKPaintStyle.Fill;
        paint.Color = color.ToSKColor();
        canvas.Save();
        canvas.ClipRect(ToSkRect(rect));
        canvas.DrawText(drawableText, (float)x, baseline, textAlign, paint);
        canvas.Restore();
    }

    private void Fill(SKCanvas canvas, CalendarRect rect, Color color)
    {
        if (rect.IsEmpty)
        {
            return;
        }

        fillPaint.Color = color.ToSKColor();
        canvas.DrawRect(ToSkRect(rect), fillPaint);
    }

    private void DrawHorizontalLine(SKCanvas canvas, double left, double right, double y)
    {
        var width = (float)style.GridLineWidth;
        fillPaint.Color = style.GridLineColor.ToSKColor();
        canvas.DrawRect(SKRect.Create((float)left, (float)y - (width / 2), (float)(right - left), width), fillPaint);
    }

    private void DrawVerticalLine(SKCanvas canvas, double x, double top, double bottom)
    {
        var width = (float)style.GridLineWidth;
        fillPaint.Color = style.GridLineColor.ToSKColor();
        canvas.DrawRect(SKRect.Create((float)x - (width / 2), (float)top, width, (float)(bottom - top)), fillPaint);
    }

    private static SKRect ToSkRect(CalendarRect rect) => new((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom);
}
