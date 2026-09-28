namespace UnitConverter.Core;

/// <summary>Stores past conversions. Implementations decide where and how.</summary>
public interface IConversionHistory
{
    /// <summary>Returns stored conversions, newest first.</summary>
    IReadOnlyList<ConversionRecord> GetAll();

    void Add(ConversionRecord record);

    void Clear();
}
