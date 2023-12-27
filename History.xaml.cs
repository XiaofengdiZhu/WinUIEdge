using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Edge
{
    public class HistoryType: ObservableObject
    {
        public string Title { get; set; }

        public string Uri { get; set; }

        public string Time { get; set; }

        public ICommand Command { get; set; }
    }

    public sealed partial class History : Page
    {
        //���� List<T> û��ʵ�� INotifyPropertyChanged �ӿڣ�
        //�����ʹ�� List<T> ��Ϊ ItemSource���� ListView ������ɾ�� Item ʱ��ListView UI �᲻�ܼ�ʱ����
        public static ObservableCollection<HistoryType> HistoryList = [];

        public static StandardUICommand Command = new(StandardUICommandKind.Delete);

        public History()
        {
            this.InitializeComponent();
            listView.ItemsSource = HistoryList;

            Command.ExecuteRequested += CommandExecuteRequested;
        }

        public static void SetHistory(string title, string uri)
        {
            HistoryList.Add(new HistoryType()
            {
                Title = title,
                Uri = uri,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Command = Command
            }); ;
        }

        private void CommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter != null)
            {
                foreach (HistoryType history in HistoryList)
                {
                    if (history.Time == (args.Parameter as string))
                    {
                        HistoryList.Remove(history);
                        return;
                    }
                }
            }
        }

        private void ListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "ShowCancelButton", true);
        }

        private void ListView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HideCancelButton", true);
        }
    }
}
