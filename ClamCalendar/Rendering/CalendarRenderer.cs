namespace ClamCalendar.Rendering;

using System.Globalization;
using System.Text;

using ClamCalendar.Layout;

using SkiaSharp;
using SkiaSharp.Views.Maui;

internal sealed class CalendarRenderer : IDisposable
{
    private const float LineWidth = 0.5f;

    private readonly CalendarStyle style;
    private readonly CalendarFontResolver fonts;
    private readonly SKPaint paint = new() { IsAntialias = true };
    private readonly SKPaint fillPaint = new() { IsAntialias = false };

    public int Measurements => fonts.Measurements;

    public CalendarRenderer(CalendarStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        style.Validate();
        this.style = style;
        fonts = new CalendarFontResolver(style.FontFamily);
    }

    public void Dispose()
    {
        paint.Dispose();
        fillPaint.Dispose();
        fonts.Dispose();
    }

    public static string FormatTitle(CalendarMonth month, CultureInfo culture, string? format)
    {
        ArgumentNullException.ThrowIfNull(month);
        ArgumentNullException.ThrowIfNull(culture);
        return new DateTime(month.Year, month.Month, 1).ToString(String.IsNullOrEmpty(format) ? culture.DateTimeFormat.YearMonthPattern : format, culture);
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

        if (layout.TodayButtonVisible)
        {
            DrawText(canvas, layout.TodayButtonBounds, state.TodayButtonText, TextAlignment.Center, style.DateNumberFontSize, false, style.NavigationButtonColor, style.EventPadding);
        }

        DrawText(canvas, layout.TitleBounds, FormatTitle(layout.Month, state.Culture, state.HeaderFormat), TextAlignment.Center, style.HeaderFontSize, true, style.HeaderTextColor, style.EventPadding);
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
            DrawText(canvas, layout.GetWeekdayHeaderCellBounds(column), FormatWeekdayName(dayOfWeek, state.Culture, state.WeekdayNameFormat), TextAlignment.Center, style.WeekdayHeaderFontSize, false, color, 1);
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
        canvas.Save();
        canvas.ClipRect(ToSkRect(body));
        var layered = state.SlideOpacity < 1;
        if (layered)
        {
            using var layerPaint = new SKPaint();
            layerPaint.Color = SKColors.Black.WithAlpha((byte)(Math.Clamp(state.SlideOpacity, 0, 1) * 255));
            canvas.SaveLayer(layerPaint);
        }

        canvas.Translate(state.SlideOffset, 0);
        if (state.MonthIndicatorVisible)
        {
            DrawText(canvas, body, month.Month.ToString(CultureInfo.InvariantCulture), TextAlignment.Center, style.MonthIndicatorFontSize, true, style.MonthIndicatorColor);
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
            DrawHorizontalLine(canvas, weekBounds.X, weekBounds.Right, weekBounds.Y);
            for (var column = 0; column < CalendarLayout.DaysPerWeek - 1; column++)
            {
                var cell = layout.GetCellBounds(w, column);
                DrawVerticalLine(canvas, cell.Right, cell.Y, cell.Bottom);
            }

            for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
            {
                var day = week.Days[column];
                if (!day.IsCurrentMonth && !state.OutsideMonthDaysVisible)
                {
                    continue;
                }

                RenderDateNumber(canvas, layout, state, day, w, column);
                RenderStamps(canvas, day, layout.GetCellBounds(w, column));
            }

            RenderEvents(canvas, layout, week, w, weekBounds);
        }

        DrawHorizontalLine(canvas, body.X, body.Right, body.Y + (layout.WeekHeight * layout.WeekCount));
        if (layered)
        {
            canvas.Restore();
        }

        canvas.Restore();
    }

    private void RenderCellBackground(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state, CalendarDay day, CalendarRect cell, int week, int column)
    {
        var background = !day.IsCurrentMonth
            ? style.OutsideMonthBackground
            : day.Kind switch
            {
                CalendarDayKind.Holiday => style.HolidayBackground,
                CalendarDayKind.Saturday or CalendarDayKind.Sunday => style.WeekendBackground,
                _ => null
            };
        if (background is not null)
        {
            Fill(canvas, cell, background);
        }

        if ((state.IsInRange?.Invoke(day.Date) ?? false) && (day.IsCurrentMonth || state.OutsideMonthDaysVisible))
        {
            Fill(canvas, layout.GetDateRowBounds(week, column), style.RangeBackground);
        }
    }

    private void RenderDateNumber(SKCanvas canvas, CalendarLayout layout, CalendarRenderState state, CalendarDay day, int week, int column)
    {
        var bubble = layout.GetDateNumberBounds(week, column);
        if (bubble.IsEmpty)
        {
            return;
        }

        Color textColor;
        Color? bubbleColor = null;
        if (state.IsDisabled?.Invoke(day.Date) ?? false)
        {
            textColor = style.DisabledDayTextColor;
        }
        else if (state.IsSelected?.Invoke(day.Date) ?? false)
        {
            textColor = style.SelectedDayTextColor;
            bubbleColor = style.SelectedDayBackground;
        }
        else if (day.IsToday)
        {
            textColor = style.TodayTextColor;
            bubbleColor = style.TodayBackground;
        }
        else
        {
            textColor = GetDateTextColor(day);
        }

        if (bubbleColor is not null)
        {
            var radius = (float)(bubble.Height / 2 * 0.3);
            paint.Style = SKPaintStyle.Fill;
            paint.Color = bubbleColor.ToSKColor();
            canvas.DrawRoundRect(ToSkRect(bubble), radius, radius, paint);
        }

        DrawText(canvas, bubble, day.Date.Day.ToString(CultureInfo.InvariantCulture), TextAlignment.Center, style.DateNumberFontSize, true, textColor);
        if (!String.IsNullOrEmpty(day.Label))
        {
            var cell = layout.GetCellBounds(week, column);
            var labelBounds = new CalendarRect(bubble.Right + 2, bubble.Y, cell.Right - bubble.Right - 4, bubble.Height);
            DrawText(canvas, labelBounds, day.Label, TextAlignment.Start, style.DayLabelFontSize, false, day.IsCurrentMonth ? style.DayLabelTextColor : style.OutsideMonthTextColor);
        }
    }

    private void RenderStamps(SKCanvas canvas, CalendarDay day, CalendarRect cell)
    {
        if (day.Stamps.Count == 0)
        {
            return;
        }

        var edge = style.StampMarginEdge;
        foreach (var stamp in day.Stamps)
        {
            if (!Single.IsFinite(stamp.FontSize) || (stamp.FontSize <= 0) || String.IsNullOrEmpty(stamp.Glyph))
            {
                continue;
            }

            var run = fonts.Shape(stamp.Glyph, stamp.FontSize, false);
            if (run.Parts.Count == 0)
            {
                continue;
            }

            var metrics = run.Parts[0].Font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var x = stamp.Position switch
            {
                CalendarStampPosition.TopLeft or CalendarStampPosition.BottomLeft => cell.X + edge,
                CalendarStampPosition.TopRight or CalendarStampPosition.BottomRight => cell.Right - edge - run.Width,
                _ => cell.CenterX - (run.Width / 2)
            };
            var baseline = stamp.Position switch
            {
                CalendarStampPosition.TopLeft or CalendarStampPosition.TopCenter or CalendarStampPosition.TopRight => cell.Y + edge - metrics.Ascent,
                CalendarStampPosition.BottomLeft or CalendarStampPosition.BottomCenter or CalendarStampPosition.BottomRight => cell.Bottom - edge - metrics.Descent,
                _ => cell.Y + ((cell.Height - textHeight) / 2) - metrics.Ascent
            };
            paint.Style = SKPaintStyle.Fill;
            paint.Color = SKColors.Black.WithAlpha((byte)(Math.Clamp(stamp.Opacity, 0, 1) * 255));
            canvas.Save();
            canvas.ClipRect(ToSkRect(cell));
            CalendarFontResolver.Draw(canvas, run, (float)x, (float)baseline, paint);
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
        foreach (var placement in week.Placements)
        {
            var rect = layout.GetEventBounds(weekIndex, placement);
            if (rect.IsEmpty)
            {
                continue;
            }

            var calendarEvent = placement.Event;
            if (calendarEvent.Style == CalendarEventStyle.Filled)
            {
                var left = placement.ContinuesFromPreviousWeek ? 0 : style.EventCornerRadius;
                var right = placement.ContinuesToNextWeek ? 0 : style.EventCornerRadius;
                var marginLeft = placement.ContinuesFromPreviousWeek ? 0 : 1;
                var marginRight = placement.ContinuesToNextWeek ? 0 : 1;
                using var roundRect = new SKRoundRect();
                roundRect.SetRectRadii(new SKRect((float)rect.X + marginLeft, (float)rect.Y, (float)rect.Right - marginRight, (float)rect.Bottom), [new SKPoint(left, left), new SKPoint(right, right), new SKPoint(right, right), new SKPoint(left, left)]);
                fillPaint.Color = calendarEvent.BackgroundColor.ToSKColor();
                canvas.DrawRoundRect(roundRect, fillPaint);
            }

            DrawText(canvas, rect, FormatEventText(calendarEvent), TextAlignment.Start, style.EventFontSize, false, calendarEvent.TextColor, style.EventPadding);
        }

        for (var column = 0; column < CalendarLayout.DaysPerWeek; column++)
        {
            var day = week.Days[column];
            if (day.HiddenEventCount > 0)
            {
                DrawText(canvas, layout.GetOverflowBounds(weekIndex, column), "+" + day.HiddenEventCount.ToString(CultureInfo.InvariantCulture), TextAlignment.Start, style.EventFontSize, false, style.OverflowTextColor, style.EventPadding);
            }
        }

        canvas.Restore();
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

    private void DrawText(SKCanvas canvas, CalendarRect rect, string text, TextAlignment alignment, float size, bool bold, Color color, float padding = 0)
    {
        var available = (float)rect.Width - (padding * 2);
        if (rect.IsEmpty || (available <= 0) || (text.Length == 0))
        {
            return;
        }

        var run = fonts.Shape(text, size, bold, available);
        if (run.Parts.Count == 0)
        {
            return;
        }

        var x = alignment switch
        {
            TextAlignment.Center => rect.X + ((rect.Width - run.Width) / 2),
            TextAlignment.End => rect.Right - padding - run.Width,
            _ => rect.X + padding
        };
        var metrics = fonts.GetMetrics(size, bold);
        var baseline = rect.Y + ((rect.Height - (metrics.Descent - metrics.Ascent)) / 2) - metrics.Ascent;
        paint.Style = SKPaintStyle.Fill;
        paint.Color = color.ToSKColor();
        canvas.Save();
        canvas.ClipRect(ToSkRect(rect));
        CalendarFontResolver.Draw(canvas, run, (float)x, (float)baseline, paint);
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
        fillPaint.Color = style.GridLineColor.ToSKColor();
        canvas.DrawRect(SKRect.Create((float)left, (float)y - (LineWidth / 2), (float)(right - left), LineWidth), fillPaint);
    }

    private void DrawVerticalLine(SKCanvas canvas, double x, double top, double bottom)
    {
        fillPaint.Color = style.GridLineColor.ToSKColor();
        canvas.DrawRect(SKRect.Create((float)x - (LineWidth / 2), (float)top, LineWidth, (float)(bottom - top)), fillPaint);
    }

    private static SKRect ToSkRect(CalendarRect rect) => new((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom);
}
