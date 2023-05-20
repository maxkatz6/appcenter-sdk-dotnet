#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Microsoft.AppCenter.Utils;

public partial class DeviceInformationHelper
{
    private static readonly IntPtr allocSel = GetHandle("alloc");
    private static readonly IntPtr initWithCharactersSel = GetHandle("initWithCharacters:length:");
    private static readonly IntPtr selUTF8StringHandle = GetHandle("UTF8String");
    private static readonly IntPtr modelStr = GetNSString("model");
    private const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string libobjc = "/usr/lib/libobjc.dylib";
    private const string IOPlatformExpertDeviceClassName = "IOPlatformExpertDevice";

    private static string? GetAppleDeviceModel()
    {
        var strPtr = GetPlatformExpertPropertyValue(modelStr);
        if (strPtr == IntPtr.Zero)
        {
            return null;
        }
        // ideally we should release strPtr string, but it's used once anyway.
        return Marshal.PtrToStringAuto(intptr_objc_msgSend(strPtr, selUTF8StringHandle));
    }
    
    private static string? GetAppleDeviceOemName()
    {
        return "Apple";
    }

    private static unsafe IntPtr GetNSString(string str)
    {
        fixed (char* ptrFirstChar = str)
        {
            var allocated = intptr_objc_msgSend(default, allocSel);
            return IntPtr_objc_msgSend_IntPtr_IntPtr(allocated, initWithCharactersSel, (IntPtr)ptrFirstChar, (IntPtr)str.Length);
        }
    }

    private static IntPtr GetPlatformExpertPropertyValue(IntPtr property)
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
    private static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);
    [DllImport(IOKitLibrary)]
    private static extern IntPtr IOServiceMatching(string s);
    [DllImport(IOKitLibrary)]
    private static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);
    [DllImport(IOKitLibrary)]
    private static extern int IOObjectRelease(uint o);
    [DllImport(libobjc, EntryPoint = "objc_msgSend")]
    private static extern IntPtr intptr_objc_msgSend(IntPtr basePtr, IntPtr selector);
    [DllImport(libobjc, EntryPoint = "objc_msgSend")]
    public extern static IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr p1, IntPtr p2);
    [DllImport(libobjc)]
    private static extern IntPtr objc_getClass(string className);
    [DllImport(libobjc)]
    private static extern IntPtr sel_getUid(string selector);
    [DllImport(libobjc, EntryPoint = "sel_registerName")]
    private extern static IntPtr GetHandle(string name);
}