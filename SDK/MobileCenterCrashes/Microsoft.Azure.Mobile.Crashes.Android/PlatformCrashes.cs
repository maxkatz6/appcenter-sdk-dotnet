﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.Runtime;

namespace Microsoft.Azure.Mobile.Crashes
{
    using ModelException = Com.Microsoft.Azure.Mobile.Crashes.Ingestion.Models.Exception;
    using ModelStackFrame = Com.Microsoft.Azure.Mobile.Crashes.Ingestion.Models.StackFrame;
    using AndroidManagedErrorLog = Com.Microsoft.Azure.Mobile.Crashes.Ingestion.Models.ManagedErrorLog;
    using AndroidCrashes = Com.Microsoft.Azure.Mobile.Crashes.AndroidCrashes;
    using AndroidICrashListener = Com.Microsoft.Azure.Mobile.Crashes.ICrashesListener;
    using AndroidExceptionDataManager = Com.Microsoft.Azure.Mobile.Crashes.WrapperSdkExceptionManager;

    class PlatformCrashes : PlatformCrashesBase
    {
        // Note: in PlatformCrashes we use only callbacks; not events (in Crashes, there are corresponding events)
        public override SendingErrorReportHandler SendingErrorReport { get; set; }
        public override SentErrorReportHandler SentErrorReport { get; set; }
        public override FailedToSendErrorHandler FailedToSendErrorReport { get; set; }
        public override ShouldProcessErrorReportCallback ShouldProcessErrorReport { get; set; }
        public override GetErrorAttachmentCallback GetErrorAttachment { get; set; }

        public override Type BindingType => typeof(AndroidCrashes);

        public override bool Enabled
        {
            get { return AndroidCrashes.Enabled; }
            set { AndroidCrashes.Enabled = value; }
        }

        public override bool HasCrashedInLastSession => AndroidCrashes.HasCrashedInLastSession;

        public override ErrorReport LastSessionCrashReport
        {
            get
            {
                var androidReport = AndroidCrashes.LastSessionCrashReport;
                if (androidReport == null)
                    return null;
                else
                    return new ErrorReport(androidReport);
            }
        }

        //public override void TrackException(Exception exception)
        //{
        //    AndroidCrashes.Instance.TrackException(GenerateModelException(exception));
        //}

        private AndroidICrashListener _crashListener;
        /// <summary>
        /// Empty model stack frame used for comparison to optimize JSON payload.
        /// </summary>
        private static readonly ModelStackFrame EmptyModelFrame = new ModelStackFrame();

        /// <summary>
        /// Error log generated by the Android SDK on a crash.
        /// </summary>
        private static AndroidManagedErrorLog _errorLog;

        /// <summary>
        /// C# unhandled exception caught by this class.
        /// </summary>
        private static Exception _exception;

        static PlatformCrashes()
        {
            MobileCenterLog.Info(Crashes.LogTag, "Set up Xamarin crash handler.");
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
            AndroidCrashes.Instance.SetWrapperSdkListener(new CrashListener());
        }

        public PlatformCrashes()
        {
            _crashListener = new AndroidCrashListener(this);
            AndroidCrashes.SetListener(_crashListener);
        }

        private static void OnUnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            _exception = e.Exception;
            MobileCenterLog.Error(Crashes.LogTag, "Unhandled Exception:", _exception);
            JoinExceptionAndLog();
        }

        /// <summary>
        /// We don't assume order between java crash handler and c# crash handler.
        /// This method is called after either of those 2 events and is thus effective only the second time when we got both the c# exception and the Android error log.
        /// </summary>
        private static void JoinExceptionAndLog()
        {
            /*
             * We don't assume order between java crash handler and c# crash handler.
             * This method is called after either of those 2 events.
             * It is thus effective only the second time when we got both the c# exception and the Android error log.
             */
            if (_errorLog != null && _exception != null)
            {
                /* Generate structured data for the C# exception and overwrite the Java exception. */
                _errorLog.Exception = GenerateModelException(_exception);

                /* Tell the Android SDK to overwrite the modified error log on disk. */
                AndroidExceptionDataManager.SaveWrapperSdkErrorLog(_errorLog);

                /* Save the System.Exception to disk as a serialized object. */
                byte[] exceptionData = CrashesUtils.SerializeException(_exception);
                AndroidExceptionDataManager.SaveWrapperExceptionData(exceptionData, _errorLog.Id);
            }
        }

#pragma warning disable XS0001 // Find usages of mono todo items

        /// <summary>
        /// Generate structured data for a dotnet exception.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Structured data for the exception.</returns>
        private static ModelException GenerateModelException(Exception exception)
        {
            var modelException = new ModelException
            {
                Type = exception.GetType().FullName,
                Message = exception.Message,
                Frames = GenerateModelStackFrames(new StackTrace(exception, true)),
                WrapperSdkName = WrapperSdk.Name
            };
            var aggregateException = exception as AggregateException;
            if (aggregateException?.InnerExceptions != null)
            {
                modelException.InnerExceptions = new List<ModelException>();
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    modelException.InnerExceptions.Add(GenerateModelException(innerException));
                }
            }
            else if (exception.InnerException != null)
            {
                modelException.InnerExceptions = new List<ModelException> { GenerateModelException(exception.InnerException) };
            }
            return modelException;
        }

        private static IList<ModelStackFrame> GenerateModelStackFrames(StackTrace stackTrace)
        {
            var modelFrames = new List<ModelStackFrame>();
            var frames = stackTrace.GetFrames();
            if (frames != null)
            {
                modelFrames.AddRange(frames.Select(frame => new ModelStackFrame
                {
                    ClassName = frame.GetMethod()?.DeclaringType?.FullName,
                    MethodName = frame.GetMethod()?.Name,
                    FileName = frame.GetFileName(),
                    LineNumber = frame.GetFileLineNumber() != 0 ? new Java.Lang.Integer(frame.GetFileLineNumber()) : null
                }).Where(modelFrame => !modelFrame.Equals(EmptyModelFrame)));
            }
            return modelFrames;
        }

        private class CrashListener : Java.Lang.Object, AndroidCrashes.IWrapperSdkListener
        {
            public void OnCrashCaptured(AndroidManagedErrorLog errorLog)
            {
                _errorLog = errorLog;
                JoinExceptionAndLog();
            }
        }

#pragma warning restore XS0001 // Find usages of mono todo items

    }

}