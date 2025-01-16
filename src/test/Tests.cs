using NUnit.Framework;
using ProcExp152SysDotNetProxy;
using System.Diagnostics;

namespace Test;

[TestFixture]
internal class Tests
{
    [Test]
    public void Open_process()
    {
        var io = new ProcExp152Sys();

        var handle = io.OpenProtectedProcessHandle(4)!;
        Assert.That(handle.IsInvalid, Is.False);
    }

    [Test]
    public void Get_handle_type_and_name()
    {
        var driver = new ProcExp152Sys();

        var info = SystemHandlesRetriever.QuerySystemHandleInformation();
        var handleTypes = new HashSet<string?>();
        var handleNames = new HashSet<string?>();

        foreach (var h in info)
        {
            handleTypes.Add(driver.GetHandleType(h));
            handleNames.Add(driver.GetHandleName(h));
        }

        handleTypes.Remove(null);
        handleNames.Remove(null);

        Assert.That(handleTypes.Count, Is.GreaterThan(0));
        Assert.That(handleTypes, Does.Contain("Process"));
        Assert.That(handleTypes, Does.Contain("Event"));
        Assert.That(handleTypes, Does.Contain("File"));

        Assert.That(handleNames.Count, Is.GreaterThan(0));
        Assert.That(handleNames, Has.Some.Contains("System32"));
        Assert.That(handleNames, Has.Some.Contains(@"\Device"));
    }

    [Test]
    public void Convert_handle_name_to_disk_letter_based_path()
    {
        var converter = new FileHandleNameConverter();

        Assert.That(
            converter.ToDriveLetterBasedFullName(@"\Device\HarddiskVolume3\Windows\System32\en-US\KernelBase.dll.mui"),
            Is.EqualTo(@"C:\Windows\System32\en-US\KernelBase.dll.mui"));

        Assert.That(converter.ToDriveLetterBasedFullName(@"\Device\HarddiskVolume2\Windows"), Is.Null);
    }

    [Test]
    public void Close_handle_works()
    {
        // Prerequisite: Adobe PDF Reader needs to be installed.
        // Adobe PDF locks the pdf file when it opens it. It is needed for this test. Any other program that locks the file could be used as well.

        var pdfFile = Path.Join(Path.GetTempPath(), "testPdfFile.pdf");
        File.Copy(@"C:\Program Files\Adobe\Acrobat DC\Acrobat\WebResources\Resource2\static\js\plugins\sample-files\assets\Sample Files\Complex Machine.pdf", pdfFile, overwrite: true);

        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Adobe Acrobat.lnk",
                Arguments = pdfFile,
                UseShellExecute = true
            }
        }.Start();

        // Wait until Adobe PDF is started
        Thread.Sleep(TimeSpan.FromSeconds(3));

        var driver = new ProcExp152Sys();

        // Find the `C:\testPdfFile.pdf` handle and close it
        var found = false;
        foreach (var (fileName, handle) in GetAllDiskFileHandles(driver))
        {
            if (fileName == pdfFile)
            {
                driver.CloseHandle(handle);
                found = true;
                break;
            }
        }

        Assert.That(found, Is.True, "The PDF file wasn't found in the list of handles");

        // Check that the handle is closed
        foreach (var (fileName, _) in GetAllDiskFileHandles(driver))
        {
            if (fileName == pdfFile)
            {
                Assert.Fail("The handle is not closed");
            }
        }

        // We must be able to delete the pdf file at this point
        File.Delete(pdfFile);
    }

    private static IEnumerable<(string fileName, SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handle)> GetAllDiskFileHandles(ProcExp152Sys io)
    {
        var info = SystemHandlesRetriever.QuerySystemHandleInformation();
        var converter = new FileHandleNameConverter();
        foreach (var h in info)
        {
            var type = io.GetHandleType(h);
            if (type != "File")
            {
                continue;
            }

            var name = io.GetHandleName(h);
            if (name == null)
            {
                continue;
            }

            var realName = converter.ToDriveLetterBasedFullName(name);
            if (realName == null)
            {
                continue;
            }

            yield return (realName, h);
        }
    }
}
