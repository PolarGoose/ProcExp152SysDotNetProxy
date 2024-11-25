using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProcExp152SysDotNetProxy;

public class ProcExp152SysDotNetProxyException : Exception
{
    public ProcExp152SysDotNetProxyException(string message) : base(message)
    {
    }
}

public class ProcExp152SysDotNetProxyWinApiException : ProcExp152SysDotNetProxyException
{
    public ProcExp152SysDotNetProxyWinApiException(string functionName, string message) :
        base(@$"{functionName} failed.
{message}.
WinApi error code: {Marshal.GetLastWin32Error()}(0x{Marshal.GetLastWin32Error():X}).
WinApi error message: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.")
    {
    }
}
