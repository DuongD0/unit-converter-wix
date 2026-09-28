using UnitConverter.Core;

namespace UnitConverter.Units;

/// <summary>
/// The built-in categories. To add a unit, add one line to the matching builder;
/// to add a category, add one more builder. Nothing else in the application changes.
/// </summary>
public sealed class StandardUnitCatalog : IUnitCatalog
{
    public IReadOnlyList<UnitCategory> Categories { get; } =
    [
        // Base unit: metre
        new CategoryBuilder("Length")
            .Add("Metre", "m", 1)
            .Add("Kilometre", "km", 1_000)
            .Add("Centimetre", "cm", 0.01)
            .Add("Millimetre", "mm", 0.001)
            .Add("Mile", "mi", 1_609.344)
            .Add("Yard", "yd", 0.9144)
            .Add("Foot", "ft", 0.3048)
            .Add("Inch", "in", 0.0254)
            .Build(),

        // Base unit: kilogram
        new CategoryBuilder("Mass")
            .Add("Kilogram", "kg", 1)
            .Add("Gram", "g", 0.001)
            .Add("Tonne", "t", 1_000)
            .Add("Pound", "lb", 0.45359237)
            .Add("Ounce", "oz", 0.028349523125)
            .Build(),

        // Base unit: kelvin
        new CategoryBuilder("Temperature")
            .Add("Kelvin", "K", 1)
            .Add("Celsius", "°C", 1, 273.15)
            .Add("Fahrenheit", "°F", 5.0 / 9.0, 459.67 * 5.0 / 9.0)
            .Build(),

        // Base unit: litre
        new CategoryBuilder("Volume")
            .Add("Litre", "L", 1)
            .Add("Millilitre", "mL", 0.001)
            .Add("Cubic metre", "m³", 1_000)
            .Add("US gallon", "gal", 3.785411784)
            .Add("US cup", "cup", 0.2365882365)
            .Build(),

        // Base unit: metre per second
        new CategoryBuilder("Speed")
            .Add("Metre per second", "m/s", 1)
            .Add("Kilometre per hour", "km/h", 1_000.0 / 3_600.0)
            .Add("Mile per hour", "mph", 0.44704)
            .Add("Knot", "kn", 1_852.0 / 3_600.0)
            .Build(),
    ];
}
