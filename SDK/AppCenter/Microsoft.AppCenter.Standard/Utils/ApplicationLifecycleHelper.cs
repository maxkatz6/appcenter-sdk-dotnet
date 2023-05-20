// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AppCenter.Utils;
using System;

namespace Microsoft.AppCenter
{
    public abstract class ApplicationLifecycleHelper : IApplicationLifecycleHelper
    {
        // Considered to be suspended until can verify that the application has started.
        protected static bool _suspended = true;
        
        /// <summary>
        /// Indicates whether the application is currently in a suspended state. 
        /// </summary>
        public bool IsSuspended => _suspended;

        public event EventHandler ApplicationSuspended;
        public event EventHandler ApplicationResuming;
        public event EventHandler<UnhandledExceptionOccurredEventArgs> UnhandledExceptionOccurred;

        public static IApplicationLifecycleHelper Instance { get; internal set; } =
            new ApplicationLifecycleHelperDesktop();

        protected void InvokeResuming()
        {
            if (_suspended)
            {
                _suspended = false;
                ApplicationResuming?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void InvokeSuspended()
        {
            if (!_suspended)
            {
                _suspended = true;
                ApplicationSuspended?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void InvokeUnhandledExceptionOccurred(object sender, UnhandledExceptionOccurredEventArgs args)
        {
            UnhandledExceptionOccurred?.Invoke(sender, args);
        }
    }
}

