using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Edge
{
    public sealed partial class ShowMoreFlyoutMenu : Page
    {
        public ShowMoreFlyoutMenu()
        {
            this.InitializeComponent();
        }

        private void TryCreateNewTab(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.Window as MainWindow;

            mainWindow.AddNewTab(new WebViewPage());
        }

        private void TryCreateNewWindow(object sender, RoutedEventArgs e)
        {
            Utils.CreateNewWindow(new WebViewPage());
        }

        private void TryCreateInPrivateWindow(object sender, RoutedEventArgs e)
        {
            Utils.ShowContentDialog(
                "InPrivate ģʽ����֧��", "Microsoft Edge δ�ṩ InPrivate API��", "ȷ��",
                this.Content.XamlRoot);
        }

        private void TryOpenSettingPage(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.Window as MainWindow;

            mainWindow.AddNewTab(new SettingsPage(), header: "Settings");
        }
    }
}
