using Microsoft.AppCenter.Crashes;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Contoso.Avalonia.Puppet
{
    public partial class UserConfirmationDialog : Window
    {
        public UserConfirmationDialog()
        {
            InitializeComponent();
        }

        private void DontSendButton_Click(object sender, RoutedEventArgs e)
        {
            ClickResult = UserConfirmation.DontSend;
            Close(true);
        }

        private void AlwaysSendButton_Click(object sender, RoutedEventArgs e)
        {
            ClickResult = UserConfirmation.AlwaysSend;
            Close(true);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ClickResult = UserConfirmation.Send;
            Close(true);
        }

        public UserConfirmation ClickResult { get; private set; }
    }
}
