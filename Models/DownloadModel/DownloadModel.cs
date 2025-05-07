using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.Web.WebView2.Core;

namespace Edge {
    public partial class DownloadModel: ObservableObject
    {
        public ObservableCollection<DownloadItemViewModel> DownloadList = [];
        public AdvancedCollectionView DownloadListFiltered { get; }

        private string filter = string.Empty;

        public string Filter
        {
            get => filter;
            set
            {
                if (!filter.Equals(value))
                {
                    filter = value;
                    DownloadListFiltered.RefreshFilter();
                }
            }
        }

        //TODO:在ToolBar的下载图标上显示ProgressRing和InfoBadge

        public DownloadModel()
        {
            DownloadListFiltered = new AdvancedCollectionView(DownloadList, true)
            {
                Filter = (item) =>
                {
                    if (string.IsNullOrEmpty(Filter))
                    {
                        return true;
                    }
                    if (item is not DownloadItemViewModel downloadObject)
                    {
                        return false;
                    }
                    return downloadObject.Operation.Uri.Contains(Filter) || downloadObject.Operation.ResultFilePath.Contains(Filter);
                }
            };
        }

        public void AddDownload(CoreWebView2DownloadOperation operation)
        {
            DownloadList.Add(new DownloadItemViewModel(operation));
        }
    }
}