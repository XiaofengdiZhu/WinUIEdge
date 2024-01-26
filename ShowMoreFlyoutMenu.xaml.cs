using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

            mainWindow.AddNewTab(new SettingsPage(), header: "����");
        }

        private void ShowHistoryFlyout(object sender, RoutedEventArgs e)
        {
            History.ShowFlyout();
        }

        private void ShowDownloadFlyout(object sender, RoutedEventArgs e)
        {
            Download.ShowFlyout();
        }

        private void ShowPrintUI(object sender, RoutedEventArgs e)
        {
            (App.Window as MainWindow).SelectedItem.ShowPrintUI();
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            App.Window.Close();
        }
    }
}
