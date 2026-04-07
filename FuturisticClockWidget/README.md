# Futuristic Clock Widget

A modern, futuristic desktop clock widget with glassmorphism design and neon effects.

## Features

- **Real-time Updates**: Updates every 10ms for smooth millisecond display
- **Futuristic Design**: Glassmorphism effect with neon cyan and magenta accents
- **Draggable**: Click and drag to position anywhere on your desktop
- **Always on Top**: Stays visible above other windows
- **Minimalist**: Clean, unobtrusive design that complements any desktop
- **Keyboard Shortcuts**: 
  - `Escape`: Close the widget
  - `Space`: Toggle "always on top" behavior

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

- **Glassmorphism Effect**: Semi-transparent background with blur
- **Neon Glow**: Cyan primary color with magenta accents
- **Pulse Animation**: Subtle breathing effect on the main time display
- **Drop Shadow**: Soft glow effect for depth
- **Rounded Corners**: Modern 15px border radius

## Time Display

- **Main Time**: HH:mm:ss format in large cyan text
- **Date**: Full day and date below the time
- **Milliseconds**: Small magenta text showing precision timing

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
