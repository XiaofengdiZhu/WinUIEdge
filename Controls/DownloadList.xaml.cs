using Microsoft.UI.Xaml.Controls;

namespace Edge
{
    public sealed partial class DownloadList: UserControl
    {
        public DownloadList()
        {
            InitializeComponent();
        }

        void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is DownloadItemViewModel { Status: DownloadStatus.Completed } downloadObject)
            {
                downloadObject.OpenFile();
            }
        }
    }
}