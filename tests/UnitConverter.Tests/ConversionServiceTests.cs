using UnitConverter.Core;
using UnitConverter.Units;

namespace UnitConverter.Tests;

public class ConversionServiceTests
{
    private static readonly IUnitCatalog Catalog = new StandardUnitCatalog();

    private readonly InMemoryHistory _history = new();
    private readonly ConversionService _service;

    public ConversionServiceTests() => _service = new ConversionService(_history, TimeProvider.System);

    [Theory]
    [InlineData("Length", "km", "m", 1, 1000)]
    [InlineData("Length", "mi", "km", 1, 1.609344)]
    [InlineData("Length", "ft", "in", 1, 12)]
    [InlineData("Mass", "lb", "kg", 1, 0.45359237)]
    [InlineData("Temperature", "°C", "°F", 100, 212)]
    [InlineData("Temperature", "°F", "°C", 32, 0)]
    [InlineData("Temperature", "°C", "K", 0, 273.15)]
    [InlineData("Volume", "L", "mL", 1, 1000)]
    [InlineData("Speed", "km/h", "m/s", 36, 10)]
    public void Convert_ReturnsExpectedValue(string category, string from, string to, double input, double expected)
    {
        var (cat, fromUnit, toUnit) = Resolve(category, from, to);

        var record = _service.Convert(cat, fromUnit, toUnit, input);

        Assert.Equal(expected, record.Output, precision: 9);
    }

    [Fact]
    public void Convert_AddsRecordToHistory()
    {
        var (cat, from, to) = Resolve("Length", "m", "cm");

        var record = _service.Convert(cat, from, to, 2);

        Assert.Equal([record], _history.GetAll());
    }

    [Fact]
    public void Convert_RejectsUnitFromAnotherCategory()
    {
        var (length, metre, _) = Resolve("Length", "m", "km");
        var (_, kilogram, _) = Resolve("Mass", "kg", "g");

        Assert.Throws<ArgumentException>(() => _service.Convert(length, metre, kilogram, 1));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Convert_RejectsNonFiniteValues(double value)
    {
        var (cat, from, to) = Resolve("Length", "m", "km");

        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Convert(cat, from, to, value));
    }

    [Fact]
    public void EveryUnit_RoundTripsThroughBase()
    {
        foreach (var unit in Catalog.Categories.SelectMany(c => c.Units))
        {
            Assert.Equal(123.456, unit.FromBase(unit.ToBase(123.456)), precision: 9);
        }
    }

    private static (UnitCategory Category, IUnit From, IUnit To) Resolve(string category, string from, string to)
    {
        var cat = Catalog.Categories.Single(c => c.Name == category);
        return (cat, cat.Units.Single(u => u.Symbol == from), cat.Units.Single(u => u.Symbol == to));
    }

    private sealed class InMemoryHistory : IConversionHistory
    {
        private readonly List<ConversionRecord> _records = [];

        public IReadOnlyList<ConversionRecord> GetAll() => _records;

        public void Add(ConversionRecord record) => _records.Insert(0, record);

        public void Clear() => _records.Clear();
    }
}
