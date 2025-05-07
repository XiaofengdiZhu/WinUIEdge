using CommunityToolkit.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;


namespace Edge
{
    public class WebViewHistory
    {
        public string DocumentTitle { get; set; }
        public string Source { get; set; }
        public Uri FaviconUri { get; set; }
        public string Time { get; set; }
        public ulong NavigationId { get; set; }
    }

    public sealed partial class ToolBar : UserControl
    {
        public ToolBar()
        {
            this.InitializeComponent();
            ExtensionsItem.InitializeExtensionsCollection();
            extensionsList.ItemsSource = ExtensionsItem.Extensions;
            historyList.ItemsSource = App.Histories;
            FavoriteList.SetItemsPanel(FavoriteList.VerticalTemplate);

            ExtensionsButton.Visibility = App.settings.ToolBar!["ExtensionsButton"] ? Visibility.Visible : Visibility.Collapsed;
            ToolBarSeparator.Visibility = ExtensionsButton.Visibility;
            HistoryButton.Visibility = App.settings.ToolBar!["HistoryButton"] ? Visibility.Visible : Visibility.Collapsed;
            DownloadButton.Visibility = App.settings.ToolBar!["DownloadButton"] ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SearchExtension(object sender, RoutedEventArgs e)
        {
            string text = (sender as TextBox)?.Text ?? String.Empty;
            if (text.Length == 0)
            {
                extensionsList.ItemsSource = ExtensionsItem.Extensions;
            }
            else
            {
                extensionsList.ItemsSource = ExtensionsItem.Extensions.Where(x => x.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private void SearchHistory(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox)?.Text ?? String.Empty;
            if (text.Length == 0)
            {
                historyList.ItemsSource = App.Histories;
            }
            else
            {
                historyList.ItemsSource = App.Histories.Where(x => x.DocumentTitle.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private void OpenHistory(object sender, ItemClickEventArgs e)
        {
            MainWindow mainWindow = App.GetWindowForElement(this);

            mainWindow.AddNewTab(new WebViewPage(new Uri((e.ClickedItem as WebViewHistory).Source)));
        }

        private void SearchDownload(object sender, TextChangedEventArgs e)
        {
            App.DownloadModel.Filter = (sender as TextBox)?.Text ?? string.Empty;
        }

        public void ShowFlyout(string name)
        {
            switch (name)
            {
                case "下载":
                    DownloadButton.ShowFlyout();
                    break;
                case "历史记录":
                    HistoryButton.ShowFlyout();
                    break;
                case "收藏夹":
                    FavoriteButton.ShowFlyout();
                    break;
            }
        }

        private void SplitWindow(object sender, RoutedEventArgs e)
        {
            WebViewPage page = App.GetWindowForElement(this).SelectedItem as WebViewPage;
            page.CreateSplitWindow();
        }

        private void OpenExtensionsPage(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.GetWindowForElement(this);
            TabView tabView = mainWindow.Content as TabView;

            TabViewItem item = tabView?.TabItems.FirstOrDefault(x => ((TabViewItem)x).Content is SettingsPage settingsPage) as TabViewItem;
            if (item != null)
            {
                tabView.SelectedItem = item;
                (item.Content as SettingsPage)?.Navigate("ExtensionsItem");
            }
            else {
                SettingsPage settingsPage = new ();
                settingsPage.Navigate("ExtensionsItem");
                mainWindow.AddNewTab(settingsPage, "设置", new FontIconSource() { Glyph = "\ue713" });
            }
        }

        void ToggleExtension(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch { DataContext: ExtensionInfo extensionInfo })
            {
                ExtensionsItem.ExtensionsToggleEnabledAsync(extensionInfo, this.XamlRoot);
            }
        }

        void OpenExtensionOption(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: ExtensionInfo extensionInfo })
            {
                MainWindow mainWindow = App.GetWindowForElement(this);
                mainWindow.AddNewTab(new  WebViewPage(new Uri(extensionInfo.OptionUri)));
            }
        }
    }
}
