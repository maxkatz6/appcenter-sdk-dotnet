using System;
using Avalonia;
using Microsoft.AppCenter.Utils;

namespace Microsoft.AppCenter.Standard.Avalonia
{

    public class AvaloniaApplicationLifecycleHelper : IApplicationLifecycleHelper
    {
        public AvaloniaApplicationLifecycleHelper()
        {
            Application.Current.ApplicationLifetime 
        }
        
        public bool IsSuspended { get; }
        public event EventHandler ApplicationSuspended;
        public event EventHandler ApplicationResuming;
        public event EventHandler<UnhandledExceptionOccurredEventArgs> UnhandledExceptionOccurred;
    }
}