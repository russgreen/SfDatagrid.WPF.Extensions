# SfDatagrid.WPF.Extensions

Extension methods and behaviors for Syncfusion WPF `SfDataGrid`.

## Features

### Ctrl+Drag Fill Behavior

An attached behavior that enables Excel-like drag-fill functionality with modifier keys:

- **Ctrl+Drag**: Fills selected cells downward in the same column with the source cell's value
- **Ctrl+Shift+Drag**: Fills downward with auto-incremented values (for numeric trailing digits and supported numeric types)
- **Column Filter**: Optional predicate to restrict which columns support fill operations
- **ReadOnly Detection**: Automatically skips fill operations on columns marked with `IsReadOnly=True` or `AllowEditing=False`
- **Non-Intrusive**: Uses mouse event handlers instead of replacing the SelectionController, preserving normal row selection behavior

Supports numeric auto-increment on trailing digits in strings and numeric data types (`int`, `double`, `decimal`, etc.). The behavior respects the grid column's read-only state and prevents modifications to read-only columns.

## Project Configuration

The primary package is defined in `source/SfDatagrid.WPF.Extensions/SfDatagrid.WPF.Extensions.csproj`:

- **Target Frameworks**: `net48`, `net8.0-windows`, `net9.0-windows`, `net10.0-windows`
- **Package ID**: `SfDatagrid.WPF.Extensions`
- **License**: MIT
- **Symbols**: Included (`.snupkg` format)
- **Documentation**: Generated from XML comments
- **Dependencies**: `Syncfusion.SfGrid.WPF` (v29.0 - v33.x)

The project generates both `.nupkg` (package) and `.snupkg` (symbol) files for debugging and NuGet consumption.

## Quick Start

Add the package reference to your project

```powershell
# Install packages
Install-Package SfDatagrid.WPF.Extensions
```

```xml
<ItemGroup>
  <PackageReference Include="SfDatagrid.WPF.Extensions" Version="*" />
</ItemGroup>	
```

Enable the behavior in the code-behind of your WPF window:
```csharp
using SfDatagrid.WPF.Extensions;

public MainWindow()
{
	InitializeComponent();

	this.ordersGrid.EnableCtrlDragFill();
}
```

To restrict to specific columns, use the `columnFilter` parameter:
```csharp
public MainWindow()
{
	InitializeComponent();

	this.ordersGrid.EnableCtrlDragFill(
		mappingName => mappingName == nameof(OrderInfo.Quantity));
}
```

To add the behavior to a details view grid, use a `DetailsViewLoading` event handler:
```csharp
private void SfDataGrid_DetailsViewLoading(object sender, DetailsViewLoadingAndUnloadingEventArgs e)
{
	if (e.DetailsViewDataGrid is not SfDataGrid detailsGrid)
	{
		return;
	}

	detailsGrid.EnableCtrlDragFill(
		mappingName => mappingName == nameof(RoomBoundaryModel.TypeCodeToUse));
}
```