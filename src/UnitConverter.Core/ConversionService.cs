namespace UnitConverter.Core;

/// <summary>Converts values between units of the same category and records each conversion.</summary>
public sealed class ConversionService
{
    private readonly IConversionHistory _history;
    private readonly TimeProvider _clock;

    public ConversionService(IConversionHistory history, TimeProvider clock)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public ConversionRecord Convert(UnitCategory category, IUnit from, IUnit to, double value)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be a finite number.");
        }

        EnsureBelongsTo(category, from, nameof(from));
        EnsureBelongsTo(category, to, nameof(to));

        var output = to.FromBase(from.ToBase(value));
        var record = new ConversionRecord(
            category.Name, value, from.Symbol, output, to.Symbol, _clock.GetUtcNow());

        _history.Add(record);
        return record;
    }

    private static void EnsureBelongsTo(UnitCategory category, IUnit unit, string paramName)
    {
        if (!category.Contains(unit))
        {
            throw new ArgumentException(
                $"Unit '{unit.Symbol}' does not belong to category '{category.Name}'.", paramName);
        }
    }
}
