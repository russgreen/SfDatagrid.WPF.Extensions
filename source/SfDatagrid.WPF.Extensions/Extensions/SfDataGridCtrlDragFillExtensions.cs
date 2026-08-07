using SfDatagrid.WPF.Extensions.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Windows.Input;

namespace SfDatagrid.WPF.Extensions;

/// <summary>
/// Provides extension methods for configuring Ctrl+Drag fill behavior on <see cref="SfDataGrid"/> instances.
/// </summary>
public static class SfDataGridCtrlDragFillExtensions
{
    /// <summary>
    /// Enables Ctrl+Drag fill functionality on the SfDataGrid using an attached behavior.
    /// This does not interfere with normal row selection or the SelectionController.
    /// </summary>
    /// <param name="dataGrid">The SfDataGrid instance.</param>
    /// <param name="columnFilter">An optional filter to determine which columns are affected.</param>
    /// <param name="requiredModifiers">The modifier keys that must be pressed to activate drag fill.</param>
    public static void EnableCtrlDragFill(
        this SfDataGrid dataGrid,
        Func<string, bool>? columnFilter = null,
        ModifierKeys requiredModifiers = ModifierKeys.Control)
    {
        CtrlDragFillBehavior.SetIsEnabled(dataGrid, true);
        CtrlDragFillBehavior.SetRequiredModifiers(dataGrid, requiredModifiers);
        if (columnFilter != null)
        {
            CtrlDragFillBehavior.SetColumnFilter(dataGrid, columnFilter);
        }
    }

    /// <summary>
    /// Disables Ctrl+Drag fill functionality on the SfDataGrid.
    /// </summary>
    /// <param name="dataGrid">The SfDataGrid instance.</param>
    public static void DisableCtrlDragFill(this SfDataGrid dataGrid)
    {
        CtrlDragFillBehavior.SetIsEnabled(dataGrid, false);
    }
}
