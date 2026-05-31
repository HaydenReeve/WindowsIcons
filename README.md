# Windows 11 Icon Pack

A complete icon pack containing icons from Windows 11 and other Microsoft products, organised into categorised `.ico` files.

## Usage

Access the icons in three ways:

- Download the repository and use the individual `.ico` files directly from `Icons/`.
- Run `dotnet build` to produce a managed .NET 8 assembly with icons embedded as manifest resources.
- Run the native build target to produce a resource-only DLL suitable for shell-style icon browsing.

## Building

### Managed assembly

```sh
dotnet build -c Release
```

Produces `bin/Release/net8.0/WindowsIcons.dll` with all icons as embedded resources for .NET consumption.

### Native icon DLL

```sh
dotnet msbuild -t:BuildNativeIconDll -p:Configuration=Release -p:NativeDllMachine=x64
```

Produces `bin/Release/native/x64/WindowsIcons.dll` — a native, resource-only DLL browsable through standard Windows shell APIs (`PickIconDlg`, `ExtractIconEx`, etc.). No `rc.exe` or `link.exe` required; the build uses the `tools/WindowsIcons.NativeBuild` helper with AsmResolver.

Supported machine targets: `x64`, `x86`, `arm64`.

## Icon categories

| Category | Count |
|---|---|
| applications | 181 |
| devices | 84 |
| emblems | 66 |
| files | 111 |
| folders | 39 |
| objects | 40 |

## Sources

### Built-in Windows

- Windows Apps
- `imageres.dll`
- `shell32.dll`
- `ddores.dll`
- Other `system32` locations

### Third-party

- [Superfolders](https://github.com/pronoy2108/Superfolders/tree/v4.0) — start, taskview, widgets, search, and volume icons.
- [Sysinternals](https://docs.microsoft.com/en-us/sysinternals/) — autoruns, packetviewer, processmonitor, and windowsobject icons.
- [PowerToys](https://docs.microsoft.com/en-us/windows/powertoys/) — PowerToys icon.
