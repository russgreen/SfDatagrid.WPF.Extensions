# SfDatagrid.WPF.Extensions

Extension methods and behaviors for Syncfusion WPF `SfDataGrid`.

## Features

### Ctrl+Drag Fill Behavior

An attached behavior that enables Excel-like drag-fill functionality with modifier keys:

- **Ctrl+Drag**: Fills selected cells downward in the same column with the source cell's value
- **Ctrl+Shift+Drag**: Fills downward with auto-incremented values (for numeric trailing digits and supported numeric types)
- **Configurable Modifier Key**: Choose the modifier key that activates drag-fill (default: Ctrl)
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
using System.Windows.Input;

public MainWindow()
{
	InitializeComponent();

	this.ordersGrid.EnableCtrlDragFill();
}
```

To use a different modifier key instead of the default Ctrl, pass the `requiredModifiers` parameter:
```csharp
using SfDatagrid.WPF.Extensions;
using System.Windows.Input;

public MainWindow()
{
	InitializeComponent();

	this.ordersGrid.EnableCtrlDragFill(requiredModifiers: ModifierKeys.Alt);
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

You can also configure the modifier key in XAML using the attached property:
```xml
<Window x:Class="MyApp.MainWindow"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:behaviors="clr-namespace:SfDatagrid.WPF.Extensions.Behaviors;assembly=SfDatagrid.WPF.Extensions">
	<sfgrid:SfDataGrid ItemsSource="{Binding Orders}"
					  behaviors:CtrlDragFillBehavior.IsEnabled="True"
					  behaviors:CtrlDragFillBehavior.RequiredModifiers="Alt" />
</Window>
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