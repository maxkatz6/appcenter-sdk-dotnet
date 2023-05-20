#nullable enable
using System.Management;
using Windows.Win32;
using Windows.Win32.Storage.Packaging.Appx;

namespace Microsoft.AppCenter.Utils;

public partial class DeviceInformationHelper
{
    private static string? GetWindowsDeviceModel()
    {
        var managementClass = new ManagementClass("Win32_ComputerSystem");
        foreach (var managementObject in managementClass.GetInstances())
        {
            var model = (string)managementObject["Model"];
            return string.IsNullOrEmpty(model) || DefaultSystemProductName == model ? null : model;
        }

        return string.Empty;
    }
    
    private static string? GetWindowsDeviceOemName()
    {
        var managementClass = new ManagementClass("Win32_ComputerSystem");
        foreach (var managementObject in managementClass.GetInstances())
        {
            var manufacturer = (string)managementObject["Manufacturer"];
            return string.IsNullOrEmpty(manufacturer) || DefaultSystemManufacturer == manufacturer
                ? null
                : manufacturer;
        }
        return string.Empty;
    }

    private static unsafe string? GetWindowsDeploymentVersion()
    {
        uint size, count;
        PInvoke.GetCurrentPackageInfo(0x00000010, &size, null, &count);
        if (count > 0)
        {
            var items = new PACKAGE_INFO[count];
            fixed (PACKAGE_INFO* buffer = items)
            {
                PInvoke.GetCurrentPackageInfo(0x00000010, &size, (byte*)buffer, &count);
            }

            var version = items[0].packageId.version.Anonymous.Anonymous;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        return null;
    }
}