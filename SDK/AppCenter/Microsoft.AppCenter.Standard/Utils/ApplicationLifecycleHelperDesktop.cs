// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.AppCenter.Utils
{
    public class ApplicationLifecycleHelperDesktop : ApplicationLifecycleHelper
    {
        static ApplicationLifecycleHelperDesktop()
        {
            // The change of the state of the flag in this place occurs at the start of the app
            // The `OnMinimized` method does not handle the first entry into the app,
            // so it must happen after initialization
            _suspended = false;
        }

        public ApplicationLifecycleHelperDesktop()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
               InvokeUnhandledExceptionOccurred(sender, new UnhandledExceptionOccurredEventArgs((Exception)eventArgs.ExceptionObject));
            };
        }
    }
}
