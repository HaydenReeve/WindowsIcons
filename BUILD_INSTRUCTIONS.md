# Building WindowsIcons.dll

This document explains how to build the WindowsIcons.dll file as a Win32 resource-only DLL that is compatible with Windows Explorer's icon picker.

## Understanding the Issue

The original v3.0.0 approach used a .NET SDK project that embedded .ico files as managed resources (`<EmbeddedResource>`). While these resources are accessible to .NET applications via reflection APIs, they are **not** accessible to Windows native applications like:
- Windows Explorer's "Change Icon" dialog
- Icon extraction utilities
- Other Win32 applications that enumerate icon resources

This is because Windows icon browsers expect **Win32 native ICON resources** (resource type `RT_GROUP_ICON`), not .NET embedded resources.

## The Solution

The proper way to create an icon library DLL for Windows is to:

1. Create a Win32 resource script (`.rc` file) that references all icon files
2. Compile the resource script into a `.res` file using RC.EXE (Resource Compiler)
3. Link the `.res` file into a resource-only DLL using LINK.EXE with the `/NOENTRY` flag

This creates a DLL that contains only Win32 resources (no executable code) and is fully compatible with all Windows icon browsing tools.

## Automated Build Process

The repository includes an automated build script that handles all the steps:

### BuildResourceDll.bat

This batch script:
1. Runs `GenerateResourceScript.ps1` to scan the Icons directory and generate:
   - `WindowsIcons.rc` - Resource script with all icon definitions
   - `resource.h` - Header file with resource ID definitions
2. Locates Visual Studio or Windows SDK tools (RC.EXE and LINK.EXE)
3. Compiles the resource script: `rc.exe /fo WindowsIcons.res WindowsIcons.rc`
4. Links into a DLL: `link.exe /DLL /NOENTRY /MACHINE:X64 /OUT:bin/WindowsIcons.dll WindowsIcons.res`

### GenerateResourceScript.ps1

This PowerShell script:
- Recursively scans the `Icons/` directory for all `.ico` files
- Generates unique resource IDs for each icon (starting from 101)
- Creates symbolic names based on the file paths (e.g., `IDI_ICONS_FOLDERS_FOLDER_ICO`)
- Writes `WindowsIcons.rc` with ICON resource definitions
- Writes `resource.h` with `#define` statements for each resource ID

## Requirements

To build the DLL, you need one of the following installed on Windows:

1. **Visual Studio 2017 or later** with "Desktop development with C++" workload
2. **Visual Studio Build Tools** (lightweight, command-line only)
3. **Windows SDK** (provides RC.EXE and LINK.EXE)

The build script will automatically detect and use these tools.

## Running the Build

### Option 1: Run BuildResourceDll.bat directly

```cmd
cd C:\path\to\WindowsIcons
BuildResourceDll.bat
```

The script will handle everything automatically, including setting up the Visual Studio environment.

### Option 2: Manual build from Developer Command Prompt

If you prefer to build manually, open a "Developer Command Prompt for VS" and run:

```cmd
cd C:\path\to\WindowsIcons

REM Generate resource files
powershell -ExecutionPolicy Bypass -File GenerateResourceScript.ps1

REM Compile resources
rc.exe /nologo /fo WindowsIcons.res WindowsIcons.rc

REM Link into DLL
mkdir bin
link.exe /DLL /NOENTRY /MACHINE:X64 /OUT:bin\WindowsIcons.dll WindowsIcons.res
```

## Output

The build creates:
- **`bin/WindowsIcons.dll`** - The Win32 resource-only DLL (compatible with Windows icon browsers)
- `WindowsIcons.res` - Compiled resource file (intermediate, can be deleted)
- `WindowsIcons.rc` - Generated resource script (can be regenerated)
- `resource.h` - Generated header file (can be regenerated)

## Using the DLL

Once built, you can use the DLL in Windows:

1. **In Windows Explorer:**
   - Right-click a folder → Properties → Customize → Change Icon
   - Click Browse and select `bin/WindowsIcons.dll`
   - You'll see all 521 icons available to choose from

2. **In applications:**
   - Any application that uses the Windows icon picker will be able to browse and select icons from this DLL

3. **Programmatically:**
   - Use Win32 APIs like `ExtractIcon`, `LoadIcon`, or `SHGetFileInfo` with the DLL path

## Troubleshooting

### "RC.EXE not found"
Install Visual Studio with C++ tools, or the Windows SDK.

### "Access denied" when running PowerShell script
Run: `powershell -ExecutionPolicy Bypass -File GenerateResourceScript.ps1`

### DLL builds but icons don't show in Explorer
- Make sure you're using `bin/WindowsIcons.dll`, not `bin/Debug/net8.0/WindowsIcons.dll`
- The .NET version is NOT compatible with Windows icon browsers

### Build fails with linker errors
- Make sure you're running from a Developer Command Prompt, or
- Let `BuildResourceDll.bat` handle the Visual Studio environment setup automatically

## Technical Details

### Resource ID Assignment

Icons are assigned sequential resource IDs starting from 101:
- First icon: 101
- Second icon: 102
- ...
- 521st icon: 621

### Resource Names

Each icon gets a symbolic name derived from its file path:
- `Icons/folders/folder.ico` → `IDI_ICONS_FOLDERS_FOLDER_ICO`
- Spaces, dashes, dots (except extension) are replaced with underscores
- Names are converted to uppercase

### DLL Structure

The resulting DLL is a PE32+ executable with:
- No code sections (resource-only)
- ICON resources of type RT_GROUP_ICON
- Each icon retains its full color depth and all icon sizes
- Total size: ~81 MB (all 521 icons embedded)

## Legacy .NET Project

The `WindowsIcons.csproj` file remains in the repository for backward compatibility with v3.0.0, but it produces a .NET assembly that is NOT compatible with Windows icon browsers. It should not be used for creating the icon library DLL.

Users who need programmatic access to icons via .NET can still use that project, but for Windows icon picker compatibility, use the Win32 DLL built by `BuildResourceDll.bat`.
