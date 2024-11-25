using NUnit.Framework;
using ProcExp152SysDotNetProxy;

namespace Test;

[TestFixture]
internal class ExamplesTest
{
    [Test]
    public void Getting_information_about_all_handles()
    {
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
    }

    [Test]
    public void Close_particular_handle()
    {
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
    }
}
