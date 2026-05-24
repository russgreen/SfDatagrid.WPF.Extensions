# SfDatagrid.WPF.Extensions

Extension methods and selection controller helpers for Syncfusion WPF `SfDataGrid`.

## Features

### CtrlDragFillSelectionController

A custom cell selection controller that enables Excel-like drag-fill behavior with modifier keys:

- **Ctrl+Drag**: Fills selected cells downward in the same column with the source cell's value
- **Ctrl+Shift+Drag**: Fills downward with auto-incremented values (for numeric trailing digits and supported numeric types)
- **Column Filter**: Optional predicate to restrict which columns support fill operations
- **ReadOnly Detection**: Automatically skips fill operations on columns marked with `IsReadOnly=True`

Supports numeric auto-increment on trailing digits in strings and numeric data types (`int`, `double`, `decimal`, etc.). The controller respects the grid column's read-only state and prevents modifications to read-only columns.

## Project Configuration

The primary package is defined in `source/SfDatagrid.WPF.Extensions/SfDatagrid.WPF.Extensions.csproj`:

- **Target Frameworks**: `net8.0-windows`, `net10.0-windows`
- **Package ID**: `SfDatagrid.WPF.Extensions`
- **License**: MIT
- **Symbols**: Included (`.snupkg` format)
- **Documentation**: Generated from XML comments
- **Dependencies**: `Syncfusion.SfGrid.WPF` (v29.0 - v33.x)

The project generates both `.nupkg` (package) and `.snupkg` (symbol) files for debugging and NuGet consumption.

## Qucik Start

Add the pacakge reference to your project

```powershell
# Install packages
Install-Package SfDatagrid.WPF.Extensions
```

```csharp
	<ItemGroup>
	  <PackageReference Include="SfDatagrid.WPF.Extensions" Version="*" />
	</ItemGroup>	
```

Use the controller extension in the code-behind of your WPF window:
```csharp
    public MainWindow()
    {
        InitializeComponent();

        this.ordersGrid.UseCtrlDragFillSelectionController<OrderInfo>();
    }
```

To add to a specific column, use the `columnFilter` parameter:
```csharp
    public MainWindow()
    {
        InitializeComponent();

        this.ordersGrid.UseCtrlDragFillSelectionController<OrderInfo>(
            mappingName => mappingName == nameof(OrderInfo.Quantity));
    }
```

To add the extension to a details view grid, add a SfDataGrid_DetailsViewLoading event handler:
```csharp
    private void SfDataGrid_DetailsViewLoading(object sender, DetailsViewLoadingAndUnloadingEventArgs e)
    {
        if (e.DetailsViewDataGrid is not SfDataGrid detailsGrid)
        {
            return;
        }

        if (detailsGrid.SelectionController is CtrlDragFillSelectionController<RoomBoundaryModel>)
        {
            return;
        }

        detailsGrid.UseCtrlDragFillSelectionController<RoomBoundaryModel>(
            mappingName => mappingName == nameof(RoomBoundaryModel.TypeCodeToUse));
    }
```