// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.AppCenter.Utils
{

    /// <summary>
    /// Implements the abstract device information helper class
    /// </summary>
    public class DeviceInformationHelper : AbstractDeviceInformationHelper
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
            return string.Empty;
        }

        protected override string GetAppNamespace()
        {
            return Assembly.GetEntryAssembly()?.EntryPoint.DeclaringType?.Namespace;
        }

        protected override string GetDeviceOemName()
        {
            return string.Empty;
        }

        protected override string GetOsName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WINDOWS"
                    : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "MACOS"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "LINUX"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")) ? "ANDROID"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")) ? "IOS"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) ? "BROWSER"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.Create("MACCATALYST")) ? "MACCATALYST"
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
            return ProductVersion ?? _defaultVersion;
        }

        protected override string GetAppBuild()
        {
            return AssemblyVersion?.FileVersion ?? _defaultVersion;
        }

        protected override string GetScreenSize()
        {
            return string.Empty;
        }

        private static string ProductVersion
        {
            get
            {
                return _defaultVersion;
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
