@echo off
REM Build script for creating WindowsIcons.dll as a Win32 resource-only DLL
REM This DLL contains native Win32 ICON resources accessible to Windows Explorer

setlocal enabledelayedexpansion

echo ===============================================
echo Windows Icons DLL Builder
echo ===============================================
echo.

REM Check if running on Windows
if not "%OS%"=="Windows_NT" (
    echo ERROR: This script must be run on Windows
    exit /b 1
)

REM Step 1: Generate the resource script
echo [1/5] Building resource script generator...
dotnet build "%~dp0ResourceScriptGenerator\ResourceScriptGenerator.csproj" -c Release --nologo -v quiet
if errorlevel 1 (
    echo ERROR: Failed to build resource script generator
    exit /b 1
)

echo [2/5] Generating resource script from Icons directory...
dotnet run --project "%~dp0ResourceScriptGenerator\ResourceScriptGenerator.csproj" -c Release --no-build
if errorlevel 1 (
    echo ERROR: Failed to generate resource script
    exit /b 1
)
echo.

REM Step 2: Check for Visual Studio tools
echo [3/5] Checking for Visual Studio Build Tools...

REM Try to find vswhere (installed with VS 2017+)
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo WARNING: Visual Studio installer not found
    echo Checking for SDK tools in PATH...
    where rc.exe >nul 2>&1
    if errorlevel 1 (
        echo ERROR: RC.EXE not found in PATH
        echo.
        echo Please install one of the following:
        echo   1. Visual Studio 2017 or later with C++ tools
        echo   2. Visual Studio Build Tools
        echo   3. Windows SDK
        echo.
        echo Alternatively, run this from a "Developer Command Prompt"
        exit /b 1
    )
    echo Found RC.EXE in PATH
    goto :build
)

REM Find latest Visual Studio installation
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do (
    set "VSINSTALLDIR=%%i"
)

if not defined VSINSTALLDIR (
    echo ERROR: Visual Studio with C++ tools not found
    echo Please install Visual Studio with C++ desktop development workload
    exit /b 1
)

echo Found Visual Studio at: %VSINSTALLDIR%

REM Setup VS environment
if exist "%VSINSTALLDIR%\VC\Auxiliary\Build\vcvarsall.bat" (
    echo Setting up Visual Studio environment...
    call "%VSINSTALLDIR%\VC\Auxiliary\Build\vcvarsall.bat" x64
) else (
    echo ERROR: vcvarsall.bat not found
    exit /b 1
)

:build

REM Step 3: Compile the resource file
echo.
echo [4/5] Compiling resources with RC.EXE...
rc.exe /nologo /fo "%~dp0WindowsIcons.res" "%~dp0WindowsIcons.rc"
if errorlevel 1 (
    echo ERROR: Resource compilation failed
    exit /b 1
)
echo Resource compilation successful

REM Step 4: Link into DLL
echo.
echo [5/5] Linking resource-only DLL...

REM Create output directory
if not exist "%~dp0bin" mkdir "%~dp0bin"

REM Link as resource-only DLL using /NOENTRY flag
link.exe /DLL /NOENTRY /MACHINE:X64 ^
    /OUT:"%~dp0bin\WindowsIcons.dll" ^
    "%~dp0WindowsIcons.res"

if errorlevel 1 (
    echo ERROR: Linking failed
    exit /b 1
)

echo.
echo ===============================================
echo Build completed successfully!
echo ===============================================
echo.
echo Output: %~dp0bin\WindowsIcons.dll
echo.
echo You can now use this DLL with Windows Explorer's
echo icon picker (Change Icon dialog).
echo.
pause
