namespace UnitConverter.Core;

/// <summary>Supplies the unit categories available to the application.</summary>
public interface IUnitCatalog
{
    IReadOnlyList<UnitCategory> Categories { get; }
}
