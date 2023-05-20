// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Contoso.WPF.Demo.DotNetCore;
using Microsoft.AspNetCore.StaticFiles;

namespace Contoso.Avalonia.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MainWindow _mainWindow;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _mainWindow = new MainWindow();
            }
            
            try
            {
                InitAppCenter();
            }
            catch (Exception ex)
            {
                
            }
        }

        private static void InitAppCenter()
        {
            AppCenter.LogLevel = LogLevel.Verbose;
            AppCenter.SetCountryCode(string.IsNullOrEmpty(Settings.Default.CountryCode) ? null : Settings.Default.CountryCode);
            if (Settings.Default.EnableManualSessionTracker) {
                Analytics.EnableManualSessionTracker();
            }

            // User callbacks.
            Crashes.ShouldAwaitUserConfirmation = ConfirmationHandler;
            Crashes.ShouldProcessErrorReport = (report) =>
            {
                Log($"Determining whether to process error report with an ID: {report.Id}");
                return true;
            };
            Crashes.GetErrorAttachments = GetErrorAttachmentsHandler;

            // Event handlers.
            Crashes.SendingErrorReport += (_, args) => Log($"Sending error report for an error ID: {args.Report.Id}");
            Crashes.SentErrorReport += (_, args) => Log($"Sent error report for an error ID: {args.Report.Id}");
            Crashes.FailedToSendErrorReport += (_, args) => Log($"Failed to send error report for an error ID: {args.Report.Id}");
            var storageMaxSize = Settings.Default.StorageMaxSize;
            if (storageMaxSize > 0)
            {
                AppCenter.SetMaxStorageSizeAsync(storageMaxSize);
            }

            // Start AppCenter.
            var appSecret = Environment.GetEnvironmentVariable("AVALONIA_CORE_PROD");
            AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));

            var userId = Settings.Default.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                AppCenter.SetUserId(userId);
            }
            
            Analytics.TrackEvent("Hello");

            Crashes.HasCrashedInLastSessionAsync().ContinueWith(hasCrashed =>
            {
                Log("Crashes.HasCrashedInLastSession=" + hasCrashed.Result);
            });
            Crashes.GetLastSessionCrashReportAsync().ContinueWith(task =>
            {
                Log("Crashes.LastSessionCrashReport.StackTrace=" + task.Result?.StackTrace);
            });
        }

        private static bool ConfirmationHandler()
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new UserConfirmationDialog();
                if (await dialog.ShowDialog<bool>(_mainWindow))
                {
                    Crashes.NotifyUserConfirmation(dialog.ClickResult);
                }
            });
            return true;
        }

        private static void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            System.Diagnostics.Debug.WriteLine($"{timestamp} [AppCenterDemo] Info: {message}");
        }

        private static IEnumerable<ErrorAttachmentLog> GetErrorAttachmentsHandler(ErrorReport report)
        {
            return GetErrorAttachments();
        }

        public static IEnumerable<ErrorAttachmentLog> GetErrorAttachments()
        {
            List<ErrorAttachmentLog> attachments = new List<ErrorAttachmentLog>();

            // Text attachment
            if (!string.IsNullOrEmpty(Settings.Default.TextErrorAttachments))
            {
                attachments.Add(
                    ErrorAttachmentLog.AttachmentWithText(Settings.Default.TextErrorAttachments, "text.txt"));
            }

            // Binary attachment
            if (!string.IsNullOrEmpty(Settings.Default.FileErrorAttachments))
            {
                if (File.Exists(Settings.Default.FileErrorAttachments))
                {
                    var fileName = new FileInfo(Settings.Default.FileErrorAttachments).Name;
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(fileName, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    var fileContent = File.ReadAllBytes(Settings.Default.FileErrorAttachments);
                    attachments.Add(ErrorAttachmentLog.AttachmentWithBinary(fileContent, fileName, contentType));
                }
                else
                {
                    Settings.Default.FileErrorAttachments = null;
                }
            }

            return attachments;
        }
    }
}
