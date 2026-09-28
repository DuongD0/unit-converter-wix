using System.Globalization;

namespace UnitConverter.Core;

/// <summary>The outcome of a single conversion, as shown to the user and stored in history.</summary>
public sealed record ConversionRecord(
    string Category,
    double Input,
    string FromSymbol,
    double Output,
    string ToSymbol,
    DateTimeOffset Timestamp)
{
    public override string ToString() =>
        string.Format(
            CultureInfo.CurrentCulture,
            "{0:g}   {1}: {2:G6} {3} = {4:G6} {5}",
            Timestamp.ToLocalTime(),
            Category,
            Input,
            FromSymbol,
            Output,
            ToSymbol);
}
