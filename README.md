# Windows 11 Icon Pack

A complete icon pack with icons from Windows 11 and other Microsoft products.

## Instructions

You can access the icons in three ways.

- Download the repo and use the individual `.ico` files.
- Run `dotnet build` to produce managed `net8.0` output with embedded manifest resources for .NET use.
- Run `dotnet msbuild -t:BuildNativeIconDll -p:Configuration=Release -p:NativeDllMachine=x64` to produce native `WindowsIcons.dll` output for shell-style icon browsing and other native consumers.

## Build outputs

### Managed assembly from `dotnet build`

`dotnet build` produces the existing managed assembly. This is a `net8.0` DLL that stores the icons as embedded manifest resources for managed .NET code.

This output is not a shell32-style native icon DLL.

### Native resource-only icon DLL from `BuildNativeIconDll`

`BuildNativeIconDll` produces a native, resource-only icon DLL. Use this output when you need shell32-style icon browsing behaviour instead of managed resources.

#### Build flow

`BuildNativeIconDll` runs a `.NET 10` helper project under `tools\WindowsIcons.NativeBuild` from MSBuild. The helper uses AsmResolver to write `WindowsIcons.dll` and inject real Win32 icon resources into the output DLL.

The result is native-icon-browsable through standard Windows APIs, even though the build is produced through managed tooling. No PowerShell, `rc.exe`, or `link.exe` is involved.

#### Recommended command

```text
dotnet msbuild -t:BuildNativeIconDll -p:Configuration=Release -p:NativeDllMachine=x64
```

#### Output location

Native output is written to:

- `bin\<Configuration>\native\<Machine>\WindowsIcons.dll`

#### Intermediate files

Native intermediate outputs are written under `obj\NativeIconDll\<Configuration>\<Machine>\`, including:

- `icons.rc`
- `icon-map.csv`

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

### v3.0.0

- Wrapped icons in a .net8 .dll package for easier consumption.
- Updated documentation.

### v2.0.1

- Unpacked .zip in preparation for conversion into central .dll files for ease of use.
- Updated documentation.
