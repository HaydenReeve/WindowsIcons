// Minimal DLL entry point for resource-only DLL
// This file provides a basic DllMain function to create a valid Win32 DLL
// The actual content of the DLL comes from the embedded icon resources

#include <windows.h>

// DllMain - Required entry point for Win32 DLLs
// This is a minimal implementation that simply returns TRUE
// The DLL itself is resource-only and contains no executable code
BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,  // handle to DLL module
    DWORD fdwReason,     // reason for calling function
    LPVOID lpReserved)   // reserved
{
    // No initialization or cleanup needed for resource-only DLL
    return TRUE;
}
