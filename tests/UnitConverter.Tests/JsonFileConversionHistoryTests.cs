using UnitConverter.Core;
using UnitConverter.Persistence;

namespace UnitConverter.Tests;

public sealed class JsonFileConversionHistoryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    private string FilePath => Path.Combine(_directory, "history.json");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void History_PersistsAcrossInstances()
    {
        new JsonFileConversionHistory(FilePath).Add(Sample(1));

        var reloaded = new JsonFileConversionHistory(FilePath);

        Assert.Equal([Sample(1)], reloaded.GetAll());
    }

    [Fact]
    public void History_KeepsNewestFirstAndRespectsCapacity()
    {
        var history = new JsonFileConversionHistory(FilePath, capacity: 2);

        history.Add(Sample(1));
        history.Add(Sample(2));
        history.Add(Sample(3));

        Assert.Equal([Sample(3), Sample(2)], history.GetAll());
    }

    [Fact]
    public void History_TreatsCorruptFileAsEmpty()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(FilePath, "{ not valid json");

        Assert.Empty(new JsonFileConversionHistory(FilePath).GetAll());
    }

    [Fact]
    public void Clear_RemovesPersistedRecords()
    {
        var history = new JsonFileConversionHistory(FilePath);
        history.Add(Sample(1));

        history.Clear();

        Assert.Empty(new JsonFileConversionHistory(FilePath).GetAll());
    }

    private static ConversionRecord Sample(double input) =>
        new("Length", input, "m", input * 100, "cm", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
}
