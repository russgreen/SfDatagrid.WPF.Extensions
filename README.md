# SfDatagrid.WPF.Extensions

Extension methods and selection controller helpers for Syncfusion WPF `SfDataGrid`.

## Features

### CtrlDragFillSelectionController

A custom cell selection controller that enables Excel-like drag-fill behavior with modifier keys:

- **Ctrl+Drag**: Fills selected cells downward in the same column with the source cell's value
- **Ctrl+Shift+Drag**: Fills downward with auto-incremented values (for numeric trailing digits and supported numeric types)
- **Column Filter**: Optional predicate to restrict which columns support fill operations

Supports numeric auto-increment on trailing digits in strings and numeric data types (`int`, `double`, `decimal`, etc.).

## Project Configuration

The primary package is defined in `source/SfDatagrid.WPF.Extensions/SfDatagrid.WPF.Extensions.csproj`:

- **Target Frameworks**: `net8.0-windows`, `net10.0-windows`
- **Package ID**: `SfDatagrid.WPF.Extensions`
- **License**: MIT
- **Symbols**: Included (`.snupkg` format)
- **Documentation**: Generated from XML comments
- **Dependencies**: `Syncfusion.SfGrid.WPF` (v29.*)

The project generates both `.nupkg` (package) and `.snupkg` (symbol) files for debugging and NuGet consumption.
