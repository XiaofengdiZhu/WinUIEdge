using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Edge
{
    public sealed partial class SafetyItem : Page
    {
        public SafetyItem()
        {
            this.InitializeComponent();
            msSmartScreen.IsOn = App.settings.Smartscreen;
        }

        private void SmartScreenChanged(object sender, RoutedEventArgs e)
        {
            bool isOn = (sender as ToggleSwitch)?.IsOn ?? false;
            if (isOn != App.settings.Smartscreen)
            {
                App.settings.Smartscreen = isOn;
                foreach (MainWindow window in App.mainWindows)
                {
                    foreach (object tabItem in window.TabView.TabItems)
                    {
                        if (tabItem is TabViewItem { Content: WebViewPage webViewPage })
                        {
                            webViewPage.WebView2.CoreWebView2.Settings.IsReputationCheckingRequired = isOn;
                        }
                    }
                }
            }
        }
    }
}
