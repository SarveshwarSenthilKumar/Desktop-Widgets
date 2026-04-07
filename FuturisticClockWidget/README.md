# Futuristic Clock Widget

A modern, futuristic desktop clock widget with glassmorphism design and neon effects.

## Features

- **Real-time Updates**: Updates every 10ms for smooth millisecond display
- **Liquid Glass Design**: Ultra-minimalist with transparent layers and liquid animations
- **Futuristic Aesthetic**: Cyan neon glow with Segoe UI Light typography
- **Resizable**: Drag the bottom-right corner to resize - fonts scale automatically
- **Draggable**: Click and drag to position anywhere on your desktop
- **Always on Top**: Stays visible above other windows
- **Minimalist**: Clean, unobtrusive design that complements any desktop
- **Keyboard Shortcuts**: 
  - `Escape`: Close the widget
  - `Space`: Toggle "always on top" behavior
- **Right-Click Menu**: Right-click anywhere on the widget to open the context menu with:
  - Time format options (12-hour/24-hour)
  - Close option

## Quick Start

### Method 1: Using the Batch File (Easiest)
1. Double-click `StartClockWidget.bat`
2. The widget will appear on your desktop
3. Drag it to your preferred position

### Method 2: Using Command Line
```bash
cd FuturisticClockWidget
dotnet run
```

### Method 3: Building for Distribution
```bash
# Build as standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# The executable will be in: bin/Release/net8.0-windows/win-x64/publish/
```

## Design Features

- **Liquid Glass Effect**: Multi-layered transparent design with depth
- **Minimalist Typography**: Segoe UI Light for clean, modern appearance
- **Cyan Neon Glow**: Subtle cyan (#00FFFF) glow with soft blur
- **Liquid Animation**: Smooth sine-wave opacity animation (3-second cycle)
- **Rounded Corners**: 20px border radius for modern, soft appearance
- **Resizable Design**: Dynamic font scaling based on widget size
- **Layered Transparency**: Radial gradients and opacity masks for depth

### Resizing the Widget

The widget can be resized to fit your preferences:

1. **Move your cursor** to the bottom-right corner of the widget
2. **Look for the resize grip** - small diagonal lines in the corner
3. **Click and drag** to make the widget larger or smaller
4. **Font sizes scale automatically** to maintain readability
5. **Minimum size**: 200x100 pixels to ensure usability

**Font Scaling Behavior:**
- **Main time**: Scales from base 28px proportionally
- **Date text**: Scales from base 10px proportionally  
- **Small info**: Scales from base 8px proportionally
- **Maintains aspect ratio** for consistent appearance

## Time Display

- **Main Time**: Large cyan text (28px) showing current time
- **Full Date**: Complete date with day name (e.g., "Monday, January 15, 2024")
- **Week Information**: Current week number (e.g., "W03") and day of year (e.g., "Day 015")
- **Time Zone**: UTC offset and local time zone name (e.g., "UTC-05 Eastern")
- **Format Options**: 
  - **24-Hour (Military)**: 00:00:00 - 23:59:59 format
  - **12-Hour (Normal)**: 12:00:00 AM/PM - 11:59:59 PM format

### Changing Time Format

1. **Right-click** on the widget
2. **Select "Time Format"** from the context menu
3. **Choose your preferred format**:
   - "12-Hour (Normal)" for standard AM/PM format
   - "24-Hour (Military)" for military time format
4. **The display updates instantly** with your selected format

## System Requirements

- Windows 10/11
- .NET 8.0 Runtime (included in self-contained build)
- 2MB+ RAM usage
- Minimal CPU impact

## Customization

You can customize the widget by modifying the following files:

- **Colors**: Edit `App.xaml` to change the color scheme
- **Layout**: Modify `Views/MainWindow.xaml` for size and positioning
- **Update Rate**: Change the timer interval in `MainWindow.xaml.cs`

## Troubleshooting

**Widget doesn't appear:**
- Ensure .NET 8.0 is installed
- Try running as administrator
- Check Windows Defender if blocked

**Performance issues:**
- The widget uses minimal resources
- If experiencing lag, try reducing the update frequency

**Can't drag widget:**
- Make sure you're clicking on the widget body (not the edges)
- Try restarting the widget

## Building from Source

1. Install .NET 8.0 SDK
2. Clone or download this repository
3. Navigate to the FuturisticClockWidget folder
4. Run `dotnet build`
5. Run `dotnet run` to test

## License

Free to use and modify. Perfect for personal desktop customization!
