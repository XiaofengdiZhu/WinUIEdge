using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Edge
{
    public class BrowserDataKind
    {
        public CoreWebView2BrowsingDataKinds Kind { get; set; }
        public string Name { get; set; }
        public bool IsChecked = false;
    }
    public sealed partial class PrivacyItem : Page
    {
        public List<BrowserDataKind> BrowserDataKindList =
        [
            new() { Kind = CoreWebView2BrowsingDataKinds.BrowsingHistory, Name = "�����ʷ��¼"},
            new() { Kind = CoreWebView2BrowsingDataKinds.CacheStorage, Name = "������Դ" },
            new() { Kind = CoreWebView2BrowsingDataKinds.Cookies, Name = "Cookies����վ����" },
            new() { Kind = CoreWebView2BrowsingDataKinds.DownloadHistory, Name = "������ʷ��¼" },
            new() { Kind = CoreWebView2BrowsingDataKinds.DiskCache, Name = "���̻���" },
            new() { Kind = CoreWebView2BrowsingDataKinds.IndexedDb, Name = "IndexedDB ���ݴ洢" },
            new() { Kind = CoreWebView2BrowsingDataKinds.LocalStorage, Name = "���ش洢����" },
            new() { Kind = CoreWebView2BrowsingDataKinds.PasswordAutosave, Name = "�����Զ����" },
            new() { Kind = CoreWebView2BrowsingDataKinds.WebSql, Name = "WebSQL ���ݿ�" },
        ];

        public List<string> trackLevelList = [.. Enum.GetNames(typeof(CoreWebView2TrackingPreventionLevel))];

        public PrivacyItem()
        {
            this.InitializeComponent();
            trackBox.ItemsSource = trackLevelList;
            trackBox.SelectedIndex = trackLevelList.IndexOf(App.webView2.CoreWebView2.Profile.PreferredTrackingPreventionLevel.ToString());
            ClearBrowsingDataButton.ItemsSource = BrowserDataKindList;
        }

        private async void ClearBrowsingData(object sender, RoutedEventArgs e)
        {
            foreach (var item in ClearBrowsingDataButton.ItemsSource as List<BrowserDataKind>)
            {
                if (item.IsChecked)
                {
                    await App.webView2.CoreWebView2.Profile.ClearBrowsingDataAsync(item.Kind);
                }
            }
            ClearBrowsingDataButton.Description = "������ѡ�����Ŀ";
        }

        private void TrackLevelChanged(object sender, SelectionChangedEventArgs e)
        {
            string level = (sender as ComboBox).SelectedItem as string;
            App.webView2.CoreWebView2.Profile.PreferredTrackingPreventionLevel = Enum.Parse<CoreWebView2TrackingPreventionLevel>(level);
        }
    }
}
