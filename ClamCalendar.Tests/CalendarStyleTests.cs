namespace ClamCalendar.Tests;

public sealed class CalendarStyleTests
{
    [Fact]
    public void DefaultsMatchTheDocumentedValues()
    {
        // Act
        var style = new CalendarStyle();

        // Assert
        Assert.Equal("sans-serif", style.FontFamily);
        Assert.Equal(20, style.HeaderFontSize);
        Assert.Equal(48, style.HeaderHeight);
        Assert.Equal(32, style.WeekdayHeaderHeight);
        Assert.Equal(28, style.DateRowHeight);
        Assert.Equal(18, style.SlotRowHeight);
        Assert.Equal(16, style.EventRowHeight);
        Assert.Equal(24, style.DateNumberSize);
        Assert.Equal(44, style.NavigationButtonWidth);
        Assert.Equal(Colors.White, style.Background);
        Assert.Equal(Color.FromArgb("#E53935"), style.HolidayTextColor);
        Assert.Equal(Color.FromArgb("#FFF1F1"), style.HolidayBackground);
    }

    [Fact]
    public void StylesAreRecordsThatCanBeCopied()
    {
        // Arrange
        var style = new CalendarStyle();

        // Act
        var dark = style with { Background = Colors.Black, HeaderTextColor = Colors.White };

        // Assert
        Assert.Equal(Colors.White, style.Background);
        Assert.Equal(Colors.Black, dark.Background);
        Assert.Equal(style.HeaderHeight, dark.HeaderHeight);
        Assert.NotEqual(style, dark);
    }

    [Fact]
    public void InvalidSizesAreRejectedWhenTheRendererIsCreated()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { HeaderFontSize = 0 }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { DateRowHeight = Single.NaN }));
        Assert.Throws<ArgumentException>(static () => new CalendarRenderer(new CalendarStyle { EventPadding = -1 }));
    }

    [Fact]
    public void InvalidateRedrawsUntilTheViewIsDisposed()
    {
        // Arrange
        var view = new ClamCalendarView();

        // Act
        view.CalendarStyle.HeaderHeight = 56;
        view.Invalidate();

        // Assert
        Assert.Equal(56, view.CalendarStyle.HeaderHeight);

        // Act
        view.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(view.Invalidate);
    }
}
