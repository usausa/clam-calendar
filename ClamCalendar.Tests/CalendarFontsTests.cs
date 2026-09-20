namespace ClamCalendar.Tests;

public sealed class CalendarFontsTests
{
    [Fact]
    public void DefaultsPreferJapaneseGlyphsWithoutExtraTypefaces()
    {
        // Act
        var languages = CalendarFonts.Languages;
        var fallbacks = CalendarFonts.Fallbacks;

        // Assert
        Assert.Equal(["ja"], languages);
        Assert.Empty(fallbacks);
    }

    [Fact]
    public void ResolverShapesMixedScriptsAndTruncatesWithAnEllipsis()
    {
        // Arrange
        using var resolver = new CalendarFontResolver("sans-serif");

        // Act
        var run = resolver.Shape("見出し 🍎 ▲ text", 14, false);
        var truncated = resolver.Shape("A long title that will not fit", 14, false, 40);
        var empty = resolver.Shape(String.Empty, 14, false);
        var tooNarrow = resolver.Shape("abc", 14, false, 1);
        var cached = resolver.Shape("見出し 🍎 ▲ text", 14, false);

        // Assert
        Assert.True(run.Width > 0);
        Assert.NotEmpty(run.Parts);
        Assert.True(resolver.Measurements > 0);
        Assert.Equal("…", truncated.Parts[^1].Text);
        Assert.True(truncated.Width <= 40);
        Assert.Empty(empty.Parts);
        Assert.Empty(tooNarrow.Parts);
        Assert.Same(run, cached);
        Assert.True(resolver.GetLineHeight(14, false) > 0);
    }

    [Fact]
    public void VariationSelectorsAreNotDrawn()
    {
        // Arrange
        using var resolver = new CalendarFontResolver("sans-serif");

        // Act
        var run = resolver.Shape("✈️", 20, false);

        // Assert
        Assert.DoesNotContain(run.Parts, static part => part.Text.Contains('️', StringComparison.Ordinal));
    }
}
