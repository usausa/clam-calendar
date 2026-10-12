namespace ClamCalendar.Rendering;

using System.Text;

internal sealed class CalendarTextFormat : IEquatable<CalendarTextFormat>
{
    private readonly string? format;
    private readonly CompositeFormat? composite;

    public static CalendarTextFormat Default { get; } = new(null, null);

    public bool IsDefault => ReferenceEquals(this, Default);

    private CalendarTextFormat(string? format, CompositeFormat? composite)
    {
        this.format = format;
        this.composite = composite;
    }

    public static CalendarTextFormat Parse(string? format)
    {
        if (String.IsNullOrEmpty(format))
        {
            return Default;
        }

        return format.Contains('{', StringComparison.Ordinal) ? new CalendarTextFormat(null, CompositeFormat.Parse(format)) : new CalendarTextFormat(format, null);
    }

    public string Format<T>(T value, IFormatProvider provider)
        where T : IFormattable =>
        composite is null ? value.ToString(format, provider) : String.Format(provider, composite, value);

    public string Format(object? value, IFormatProvider provider)
    {
        if (value is null)
        {
            return String.Empty;
        }

        if (composite is not null)
        {
            return String.Format(provider, composite, value);
        }

        return value is IFormattable formattable ? formattable.ToString(format, provider) : Convert.ToString(value, provider) ?? String.Empty;
    }

    public bool Equals(CalendarTextFormat? other) => (other is not null) && (format == other.format) && (composite?.Format == other.composite?.Format);

    public override bool Equals(object? obj) => (obj is CalendarTextFormat other) && Equals(other);

    public override int GetHashCode() => HashCode.Combine(format, composite?.Format);
}
