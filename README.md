# Windows 11 Icon Pack

A complete pack with lots of icons from various places in windows 11 as well as other microsoft products.

## Instructions

You can access the icons in one of three ways:

### Option 1: Direct Icon Files
Simply download the repo and grab the icons you want, handily stored in a .ico format in the `Icons/` directory.

### Option 2: Pre-compiled DLL
Download the pre-compiled `WindowsIcons.dll` from the [releases page](https://github.com/HaydenReeve/WindowsIcons/releases) and use it with Windows Explorer's icon picker (Change Icon dialog), just like how Windows icon libraries (shell32.dll, imageres.dll) work natively.

### Option 3: Build the DLL Yourself

**Requirements:**
- Windows operating system
- .NET 8.0 SDK
- Visual Studio 2017+ with C++ tools, Visual Studio Build Tools, or Windows SDK (for RC.EXE and LINK.EXE)

**Build Steps:**

1. Clone or download this repository
2. Open a Command Prompt in the repository root directory
3. Run: `BuildResourceDll.bat`

The script builds a C# resource generator, scans all .ico files, and produces `bin/WindowsIcons.dll` with Win32 native ICON resources compatible with Windows Explorer's icon picker and other Win32 applications.

## Sources

### Built in

- windowsApps,
- imageres.dll,
- shell32.dll,
- ddores.dll*,
- other system32 locations.

### Superfolders

Superfolders for the start, taskview, widgets, search, and volume icons.

https://github.com/pronoy2108/Superfolders/tree/v4.0

### Sysinternals

Sysinternals for internal, autoruns, packetviewer, processmonitor, and windowsobject icons.

https://docs.microsoft.com/en-us/sysinternals/

### Powertoys

Powertoys for the powertoys icon.

https://docs.microsoft.com/en-us/windows/powertoys/

## Changelog

### v3.1.0

- **Fixed**: DLL now uses Win32 native ICON resources instead of .NET embedded resources
- Icons are now accessible to Windows Explorer's icon picker and other Windows icon browsers
- Resource script generation reimplemented as C# console application (ResourceScriptGenerator)
- Automated build via BuildResourceDll.bat

### v3.0.0

- Wrapped icons in a .net8 .dll package for easier consumption.
- Updated documentation.

### v2.0.1

- Unpacked .zip in preparation for conversion into central .dll files for ease of use.
- Updated documentation.