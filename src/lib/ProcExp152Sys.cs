using Microsoft.Win32.SafeHandles;
using ProcExp152SysDotNetProxy.Impl;

namespace ProcExp152SysDotNetProxy;

public sealed class ProcExp152Sys : IDisposable
{
    private SafeFileHandle openedDriverFile;

    /// <summary>
    /// The constructor loads the driver and opens the driver file.
    /// This class needs to be instantiated only if a process is run from an admin.
    /// </summary>
    public ProcExp152Sys()
    {
        openedDriverFile = DriverLoader.LoadDriverAndOpenTheDriverFile();
    }

    /// <returns>null if process failed to open</returns>
    public SafeFileHandle? OpenProtectedProcessHandle(ulong pid)
    {
        return DriverCommand_OpenProcessHandle.OpenProtectedProcessHandle(openedDriverFile, pid);
    }

    /// <throws>Exception in case handle failed to close</throws>
    public void CloseHandle(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        DriverCommand_CloseHandle.CloseHandle(openedDriverFile, handleInfo);
    }

    /// <returns>
    /// * The type of the handle
    /// * Null in case of a failure.
    /// </returns>
    public string? GetHandleType(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return DriverCommand_GetHandleNameOrType.GetHandleType(openedDriverFile, handleInfo);
    }

    /// <returns>
    /// * The name of the nandle. For the file handles it will be a device-based path like "\\Device\\HardDiskVolume3\\Windows\\System32\\en-US\\KernelBase.dll.mui".
    /// * Null in case failed to get handle name.
    /// </returns>
    public string? GetHandleName(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return DriverCommand_GetHandleNameOrType.GetHandleName(openedDriverFile, handleInfo);
    }

    void IDisposable.Dispose()
    {
        openedDriverFile.Dispose();
    }
}
