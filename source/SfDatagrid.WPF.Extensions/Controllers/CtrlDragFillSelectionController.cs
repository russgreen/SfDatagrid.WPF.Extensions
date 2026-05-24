using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SfDatagrid.WPF.Extensions.Controllers;

public class CtrlDragFillSelectionController<TRow> : GridCellSelectionController
    where TRow : class
{
    private readonly Func<string, bool>? _columnFilter;

    // Position where the mouse was pressed; used to distinguish a real drag from a simple click.
    private Point? _pressedPosition;
    private bool _ctrlHeldAtPress;
    private bool _shiftHeldAtPress;

    private const double DragThreshold = 4.0;

    public CtrlDragFillSelectionController(SfDataGrid dataGrid, Func<string, bool>? columnFilter = null)
        : base(dataGrid)
    {
        _columnFilter = columnFilter;
    }

    protected override void ProcessPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
    {
        _pressedPosition = args.GetPosition(DataGrid);
        _ctrlHeldAtPress = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        _shiftHeldAtPress = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        base.ProcessPointerPressed(args, rowColumnIndex);
    }

    protected override void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex)
    {
        base.ProcessDragSelection(args, rowColumnIndex);
    }

    protected override void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
    {
        base.ProcessPointerReleased(args, rowColumnIndex);

        try
        {
            // Require Ctrl to have been held when the drag started.
            if (!_ctrlHeldAtPress)
            {
                return;
            }

            // Require the mouse to have actually moved enough to be a real drag.
            if (_pressedPosition is null)
            {
                return;
            }

            var released = args.GetPosition(DataGrid);
            var dx = released.X - _pressedPosition.Value.X;
            var dy = released.Y - _pressedPosition.Value.Y;
            if ((dx * dx + dy * dy) < (DragThreshold * DragThreshold))
            {
                return;
            }

            var selectedCells = GetSelectedCells()
                .OfType<GridCellInfo>()
                .Where(x => x.RowData is TRow)
                .ToList();

            if (selectedCells.Count < 2)
            {
                return;
            }

            // Source = top-most selected cell.
            var sourceCell = selectedCells
                .OrderBy(x => GetRecordIndex(x.RowData))
                .FirstOrDefault();

            if (sourceCell?.Column?.MappingName is not string mappingName || string.IsNullOrWhiteSpace(mappingName))
            {
                return;
            }

            if (sourceCell.Column.IsReadOnly || !sourceCell.Column.AllowEditing)
            {
                return;
            }

            if (_columnFilter != null && !_columnFilter(mappingName))
            {
                return;
            }

            var sourceRow = sourceCell.RowData as TRow;
            if (sourceRow is null)
            {
                return;
            }

            var property = typeof(TRow).GetProperty(mappingName);
            if (property is null || !property.CanRead || !property.CanWrite)
            {
                return;
            }

            var sourceValue = property.GetValue(sourceRow);
            var sourceRecordIndex = GetRecordIndex(sourceRow);
            var incrementOnFill = _shiftHeldAtPress;
            var assignments = new List<(TRow Row, object? Value)>();

            // Downward only + same column only.
            foreach (var cell in selectedCells.Where(c => c.Column?.MappingName == mappingName))
            {
                var targetRow = cell.RowData as TRow;
                if (targetRow is null || ReferenceEquals(targetRow, sourceRow))
                {
                    continue;
                }

                var targetRecordIndex = GetRecordIndex(targetRow);
                if (targetRecordIndex <= sourceRecordIndex)
                {
                    continue;
                }

                var valueToSet = sourceValue;

                if (incrementOnFill)
                {
                    var increment = targetRecordIndex - sourceRecordIndex;

                    if (TryGetIncrementedValue(sourceValue, increment, property.PropertyType, out var incrementedValue))
                    {
                        valueToSet = incrementedValue;
                    }
                }

                assignments.Add((targetRow, valueToSet));
            }

            // Defer value updates so they do not interfere with Syncfusion's pointer-release
            // selection state update (including the selector-column checkbox binding).
            // Use Background priority to ensure all Syncfusion internal updates complete first.
            if (assignments.Count > 0)
            {
                _ = DataGrid.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => ApplyAssignments(property, assignments)));
            }
        }
        finally
        {
            _pressedPosition = null;
            _ctrlHeldAtPress = false;
            _shiftHeldAtPress = false;
        }
    }

    private static void ApplyAssignments(PropertyInfo property, List<(TRow Row, object? Value)> assignments)
    {
        foreach (var (row, value) in assignments)
        {
            try
            {
                property.SetValue(row, value);
            }
            catch
            {
                // Suppress exceptions if Syncfusion internal state is still settling.
                // This prevents crashes from INotifyPropertyChanged timing conflicts.
            }
        }
    }

    private static bool TryGetIncrementedValue(object? sourceValue, int increment, Type propertyType, out object? incrementedValue)
    {
        incrementedValue = sourceValue;

        if (sourceValue is null)
        {
            return false;
        }

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (sourceValue is string sourceText)
        {
            var match = Regex.Match(sourceText, @"(\d+)$");
            if (!match.Success)
            {
                return false;
            }

            var numberText = match.Groups[1].Value;
            if (!long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                return false;
            }

            var nextNumber = number + increment;
            var nextNumberText = nextNumber.ToString($"D{numberText.Length}", CultureInfo.InvariantCulture);
            var prefixLength = sourceText.Length - numberText.Length;
            var prefix = prefixLength > 0 ? sourceText.Substring(0, prefixLength) : string.Empty;
            incrementedValue = prefix + nextNumberText;
            return true;
        }

        if (!TryConvertToDecimal(sourceValue, out var decimalValue))
        {
            return false;
        }

        var incrementedDecimal = decimalValue + increment;

        try
        {
            incrementedValue = targetType == typeof(decimal)
                ? incrementedDecimal
                : Convert.ChangeType(incrementedDecimal, targetType, CultureInfo.InvariantCulture);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryConvertToDecimal(object value, out decimal result)
    {
        switch (value)
        {
            case byte v:    result = v; return true;
            case sbyte v:   result = v; return true;
            case short v:   result = v; return true;
            case ushort v:  result = v; return true;
            case int v:     result = v; return true;
            case uint v:    result = v; return true;
            case long v:    result = v; return true;
            case ulong v:   result = v; return true;
            case float v:   result = (decimal)v; return true;
            case double v:  result = (decimal)v; return true;
            case decimal v: result = v; return true;
            default:        result = default; return false;
        }
    }

    private int GetRecordIndex(object? rowData)
    {
        if (rowData is null || DataGrid.View?.Records is null)
        {
            return -1;
        }

        for (var i = 0; i < DataGrid.View.Records.Count; i++)
        {
            if (DataGrid.View.Records[i] is RecordEntry entry && ReferenceEquals(entry.Data, rowData))
            {
                return i;
            }
        }

        return -1;
    }
}
