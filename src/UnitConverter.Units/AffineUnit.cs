using UnitConverter.Core;

namespace UnitConverter.Units;

/// <summary>
/// A unit related to its base unit by <c>base = value * Factor + Offset</c>.
/// A zero offset covers ordinary scaled units (metres, grams); a non-zero offset covers
/// temperature scales such as Celsius and Fahrenheit.
/// </summary>
public sealed class AffineUnit : IUnit
{
    public AffineUnit(string name, string symbol, double factor, double offset = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        if (!double.IsFinite(factor) || factor == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), factor, "Factor must be finite and non-zero.");
        }

        Name = name;
        Symbol = symbol;
        Factor = factor;
        Offset = offset;
    }

    public string Name { get; }

    public string Symbol { get; }

    public double Factor { get; }

    public double Offset { get; }

    public double ToBase(double value) => value * Factor + Offset;

    public double FromBase(double value) => (value - Offset) / Factor;

    public override string ToString() => $"{Name} ({Symbol})";
}
