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
- Visual Studio 2017 or later with C++ tools, OR
- Visual Studio Build Tools, OR
- Windows SDK with RC.EXE and LINK.EXE

**Build Steps:**

1. Clone or download this repository
2. Open a Command Prompt in the repository root directory
3. Run the build script:
   ```cmd
   BuildResourceDll.bat
   ```

The build script will:
- Generate a resource script (`WindowsIcons.rc`) from all .ico files
- Compile the resources using RC.EXE
- Link them into a Win32 resource-only DLL (`bin/WindowsIcons.dll`)

The resulting DLL contains native Win32 ICON resources that are accessible to:
- Windows Explorer's "Change Icon" dialog
- Any application that uses Windows icon picker APIs
- Icon extraction tools

**Note:** The `.dll` file in `bin/Debug/net8.0/` is a .NET assembly with managed resources and is NOT compatible with Windows icon browsers. Use the `bin/WindowsIcons.dll` created by `BuildResourceDll.bat` instead.

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

- **Fixed**: Generated .dll now uses Win32 native ICON resources instead of .NET embedded resources
- Icons are now accessible to Windows Explorer's icon picker and other Windows icon browsers
- Added automated build script (`BuildResourceDll.bat`) to generate the resource-only DLL
- Added PowerShell script (`GenerateResourceScript.ps1`) to auto-generate resource files from Icons directory
- The .NET project remains for compatibility, but users should use `BuildResourceDll.bat` to create the proper Win32 DLL

### v3.0.0

- Wrapped icons in a .net8 .dll package for easier consumption.
- Updated documentation.

### v2.0.1

- Unpacked .zip in preparation for conversion into central .dll files for ease of use.
- Updated documentation.