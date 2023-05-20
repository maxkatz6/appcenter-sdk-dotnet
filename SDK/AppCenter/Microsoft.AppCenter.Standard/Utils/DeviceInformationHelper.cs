// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.AppCenter.Utils
{
    /// <summary>
    /// Implements the abstract device information helper class
    /// </summary>
    public partial class DeviceInformationHelper : AbstractDeviceInformationHelper
    {
        private const string _defaultVersion = "Unknown";

        protected override string GetSdkName()
        {
#if NETFRAMEWORK
            return $"appcenter.standard.net";
#elif NETSTANDARD || NETCOREAPP
            return $"appcenter.standard.core";
#endif
        }

        protected override string GetDeviceModel()
        {
            try
            {
                if (OperatingSystemEx.IsWindows())
                {
                    return GetWindowsDeviceModel();
                }
                else if (OperatingSystemEx.IsMacOS() || OperatingSystemEx.IsIOS())
                {
                    return GetAppleDeviceModel();
                }
            }
            catch (Exception exception)
            {
                AppCenterLog.Warn(AppCenterLog.LogTag, "Failed to get device model.", exception);
                return string.Empty;
            }
            return string.Empty;
        }

        protected override string GetDeviceOemName()
        {
            try
            {
                if (OperatingSystemEx.IsWindows())
                {
                    return GetWindowsDeviceOemName();
                }
                else if (OperatingSystemEx.IsMacOS() || OperatingSystemEx.IsIOS())
                {
                    return GetAppleDeviceOemName();
                }
            }
            catch (Exception exception)
            {
                AppCenterLog.Warn(AppCenterLog.LogTag, "Failed to get device OEM name.", exception);
                return string.Empty;
            }
            return string.Empty;
        }

        protected override string GetOsName()
        {
            return OperatingSystemEx.IsWindows() ? "WINDOWS"
                : OperatingSystemEx.IsMacOS() ? "MACOS"
                : OperatingSystemEx.IsLinux() ? "LINUX"
                : OperatingSystemEx.IsAndroid() ? "ANDROID"
                : OperatingSystemEx.IsIOS() ? "IOS"
                : OperatingSystemEx.IsBrowser() ? "BROWSER"
                : OperatingSystemEx.IsOSPlatform("MACCATALYST") ? "MACCATALYST"
                : string.Empty;
        }

        protected override string GetOsBuild()
        {
            return Environment.OSVersion.Version.ToString(4);
        }

        protected override string GetOsVersion()
        {
            return Environment.OSVersion.Version.ToString(4);
        }

        protected override string GetAppVersion()
        {
            return DeploymentVersion ?? ProductVersion ?? _defaultVersion;
        }

        protected override string GetAppBuild()
        {
            return DeploymentVersion ?? AssemblyVersion?.FileVersion ?? _defaultVersion;
        }

        private static string ProductVersion
        {
            get
            {
                var assemblyVersion = AssemblyVersion;
                return assemblyVersion?.ProductVersion ?? assemblyVersion?.FileVersion;
            }
        }

        private static string _deploymentVersion;
        private static string DeploymentVersion
        {
            get
            {
                try
                {
                    if (_deploymentVersion is not null)
                    {
                        return _deploymentVersion;
                    }
                    if (OperatingSystemEx.IsWindows() && Environment.OSVersion.Version.Major >= 10)
                    {
                        return _deploymentVersion = GetWindowsDeploymentVersion();
                    }
                }
                catch (InvalidOperationException exception)
                {
                    AppCenterLog.Warn(AppCenterLog.LogTag, "Failed to get DeploymentVersion.", exception);
                }

                return null;
            }
        }

        private static FileVersionInfo AssemblyVersion
        {
            get
            {
                // The AssemblyFileVersion uniquely identifies a build.
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    var assemblyLocation = entryAssembly.Location;
                    if (string.IsNullOrWhiteSpace(assemblyLocation))
                    {
                        // This is a fix for single file (self-contained publish) api incompatibility in runtime.
                        // Read at https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file#api-incompatibility
                        assemblyLocation = Environment.GetCommandLineArgs()[0];
                    }
                    return FileVersionInfo.GetVersionInfo(assemblyLocation);
                }
                return null;
            }
        }
    }
}
