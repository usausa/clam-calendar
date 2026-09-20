namespace ClamCalendar.Rendering;

using System.Buffers;
using System.Globalization;
using System.Text;

using SkiaSharp;

internal sealed record TextRunPart(string Text, float Width, SKFont Font);

internal sealed record TextRun(IReadOnlyList<TextRunPart> Parts, float Width)
{
    public static TextRun Empty { get; } = new([], 0);
}

// Resolves a font per character: the primary family, then CalendarFonts.Fallbacks, then the system fonts with the language hints
internal sealed class CalendarFontResolver : IDisposable
{
    private const int EmojiFlag = 1 << 24;
    private const int CacheLimit = 2048;
    private const float ProbeSize = 12;

    private static readonly string[] EmojiLanguages = ["und-Zsye"];
    private static readonly SearchValues<char> Selectors = SearchValues.Create("︎️‍");

    private readonly string familyName;
    private readonly SKTypeface primary;
    private readonly string[]? languages;
    private readonly SKTypeface[] fallbacks;
    private readonly Dictionary<string, SKTypeface> systemByFamily = [with(StringComparer.Ordinal)];
    private readonly Dictionary<int, SKTypeface> byCodepoint = [];
    private readonly Dictionary<(SKTypeface Typeface, float Size, bool Bold), SKFont> fonts = [];
    private readonly Dictionary<(string Text, float Size, bool Bold, float MaxWidth), TextRun> cache = [];

    public int Measurements { get; private set; }

    public CalendarFontResolver(string familyName)
    {
        this.familyName = familyName;
        primary = SKTypeface.FromFamilyName(familyName) ?? SKTypeface.Default;
        languages = CalendarFonts.Languages.Count > 0 ? CalendarFonts.Languages.ToArray() : null;
        fallbacks = CalendarFonts.Fallbacks.ToArray();
    }

    public void Dispose()
    {
        foreach (var font in fonts.Values)
        {
            font.Dispose();
        }

        fonts.Clear();
        foreach (var typeface in systemByFamily.Values)
        {
            typeface.Dispose();
        }

        systemByFamily.Clear();
        byCodepoint.Clear();
        cache.Clear();
        primary.Dispose();
    }

    public SKFontMetrics GetMetrics(float size, bool bold) => GetFont(primary, size, bold).Metrics;

    public float GetLineHeight(float size, bool bold)
    {
        var metrics = GetMetrics(size, bold);
        return metrics.Descent - metrics.Ascent;
    }

    // Splits the text into runs that share a font and truncates it with an ellipsis when it does not fit
    public TextRun Shape(string text, float size, bool bold, float maxWidth = Single.PositiveInfinity)
    {
        if (text.Length == 0)
        {
            return TextRun.Empty;
        }

        var key = (text, size, bold, maxWidth);
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var run = ShapeCore(Sanitize(text), size, bold, maxWidth);
        if (cache.Count >= CacheLimit)
        {
            cache.Clear();
        }

        cache[key] = run;
        return run;
    }

    public static float Draw(SKCanvas canvas, TextRun run, float x, float baseline, SKPaint paint)
    {
        foreach (var part in run.Parts)
        {
            canvas.DrawText(part.Text, x, baseline, SKTextAlign.Left, part.Font, paint);
            x += part.Width;
        }

        return x;
    }

    private static string Sanitize(string text) => text.Replace('\r', ' ').Replace('\n', ' ');

    // Variation selectors and joiners have no glyph without shaping, so they only steer the font choice and are not drawn
    private static string StripSelectors(string text) => text.AsSpan().IndexOfAny(Selectors) < 0 ? text : text.Replace("️", String.Empty, StringComparison.Ordinal).Replace("︎", String.Empty, StringComparison.Ordinal).Replace("‍", String.Empty, StringComparison.Ordinal);

    private TextRun ShapeCore(string text, float size, bool bold, float maxWidth)
    {
        var segments = Segment(text, size, bold);
        var widths = new float[segments.Count];
        var width = 0f;
        for (var i = 0; i < segments.Count; i++)
        {
            Measurements++;
            widths[i] = segments[i].Font.MeasureText(segments[i].Text);
            width += widths[i];
        }

        var parts = new List<TextRunPart>(segments.Count + 1);
        if (width <= maxWidth)
        {
            for (var i = 0; i < segments.Count; i++)
            {
                parts.Add(new TextRunPart(segments[i].Text, widths[i], segments[i].Font));
            }

            return new TextRun(parts, width);
        }

        // Binary search on text element boundaries for the longest prefix that fits together with the ellipsis
        var ellipsisFont = GetFont(primary, size, bold);
        Measurements++;
        var ellipsisWidth = ellipsisFont.MeasureText("…");
        if (ellipsisWidth > maxWidth)
        {
            return TextRun.Empty;
        }

        width = 0f;
        for (var i = 0; i < segments.Count; i++)
        {
            if (width + widths[i] + ellipsisWidth <= maxWidth)
            {
                parts.Add(new TextRunPart(segments[i].Text, widths[i], segments[i].Font));
                width += widths[i];
                continue;
            }

            var segmentText = segments[i].Text;
            var boundaries = StringInfo.ParseCombiningCharacters(segmentText);
            var low = 0;
            var high = boundaries.Length;
            var lowWidth = 0f;
            while (low < high)
            {
                var middle = low + ((high - low + 1) / 2);
                var end = middle == boundaries.Length ? segmentText.Length : boundaries[middle];
                Measurements++;
                var prefixWidth = segments[i].Font.MeasureText(segmentText.AsSpan(0, end));
                if (width + prefixWidth + ellipsisWidth <= maxWidth)
                {
                    low = middle;
                    lowWidth = prefixWidth;
                }
                else
                {
                    high = middle - 1;
                }
            }

            if (low > 0)
            {
                var end = low == boundaries.Length ? segmentText.Length : boundaries[low];
                parts.Add(new TextRunPart(segmentText[..end], lowWidth, segments[i].Font));
                width += lowWidth;
            }

            break;
        }

        parts.Add(new TextRunPart("…", ellipsisWidth, ellipsisFont));
        return new TextRun(parts, width + ellipsisWidth);
    }

    // Picks a font per text element (combining marks and surrogate pairs included) and merges runs that share a font
    private List<TextRunPart> Segment(string text, float size, bool bold)
    {
        var segments = new List<TextRunPart>();
        var boundaries = StringInfo.ParseCombiningCharacters(text);
        SKTypeface? current = null;
        var start = 0;
        for (var i = 0; i < boundaries.Length; i++)
        {
            var index = boundaries[i];
            var end = i + 1 < boundaries.Length ? boundaries[i + 1] : text.Length;
            var emoji = text.AsSpan(index, end - index).Contains('️');
            var typeface = Rune.TryGetRuneAt(text, index, out var rune) ? ResolveTypeface(rune.Value, emoji) : primary;
            if (ReferenceEquals(typeface, current))
            {
                continue;
            }

            if (current is not null)
            {
                segments.Add(new TextRunPart(StripSelectors(text[start..index]), 0, GetFont(current, size, bold)));
            }

            current = typeface;
            start = index;
        }

        if (current is not null)
        {
            segments.Add(new TextRunPart(StripSelectors(text[start..]), 0, GetFont(current, size, bold)));
        }

        return segments;
    }

    private SKTypeface ResolveTypeface(int codepoint, bool emoji)
    {
        if ((codepoint <= 127) && !emoji)
        {
            return primary;
        }

        var key = emoji ? codepoint | EmojiFlag : codepoint;
        if (byCodepoint.TryGetValue(key, out var typeface))
        {
            return typeface;
        }

        // An emoji presentation sequence asks the system for the emoji font before the primary font is considered
        typeface = emoji ? MatchSystemFont(codepoint, EmojiLanguages) : null;
        if (typeface is null)
        {
            typeface = primary;
            if (!ContainsGlyph(primary, codepoint))
            {
                // Configured fallback fonts come first, then the system fonts are searched per character with the language hints
                typeface = Array.Find(fallbacks, fallback => ContainsGlyph(fallback, codepoint)) ?? MatchSystemFont(codepoint, languages) ?? primary;
            }
        }

        byCodepoint[key] = typeface;
        return typeface;
    }

    private SKTypeface? MatchSystemFont(int codepoint, string[]? hints)
    {
        var typeface = SKFontManager.Default.MatchCharacter(familyName, SKFontStyle.Normal, hints, codepoint);
        if (typeface is null)
        {
            return null;
        }

        if (systemByFamily.TryGetValue(typeface.FamilyName, out var existing))
        {
            typeface.Dispose();
            return existing;
        }

        systemByFamily[typeface.FamilyName] = typeface;
        return typeface;
    }

    private bool ContainsGlyph(SKTypeface typeface, int codepoint) => GetFont(typeface, ProbeSize, false).ContainsGlyph(codepoint);

    private SKFont GetFont(SKTypeface typeface, float size, bool bold)
    {
        var key = (typeface, size, bold);
        if (!fonts.TryGetValue(key, out var font))
        {
            font = new SKFont(typeface, size) { Embolden = bold };
            fonts[key] = font;
        }

        return font;
    }
}
