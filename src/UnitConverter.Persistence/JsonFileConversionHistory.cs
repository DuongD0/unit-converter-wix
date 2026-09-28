using System.Text.Json;
using UnitConverter.Core;

namespace UnitConverter.Persistence;

/// <summary>
/// Keeps the most recent conversions in a JSON file so history survives restarts.
/// A missing or unreadable file is treated as empty history rather than a fatal error.
/// </summary>
public sealed class JsonFileConversionHistory : IConversionHistory
{
    public const int DefaultCapacity = 50;

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly int _capacity;
    private readonly Lock _gate = new();
    private readonly List<ConversionRecord> _records;

    public JsonFileConversionHistory(string filePath, int capacity = DefaultCapacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _filePath = filePath;
        _capacity = capacity;
        _records = Load(filePath);
    }

    /// <summary>History stored under the current user's local application data folder.</summary>
    public static JsonFileConversionHistory ForCurrentUser(string appName) =>
        new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName,
            "history.json"));

    public IReadOnlyList<ConversionRecord> GetAll()
    {
        lock (_gate)
        {
            return _records.ToArray();
        }
    }

    public void Add(ConversionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_gate)
        {
            _records.Insert(0, record);
            if (_records.Count > _capacity)
            {
                _records.RemoveRange(_capacity, _records.Count - _capacity);
            }

            Save();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _records.Clear();
            Save();
        }
    }

    private static List<ConversionRecord> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            return JsonSerializer.Deserialize<List<ConversionRecord>>(stream, SerializerOptions) ?? [];
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh; the next save overwrites it.
            return [];
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_filePath))!);

        // Write to a temp file then swap, so a crash mid-write never leaves a half-written history.
        var tempPath = _filePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(_records, SerializerOptions));
        File.Move(tempPath, _filePath, overwrite: true);
    }
}
