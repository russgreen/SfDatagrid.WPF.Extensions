using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace SfDatagrid.WPF.Extensions.Controllers;

public class CtrlDragFillSelectionController<TRow> : GridCellSelectionController
    where TRow : class
{
    private readonly Func<string, bool>? _columnFilter;
    private bool _isDragged;
    private bool _ctrlPressedDuringDrag;
    private bool _shiftPressedDuringDrag;

    public CtrlDragFillSelectionController(SfDataGrid dataGrid, Func<string, bool>? columnFilter = null)
        : base(dataGrid) 
    {
        _columnFilter = columnFilter;
    }

    protected override void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex)
    {
        _isDragged = true;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _ctrlPressedDuringDrag = true;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            _shiftPressedDuringDrag = true;
        }

        base.ProcessDragSelection(args, rowColumnIndex);
    }

    protected override void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
    {
        base.ProcessPointerReleased(args, rowColumnIndex);

        try
        {
            // Must be drag + Ctrl held.
            if (!_isDragged || !_ctrlPressedDuringDrag || !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
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
            var incrementOnFill = _shiftPressedDuringDrag;

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

                property.SetValue(targetRow, valueToSet);
            }
        }
        finally
        {
            _isDragged = false;
            _ctrlPressedDuringDrag = false;
            _shiftPressedDuringDrag = false;
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
            incrementedValue = sourceText[..^numberText.Length] + nextNumberText;
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
            case byte byteValue:
                result = byteValue;
                return true;
            case sbyte sbyteValue:
                result = sbyteValue;
                return true;
            case short shortValue:
                result = shortValue;
                return true;
            case ushort ushortValue:
                result = ushortValue;
                return true;
            case int intValue:
                result = intValue;
                return true;
            case uint uintValue:
                result = uintValue;
                return true;
            case long longValue:
                result = longValue;
                return true;
            case ulong ulongValue:
                result = ulongValue;
                return true;
            case float floatValue:
                result = (decimal)floatValue;
                return true;
            case double doubleValue:
                result = (decimal)doubleValue;
                return true;
            case decimal decimalValue:
                result = decimalValue;
                return true;
            default:
                result = default;
                return false;
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
