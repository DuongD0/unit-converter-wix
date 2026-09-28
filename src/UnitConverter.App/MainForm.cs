using System.Globalization;
using UnitConverter.Core;

namespace UnitConverter.App;

/// <summary>Main window. Handles presentation only and delegates all work to injected services.</summary>
internal sealed class MainForm : Form
{
    private readonly ConversionService _converter;
    private readonly IConversionHistory _history;

    private readonly ComboBox _categoryBox = CreateDropDown();
    private readonly ComboBox _fromBox = CreateDropDown();
    private readonly ComboBox _toBox = CreateDropDown();
    private readonly TextBox _valueBox = new() { Dock = DockStyle.Fill, Text = "1" };
    private readonly Label _resultLabel = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        Font = new Font(SystemFonts.MessageBoxFont!.FontFamily, 14f, FontStyle.Bold),
        Padding = new Padding(0, 8, 0, 8),
    };
    private readonly ListBox _historyList = new() { Dock = DockStyle.Fill, IntegralHeight = false };

    public MainForm(IUnitCatalog catalog, ConversionService converter, IConversionHistory history)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _history = history ?? throw new ArgumentNullException(nameof(history));

        Text = "Unit Converter";
        MinimumSize = new Size(560, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = SystemFonts.MessageBoxFont!;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        AcceptButton = BuildLayout();

        _categoryBox.SelectedIndexChanged += (_, _) => ShowUnitsOf(SelectedCategory);
        _categoryBox.DataSource = catalog.Categories.ToList();

        RefreshHistory();
    }

    private UnitCategory SelectedCategory => (UnitCategory)_categoryBox.SelectedItem!;

    private Button BuildLayout()
    {
        var convertButton = CreateButton("&Convert", (_, _) => ConvertCurrentInput());
        var swapButton = CreateButton("&Swap ⇅", (_, _) => SwapUnits());
        var clearButton = CreateButton("C&lear history", (_, _) => ClearHistory());

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Category", _categoryBox);
        AddRow(layout, "Value", _valueBox);
        AddRow(layout, "From", _fromBox);
        AddRow(layout, "To", _toBox);
        AddRow(layout, string.Empty, CreateButtonRow(convertButton, swapButton));
        AddRow(layout, "Result", _resultLabel);
        AddRow(layout, "History", _historyList, fill: true);
        AddRow(layout, string.Empty, CreateButtonRow(clearButton));

        Controls.Add(layout);
        return convertButton;
    }

    private void ShowUnitsOf(UnitCategory category)
    {
        _fromBox.DataSource = category.Units.ToList();
        _toBox.DataSource = category.Units.ToList();
        _toBox.SelectedIndex = Math.Min(1, category.Units.Count - 1);
        _resultLabel.Text = string.Empty;
    }

    private void ConvertCurrentInput()
    {
        if (!double.TryParse(_valueBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value)
            || !double.IsFinite(value))
        {
            ShowError($"'{_valueBox.Text}' is not a valid number.");
            _valueBox.Focus();
            return;
        }

        var record = _converter.Convert(
            SelectedCategory, (IUnit)_fromBox.SelectedItem!, (IUnit)_toBox.SelectedItem!, value);

        _resultLabel.Text = string.Format(
            CultureInfo.CurrentCulture, "{0:G10} {1} = {2:G10} {3}",
            record.Input, record.FromSymbol, record.Output, record.ToSymbol);

        RefreshHistory();
    }

    private void SwapUnits() =>
        (_fromBox.SelectedIndex, _toBox.SelectedIndex) = (_toBox.SelectedIndex, _fromBox.SelectedIndex);

    private void ClearHistory()
    {
        _history.Clear();
        RefreshHistory();
    }

    private void RefreshHistory() => _historyList.DataSource = _history.GetAll().ToList();

    private void ShowError(string message) =>
        MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static void AddRow(TableLayoutPanel layout, string caption, Control control, bool fill = false)
    {
        layout.RowStyles.Add(fill ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(
            new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) });
        layout.Controls.Add(control);
    }

    private static FlowLayoutPanel CreateButtonRow(params Button[] buttons)
    {
        var row = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false };
        row.Controls.AddRange(buttons);
        return row;
    }

    private static Button CreateButton(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 2, 8, 2) };
        button.Click += onClick;
        return button;
    }

    private static ComboBox CreateDropDown() =>
        new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
}
