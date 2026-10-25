namespace ClamCalendar.Tests;

using SkiaSharp;
using SkiaSharp.Views.Maui;

public sealed class CalendarFrameTests
{
    [Fact]
    public void FrameRenderedReportsEachFrame()
    {
        // Arrange
        using var view = new TestCalendarView();
        var frames = new List<CalendarFrameEventArgs>();
        view.FrameRendered += (_, e) => frames.Add(e);

        // Act
        view.Paint(350, 600);
        view.Paint(350, 600);

        // Assert
        Assert.Equal(2, frames.Count);
        Assert.Equal(42, frames[0].DayCount);
        Assert.True(frames[0].TextMeasurements > 0);
        Assert.False(frames[0].Animating);
        Assert.True(frames[0].ElapsedMilliseconds >= 0);
    }

    [Fact]
    public void FrameRenderedIsNotRaisedWithoutASize()
    {
        // Arrange
        using var view = new TestCalendarView();
        var frames = 0;
        view.FrameRendered += (_, _) => frames++;

        // Act
        view.Paint(0, 0);

        // Assert
        Assert.Equal(0, frames);
    }

    [Fact]
    public void DayCountFollowsTheWeekRows()
    {
        // Arrange
        using var fixedRows = new TestCalendarView();
        fixedRows.DisplayDate = new DateOnly(2026, 9, 1);
        using var trimmedRows = new TestCalendarView();
        trimmedRows.FixedWeekRows = false;
        trimmedRows.DisplayDate = new DateOnly(2026, 9, 1);
        var counts = new List<int>();
        fixedRows.FrameRendered += (_, e) => counts.Add(e.DayCount);
        trimmedRows.FrameRendered += (_, e) => counts.Add(e.DayCount);

        // Act
        fixedRows.Paint(350, 600);
        trimmedRows.Paint(350, 600);

        // Assert
        Assert.Equal(42, counts[0]);
        Assert.Equal(35, counts[1]);
    }

    private sealed class TestCalendarView : ClamCalendarView
    {
        public void Paint(double width, double height)
        {
            Frame = new Rect(0, 0, width, height);
            var info = new SKImageInfo((int)Math.Max(1, width * 2), (int)Math.Max(1, height * 2));
            using var surface = SKSurface.Create(info);
            OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
        }
    }
}
