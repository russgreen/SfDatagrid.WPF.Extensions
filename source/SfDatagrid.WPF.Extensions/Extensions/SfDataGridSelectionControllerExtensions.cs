using SfDatagrid.WPF.Extensions.Controllers;
using Syncfusion.UI.Xaml.Grid;
using System;

namespace SfDatagrid.WPF.Extensions.Extensions;

/// <summary>
/// Provides extension methods for configuring selection controllers on <see cref="SfDataGrid"/> instances.
/// </summary>
public static class SfDataGridSelectionControllerExtensions
{
    /// <summary>
    /// Configures the SfDataGrid to use the CtrlDragFillSelectionController for selection.
    /// </summary>
    /// <typeparam name="TRow">The type of the row.</typeparam>
    /// <param name="dataGrid">The SfDataGrid instance.</param>
    /// <param name="columnFilter">An optional filter to determine which columns are affected.</param>
    public static void UseCtrlDragFillSelectionController<TRow>(
        this SfDataGrid dataGrid,
        Func<string, bool>? columnFilter = null)
        where TRow : class
    {
        dataGrid.SelectionController = new CtrlDragFillSelectionController<TRow>(dataGrid, columnFilter);
    }
}