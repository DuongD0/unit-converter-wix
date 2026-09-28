using UnitConverter.Core;

namespace UnitConverter.Units;

/// <summary>Fluent helper that keeps unit definitions short and readable.</summary>
public sealed class CategoryBuilder
{
    private readonly string _name;
    private readonly List<IUnit> _units = [];

    public CategoryBuilder(string name) => _name = name;

    public CategoryBuilder Add(string name, string symbol, double factor, double offset = 0)
    {
        _units.Add(new AffineUnit(name, symbol, factor, offset));
        return this;
    }

    public UnitCategory Build() => new(_name, _units);
}
