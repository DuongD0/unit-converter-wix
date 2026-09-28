namespace UnitConverter.Core;

/// <summary>A group of units that measure the same quantity (e.g. length).</summary>
public sealed class UnitCategory
{
    public UnitCategory(string name, IEnumerable<IUnit> units)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(units);

        Name = name;
        Units = units.ToList().AsReadOnly();

        if (Units.Count == 0)
        {
            throw new ArgumentException($"Category '{name}' must contain at least one unit.", nameof(units));
        }
    }

    public string Name { get; }

    public IReadOnlyList<IUnit> Units { get; }

    public bool Contains(IUnit unit) => Units.Contains(unit);

    public override string ToString() => Name;
}
