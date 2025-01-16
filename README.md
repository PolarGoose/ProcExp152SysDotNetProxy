# ProcExpSys152DotNetProxy
.Net library for interfacing with the `PROCEXP152.SYS` driver.<br>
The following functionality is supported:
* Open protected process handle
* Close Handle
* Get handle type and name

The library includes the `PROCEXP152.SYS` driver and automatically extracts it if needed.

There are also helper classes for:
* Get all handles in the system
* Convert file handle names to disk-based file names

# Prerequisites
* Windows x86, x64 or arm64

# Examples
## Getting information about all handles in the system
```
// Load the driver service
var driver = new ProcExp152Sys();

// Get all handles in the system
var handles = SystemHandlesRetriever.QuerySystemHandleInformation();

// Create the converter to convert device-based paths to disk letter-based paths:
//  "\\Device\HarddiskVolume3\Windows\System32\en-US\KernelBase.dll.mui" -> "C:\Windows\System32\en-US\KernelBase.dll.mui"
var fileNameConverter = new FileHandleNameConverter();

// Iterate through all handles and print their type and name using the ProcExp152 driver
foreach (var handle in handles)
{
    var handleType = driver.GetHandleType(handle);
    var handleName = driver.GetHandleName(handle);

    // If handle is a file, we can get its full file name using a fileNameConverter
    if (handleType == "File" && handleName != null)
    {
        var handleFilePath = fileNameConverter.ToDriveLetterBasedFullName(handleName);

        // Not all file handles are actual files, thus the conversion might fail
        if (handleFilePath != null)
        {
            Console.WriteLine($"pid={handle.UniqueProcessId}; type={handleType}; name={handleFilePath}");
            continue;
        }
    }

    Console.WriteLine($"pid={handle.UniqueProcessId}; type={handleType}; name={handleName}");
}
```

## Close a particular handle
```
// Load the driver service
var driver = new ProcExp152Sys();

// Get all handles in the system
var handles = SystemHandlesRetriever.QuerySystemHandleInformation();

// Create the converter to convert device-based paths to disk letter-based paths:
//  "\\Device\HarddiskVolume3\Windows\System32\en-US\KernelBase.dll.mui" -> "C:\Windows\System32\en-US\KernelBase.dll.mui"
var fileNameConverter = new FileHandleNameConverter();

// Iterate through all handles and look for a specific file handle
foreach (var handle in handles)
{
    var handleType = driver.GetHandleType(handle);
    var handleName = driver.GetHandleName(handle);

    if (handleType == "File" && handleName != null)
    {
        var handleFilePath = fileNameConverter.ToDriveLetterBasedFullName(handleName);
        if (handleFilePath == @"C:\my\repo\someFile.pdf")
        {
            // Close the handle
            driver.CloseHandle(handle);
            break;
        }
    }
}
```

# Details
`PROCEXP152.SYS` is a Windows kernel driver that is part of the Sysinternals [Process Explorer](https://learn.microsoft.com/en-us/sysinternals/downloads/process-explorer) and [Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle).
It is used to get access to privileged processes and handles. For example, it allows access to handles of a System (pid=4) process, which is not possible by conventional means.<br>
Even though it is possible to use NtDll to get handle names. It has a bug when a method to get the name or type of a handle hangs the calling thread without any way to recover ([issue](https://github.com/giampaolo/psutil/issues/340)). Thus, the `PROCEXP152.sys` driver is the only reliable way to do that.

# Example of a potential use case for this library
Making utilities like [Backstab](https://github.com/Yaxser/Backstab) in C#.
