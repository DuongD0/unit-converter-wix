using UnitConverter.Core;
using UnitConverter.Persistence;
using UnitConverter.Units;

namespace UnitConverter.App;

internal static class Program
{
    private const string AppName = "UnitConverter";

    /// <summary>
    /// Composition root: the only place that chooses concrete implementations.
    /// Swapping the catalog or the history store means changing one line here.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        IUnitCatalog catalog = new StandardUnitCatalog();
        IConversionHistory history = JsonFileConversionHistory.ForCurrentUser(AppName);
        var converter = new ConversionService(history, TimeProvider.System);

        Application.Run(new MainForm(catalog, converter, history));
    }
}
