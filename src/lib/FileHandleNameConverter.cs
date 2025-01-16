using System.Runtime.InteropServices;
using System.Text;

namespace ProcExp152SysDotNetProxy;

public sealed class FileHandleNameConverter
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, uint ucchMax);

    private readonly Dictionary<string, char> deviceNameToDriveLetterConversionMap = CreateDeviceNameToDriveLetterConversionMap();

    /// <summary>
    /// Creates a conversion map {device_name, drive_letter}, consisting of all available logical drives on the machine. Example:
    ///   { {"\\Device\\HardDiskVolume2\\", 'C'},
    ///     {"\\Device\\HardDiskVolume15\\", 'D'},
    ///     {"\\Device\\VBoxMiniRdr\\;H:\\VBoxSvr\\My-H\\", 'H'},
    ///     {"\\Device\\LanmanRedirector\\;I:000215d7\\10.22.3.84\\i\\", 'I'},
    ///     {"\\Device\\LanmanRedirector\\;S:000215d7\\10.22.3.84\\devshare\\", 'S'},
    ///     {"\\Device\\LanmanRedirector\\;U:000215d7\\10.22.3.190\\d$\\", 'U'},
    ///     {"\\Device\\LanmanRedirector\\;V:000215d7\\10.22.3.153\\d$\\", 'V'},
    ///     {"\\Device\\CdRom0\\", 'X'} }
    /// </summary>
    private static Dictionary<string, char> CreateDeviceNameToDriveLetterConversionMap()
    {
        var conversionMap = new Dictionary<string, char>();

        for (var driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
        {
            var deviceNameBuffer = new StringBuilder(1024);
            var drive = $"{driveLetter}:";

            var length = QueryDosDevice(drive, deviceNameBuffer, (uint)deviceNameBuffer.Capacity);
            if (length == 0)
            {
                continue;
            }

            // The returned from QueryDosDevice device name doesn't have a '\' at the end.
            // We add it to distinguish between similar device names.
            deviceNameBuffer.Append(@"\");
            conversionMap[deviceNameBuffer.ToString()] = driveLetter;
        }

        return conversionMap;
    }

    /// <summary>
    /// Converts a device-based file path to a drive letter-based full path.
    /// For example:
    ///   From: "\\Device\\HardDiskVolume3\\Windows\\System32\\en-US\\KernelBase.dll.mui"
    ///   To:   "C:\\Windows\\System32\\en-US\\KernelBase.dll.mui"
    /// </summary>
    /// <returns>null if the conversion fails</returns>
    public string? ToDriveLetterBasedFullName(string deviceBasedFileFullName)
    {
        foreach (var kvp in deviceNameToDriveLetterConversionMap)
        {
            var deviceName = kvp.Key;
            var driveLetter = kvp.Value;

            if (deviceBasedFileFullName.StartsWith(deviceName, StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = deviceBasedFileFullName.Substring(deviceName.Length);
                return $@"{driveLetter}:\{relativePath}";
            }
        }

        return null;
    }
}
