#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Microsoft.AppCenter.Utils;

public partial class DeviceInformationHelper
{
    private static string? GetAppleDeviceModel()
    {
        var strPtr = AppleNativeMethods.GetPlatformExpertPropertyValue(AppleNativeMethods.modelStr);
        if (strPtr == IntPtr.Zero)
        {
            return null;
        }
        // ideally we should release strPtr string, but it's used once anyway.
        return Marshal.PtrToStringAuto(AppleNativeMethods.CFDataGetBytePtr(strPtr));
    }
    
    private static string? GetAppleDeviceOemName()
    {
        return "Apple";
    }

    private static class AppleNativeMethods
    {
        internal static readonly IntPtr allocSel = GetHandle("alloc");
        internal static readonly IntPtr classSel = GetHandle("class");
        internal static readonly IntPtr nsStringClass = objc_getClass("NSString");
        internal static readonly IntPtr initWithCharactersSel = GetHandle("initWithCharacters:length:");
        internal static readonly IntPtr selUTF8StringHandle = GetHandle("UTF8String");
        internal static readonly IntPtr modelStr = GetNSString("model");
        internal const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
        internal const string libobjc = "/usr/lib/libobjc.dylib";

        internal const string coreFoundationLibrary =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        internal const string IOPlatformExpertDeviceClassName = "IOPlatformExpertDevice";

        internal static unsafe IntPtr GetNSString(string str)
        {
            fixed (char* ptrFirstChar = str)
            {
                var allocated = intptr_objc_msgSend(nsStringClass, allocSel);
                return IntPtr_objc_msgSend_IntPtr_IntPtr(allocated, initWithCharactersSel, (IntPtr)ptrFirstChar,
                    (IntPtr)str.Length);
            }
        }

        internal static IntPtr GetPlatformExpertPropertyValue(IntPtr property)
        {
            uint platformExpertRef = 0;
            try
            {
                platformExpertRef = IOServiceGetMatchingService(0, IOServiceMatching(IOPlatformExpertDeviceClassName));
                if (platformExpertRef == 0)
                    return default;

                var propertyRef = IORegistryEntryCreateCFProperty(platformExpertRef, property, IntPtr.Zero, 0);
                if (propertyRef == IntPtr.Zero)
                    return default;

                return propertyRef;
            }
            finally
            {
                if (platformExpertRef != 0)
                    IOObjectRelease(platformExpertRef);
            }
        }

        [DllImport(IOKitLibrary)]
        internal static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport(IOKitLibrary)]
        internal static extern IntPtr IOServiceMatching(string s);

        [DllImport(IOKitLibrary)]
        internal static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator,
            uint options);

        [DllImport(IOKitLibrary)]
        internal static extern int IOObjectRelease(uint o);

        [DllImport(coreFoundationLibrary)]
        internal static extern IntPtr CFDataGetBytePtr(IntPtr ptr);

        [DllImport(libobjc, EntryPoint = "objc_msgSend")]
        internal static extern IntPtr intptr_objc_msgSend(IntPtr basePtr, IntPtr selector);

        [DllImport(libobjc, EntryPoint = "objc_msgSend")]
        internal static extern IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr p1,
            IntPtr p2);

        [DllImport(libobjc)]
        internal static extern IntPtr objc_getClass(string className);

        [DllImport(libobjc)]
        internal static extern IntPtr sel_getUid(string selector);

        [DllImport(libobjc, EntryPoint = "sel_registerName")]
        internal extern static IntPtr GetHandle(string name);
    }
}