using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SfDatagrid.WPF.Extensions.Behaviors;

/// <summary>
/// Attached behavior that enables Ctrl+Drag fill functionality for SfDataGrid without interfering with normal row selection.
/// </summary>
public static class CtrlDragFillBehavior
{
    private class DragState
    {
        public Point? PressedPosition { get; set; }
        public bool TriggerModifiersHeldAtPress { get; set; }
        public bool ShiftHeldAtPress { get; set; }
        public bool IsDragging { get; set; }
        public ModifierKeys RequiredModifiersAtPress { get; set; }
    }

    public static readonly DependencyProperty RequiredModifiersProperty =
        DependencyProperty.RegisterAttached(
            "RequiredModifiers",
            typeof(ModifierKeys),
            typeof(CtrlDragFillBehavior),
            new PropertyMetadata(ModifierKeys.Control));

    public static ModifierKeys GetRequiredModifiers(DependencyObject obj)
    {
        return (ModifierKeys)obj.GetValue(RequiredModifiersProperty);
    }

    public static void SetRequiredModifiers(DependencyObject obj, ModifierKeys value)
    {
        obj.SetValue(RequiredModifiersProperty, value);
    }

    private const double DragThreshold = 4.0;
    private static readonly Dictionary<SfDataGrid, DragState> States = new();

    #region Attached Properties

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(CtrlDragFillBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    public static readonly DependencyProperty ColumnFilterProperty =
        DependencyProperty.RegisterAttached(
            "ColumnFilter",
            typeof(Func<string, bool>),
            typeof(CtrlDragFillBehavior),
            new PropertyMetadata(null));

    public static Func<string, bool>? GetColumnFilter(DependencyObject obj)
    {
        return (Func<string, bool>?)obj.GetValue(ColumnFilterProperty);
    }

    public static void SetColumnFilter(DependencyObject obj, Func<string, bool>? value)
    {
        obj.SetValue(ColumnFilterProperty, value);
    }

    #endregion

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SfDataGrid dataGrid)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            AttachBehavior(dataGrid);
        }
        else
        {
            DetachBehavior(dataGrid);
        }
    }

    private static void AttachBehavior(SfDataGrid dataGrid)
    {
        States[dataGrid] = new DragState();
        dataGrid.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        dataGrid.PreviewMouseMove += OnPreviewMouseMove;
        dataGrid.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        dataGrid.Unloaded += OnDataGridUnloaded;
    }

    private static void DetachBehavior(SfDataGrid dataGrid)
    {
        dataGrid.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        dataGrid.PreviewMouseMove -= OnPreviewMouseMove;
        dataGrid.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        dataGrid.Unloaded -= OnDataGridUnloaded;
        States.Remove(dataGrid);
    }

    private static void OnDataGridUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is SfDataGrid dataGrid)
        {
            DetachBehavior(dataGrid);
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not SfDataGrid dataGrid || !States.TryGetValue(dataGrid, out var state))
        {
            return;
        }

        var requiredModifiers = GetRequiredModifiers(dataGrid);

        state.PressedPosition = e.GetPosition(dataGrid);
        state.RequiredModifiersAtPress = requiredModifiers;
        state.TriggerModifiersHeldAtPress = HasRequiredModifiers(requiredModifiers);
        state.ShiftHeldAtPress = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        state.IsDragging = false;

        Debug.WriteLine($"[CtrlDragFill] MouseDown: RequiredModifiers={requiredModifiers}, Shift={state.ShiftHeldAtPress}");
    }

    private static bool HasRequiredModifiers(ModifierKeys requiredModifiers)
    {
        if (requiredModifiers == ModifierKeys.None)
        {
            return true;
        }

        return (Keyboard.Modifiers & requiredModifiers) == requiredModifiers;
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not SfDataGrid dataGrid || !States.TryGetValue(dataGrid, out var state))
        {
            return;
        }

        if (state.PressedPosition is null || !state.TriggerModifiersHeldAtPress || !HasRequiredModifiers(state.RequiredModifiersAtPress))
        {
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(dataGrid);
        var dx = currentPosition.X - state.PressedPosition.Value.X;
        var dy = currentPosition.Y - state.PressedPosition.Value.Y;

        if ((dx * dx + dy * dy) >= (DragThreshold * DragThreshold))
        {
            if (!state.IsDragging)
            {
                Debug.WriteLine($"[CtrlDragFill] Drag detected! Distance: {Math.Sqrt(dx * dx + dy * dy):F1}");
            }
            state.IsDragging = true;
        }
    }

    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not SfDataGrid dataGrid || !States.TryGetValue(dataGrid, out var state))
        {
            return;
        }

        try
        {
            Debug.WriteLine($"[CtrlDragFill] MouseUp: RequiredModifiers={state.RequiredModifiersAtPress}, Dragging={state.IsDragging}");

            // Only proceed if the configured modifier keys were held and a real drag occurred
            if (!state.TriggerModifiersHeldAtPress || !HasRequiredModifiers(state.RequiredModifiersAtPress) || !state.IsDragging)
            {
                return;
            }

            Debug.WriteLine($"[CtrlDragFill] Processing drag fill...");

            // Capture locals before the finally block clears state
            var capturedIncrement = state.ShiftHeldAtPress;

            // Use Background priority to ensure Syncfusion's selection has fully settled.
            // CurrentColumn is read inside ProcessDragFill so Syncfusion has time to set it correctly.
            dataGrid.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => ProcessDragFill(dataGrid, capturedIncrement)));
        }
        finally
        {
            state.PressedPosition = null;
            state.TriggerModifiersHeldAtPress = false;
            state.ShiftHeldAtPress = false;
            state.IsDragging = false;
            state.RequiredModifiersAtPress = ModifierKeys.None;
        }
    }

    private static void ProcessDragFill(SfDataGrid dataGrid, bool incrementOnFill)
    {
        try
        {
            // Read CurrentColumn here — by Background priority Syncfusion has set it correctly.
            var targetColumn = dataGrid.CurrentColumn;

            Debug.WriteLine($"[CtrlDragFill] ProcessDragFill called. Column: {targetColumn?.MappingName}, Increment: {incrementOnFill}");

            if (targetColumn is null)
            {
                Debug.WriteLine("[CtrlDragFill] CurrentColumn is null, exiting");
                return;
            }

            var columnFilter = GetColumnFilter(dataGrid);
            var mappingName = targetColumn.MappingName;

            if (string.IsNullOrWhiteSpace(mappingName))
            {
                Debug.WriteLine("[CtrlDragFill] MappingName is empty, exiting");
                return;
            }

            // Skip selector columns
            if (targetColumn is GridCheckBoxSelectorColumn)
            {
                Debug.WriteLine("[CtrlDragFill] Selector column, exiting");
                return;
            }

            // Skip readonly columns
            if (targetColumn.IsReadOnly || !targetColumn.AllowEditing)
            {
                Debug.WriteLine("[CtrlDragFill] Column is readonly, exiting");
                return;
            }

            // Apply column filter if provided
            if (columnFilter != null && !columnFilter(mappingName))
            {
                Debug.WriteLine("[CtrlDragFill] Column filtered out, exiting");
                return;
            }

            // Get selected rows (works with row selection mode)
            var selectedRows = dataGrid.SelectedItems?.Cast<object>().ToList();
            Debug.WriteLine($"[CtrlDragFill] Selected rows: {selectedRows?.Count ?? 0}");

            if (selectedRows == null || selectedRows.Count < 2)
            {
                Debug.WriteLine("[CtrlDragFill] Not enough selected rows, exiting");
                return;
            }

            // Sort selected rows by record index
            var sortedRows = selectedRows
                .Select(row => new { Row = row, Index = GetRecordIndex(dataGrid, row) })
                .Where(x => x.Index >= 0)
                .OrderBy(x => x.Index)
                .ToList();

            Debug.WriteLine($"[CtrlDragFill] Sorted rows: {sortedRows.Count}");

            if (sortedRows.Count < 2)
            {
                Debug.WriteLine("[CtrlDragFill] Not enough sorted rows, exiting");
                return;
            }

            var sourceRow = sortedRows[0].Row;
            var sourceRecordIndex = sortedRows[0].Index;

            var rowType = sourceRow.GetType();
            var property = rowType.GetProperty(mappingName);
            if (property is null || !property.CanRead || !property.CanWrite)
            {
                Debug.WriteLine($"[CtrlDragFill] Property '{mappingName}' not found or not accessible, exiting");
                return;
            }

            var sourceValue = property.GetValue(sourceRow);
            Debug.WriteLine($"[CtrlDragFill] Source value: {sourceValue}");

            var assignments = new List<(object Row, object? OldValue, object? NewValue)>();

            for (int i = 1; i < sortedRows.Count; i++)
            {
                var targetRow = sortedRows[i].Row;
                var targetRecordIndex = sortedRows[i].Index;
                var oldValue = property.GetValue(targetRow);
                var valueToSet = sourceValue;

                if (incrementOnFill)
                {
                    var increment = targetRecordIndex - sourceRecordIndex;
                    if (TryGetIncrementedValue(sourceValue, increment, property.PropertyType, out var incrementedValue))
                    {
                        valueToSet = incrementedValue;
                    }
                }

                assignments.Add((targetRow, oldValue, valueToSet));
            }

            Debug.WriteLine($"[CtrlDragFill] Assignments to apply: {assignments.Count}");

            if (assignments.Count > 0)
            {
                ApplyAssignments(dataGrid, targetColumn, property, assignments);
                Debug.WriteLine("[CtrlDragFill] Assignments applied successfully");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CtrlDragFill] Exception: {ex.Message}");
        }
    }

    private static void ApplyAssignments(SfDataGrid dataGrid, GridColumn column, PropertyInfo property, List<(object Row, object? OldValue, object? NewValue)> assignments)
    {
        var raiseValidated = dataGrid.GetType().GetMethod(
            "RaiseCurrentCellValidatedEvent",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        foreach (var (row, oldValue, newValue) in assignments)
        {
            try
            {
                property.SetValue(row, newValue);

                if (raiseValidated != null)
                        {
                            var args = new CurrentCellValidatedEventArgs(dataGrid);
                            var argsType = args.GetType();
                            argsType.GetProperty("Column")?.SetValue(args, column);
                            argsType.GetProperty("RowData")?.SetValue(args, row);
                            argsType.GetProperty("OldValue")?.SetValue(args, oldValue);
                            argsType.GetProperty("NewValue")?.SetValue(args, newValue);
                            raiseValidated.Invoke(dataGrid, new object[] { args });
                        }
            }
            catch
            {
                // Suppress individual assignment errors
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

    private static int GetRecordIndex(SfDataGrid dataGrid, object? rowData)
    {
        if (rowData is null || dataGrid.View?.Records is null)
        {
            return -1;
        }

        for (var i = 0; i < dataGrid.View.Records.Count; i++)
        {
            if (dataGrid.View.Records[i] is RecordEntry entry && ReferenceEquals(entry.Data, rowData))
            {
                return i;
            }
        }

        return -1;
    }
}
