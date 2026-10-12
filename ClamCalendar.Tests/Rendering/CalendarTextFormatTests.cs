namespace ClamCalendar.Tests.Rendering;

public sealed class CalendarTextFormatTests
{
    [Fact]
    public void FormatsWithABraceAreCompositeAndOthersAreFormatStrings()
    {
        // Arrange
        var culture = CultureInfo.InvariantCulture;
        var date = new DateTime(2026, 9, 13);

        // Act & Assert
        Assert.Equal("1,234 pcs", CalendarTextFormat.Parse("{0:N0} pcs").Format(1234, culture));
        Assert.Equal("1,234", CalendarTextFormat.Parse("N0").Format(1234, culture));
        Assert.Equal("2026/09", CalendarTextFormat.Parse("yyyy/MM").Format(date, culture));
        Assert.Equal("Yamada san", CalendarTextFormat.Parse("{0} san").Format((object?)"Yamada", culture));
        Assert.Equal("text", CalendarTextFormat.Parse("D6").Format((object?)"text", culture));
        Assert.Equal(String.Empty, CalendarTextFormat.Parse("{0} san").Format(null, culture));
    }

    [Fact]
    public void EmptyFormatsShareTheDefaultText()
    {
        // Arrange
        var culture = CultureInfo.InvariantCulture;

        // Act
        var none = CalendarTextFormat.Parse(null);
        var empty = CalendarTextFormat.Parse(String.Empty);

        // Assert
        Assert.Same(CalendarTextFormat.Default, none);
        Assert.Same(CalendarTextFormat.Default, empty);
        Assert.True(none.IsDefault);
        Assert.False(CalendarTextFormat.Parse("N0").IsDefault);
        Assert.Equal("1234.5", none.Format(1234.5, culture));
        Assert.Equal("1234.5", none.Format((object?)1234.5, culture));
    }

    [Fact]
    public void InvalidCompositeFormatsAreRejectedWhenParsed()
    {
        // Act & Assert
        Assert.Throws<FormatException>(static () => CalendarTextFormat.Parse("{0:N0"));
    }

    [Fact]
    public void FormatsParsedFromTheSameTextAreEqual()
    {
        // Act & Assert
        Assert.Equal(CalendarTextFormat.Parse("{0:N0} pcs"), CalendarTextFormat.Parse("{0:N0} pcs"));
        Assert.Equal(CalendarTextFormat.Parse("{0:N0} pcs").GetHashCode(), CalendarTextFormat.Parse("{0:N0} pcs").GetHashCode());
        Assert.Equal(CalendarTextFormat.Parse("N0"), CalendarTextFormat.Parse("N0"));
        Assert.NotEqual(CalendarTextFormat.Parse("N0"), CalendarTextFormat.Parse("N1"));
        Assert.NotEqual(CalendarTextFormat.Parse("{0:N0}"), CalendarTextFormat.Parse("N0"));
        Assert.NotEqual(CalendarTextFormat.Default, CalendarTextFormat.Parse("N0"));
    }
}
