namespace UnitConverter.Core;

/// <summary>
/// A unit of measurement that converts values to and from its category's base unit.
/// Converting through a shared base unit means N units need N definitions, not N² conversion pairs.
/// </summary>
public interface IUnit
{
    string Name { get; }

    string Symbol { get; }

    double ToBase(double value);

    double FromBase(double value);
}
