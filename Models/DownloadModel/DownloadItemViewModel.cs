using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Text;
using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;

namespace Edge {
    public partial class DownloadItemViewModel : ObservableObject {
        public CoreWebView2DownloadOperation Operation { get; }
        private Stopwatch stopwatch = new ();

        [ObservableProperty]
        private DownloadStatus status;
        partial void OnStatusChanged(DownloadStatus value)
        {
            OnPropertyChanged(nameof(GetDownloadVisibility));
            OnPropertyChanged(nameof(GetProgressBarOpacity));
            OnPropertyChanged(nameof(GetPaused));
            OnPropertyChanged(nameof(GetTitleDecorations));
        }

        public readonly string Title;
        [ObservableProperty]
        private double bytesReceived;

        public readonly double TotalBytes;
        [ObservableProperty]
        private string information = "准备中";
        [ObservableProperty]
        private bool isHovered;
        public DownloadItemViewModel(CoreWebView2DownloadOperation operation)
        {
            Operation = operation;
            Title = Path.GetFileName(Operation.ResultFilePath);
            TotalBytes = Operation.TotalBytesToReceive;
            stopwatch.Start();
            Operation.BytesReceivedChanged += Operation_BytesReceivedChanged;
            Operation.StateChanged += OperationOnStateChanged;
        }

        void OperationOnStateChanged(CoreWebView2DownloadOperation sender, object args)
        {
            switch (Operation.State)
            {
                case CoreWebView2DownloadState.InProgress:
                    Status = DownloadStatus.InProgress;
                    break;
                case CoreWebView2DownloadState.Interrupted:
                    switch (Operation.InterruptReason)
                    {
                        case CoreWebView2DownloadInterruptReason.UserPaused:
                            Status = DownloadStatus.Paused;
                            Information = "已暂停";
                            break;
                        case CoreWebView2DownloadInterruptReason.UserCanceled:
                        case CoreWebView2DownloadInterruptReason.UserShutdown:
                            Status = DownloadStatus.Canceled;
                            Information = "已取消";
                            break;
                        default:
                            Status = DownloadStatus.Failed;
                            Information = $"下载失败：{Operation.InterruptReason}";
                            break;
                    }
                    break;
                case CoreWebView2DownloadState.Completed:
                    Status = DownloadStatus.Completed;
                    Information = string.Empty;
                    break;
            }
        }

        private void Operation_BytesReceivedChanged(CoreWebView2DownloadOperation sender, object args)
        {
            string receivedDelta = Converters.ToFileSizeString((long)((sender.BytesReceived - BytesReceived) / stopwatch.Elapsed.TotalSeconds));
            stopwatch.Restart();
            BytesReceived = sender.BytesReceived;
            string received = Converters.ToFileSizeString(sender.BytesReceived);
            string total = Converters.ToFileSizeString(sender.TotalBytesToReceive);
            string speed = receivedDelta + "/s";
            Information = $@"{speed} - {received}/{total}，剩余时间：{DateTime.Parse(sender.EstimatedEndTime) - DateTime.Now:hh\:mm\:ss}";
        }

        [RelayCommand]
        public void Pause()
        {
            if (Status == DownloadStatus.InProgress)
            {
                Operation.Pause();
            }
        }

        [RelayCommand]
        public void Resume()
        {
            if (Status == DownloadStatus.Paused)
            {
                Operation.Resume();
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            if (Status <= DownloadStatus.Paused)
            {
                Operation.Cancel();
            }
        }

        [RelayCommand]
        public void Remove()
        {
            if (Status <= DownloadStatus.Paused)
            {
                Operation.Cancel();
            }
            App.DownloadModel.DownloadList.Remove(this);
        }

        [RelayCommand]
        public void OpenFile()
        {
            if (Status != DownloadStatus.Deleted && File.Exists(Operation.ResultFilePath))
            {
                Process.Start(new ProcessStartInfo { FileName = Operation.ResultFilePath, UseShellExecute = true });
            }
        }

        [RelayCommand]
        public void OpenDirectory()
        {
            string directory = Path.GetDirectoryName(Operation.ResultFilePath);
            if (Directory.Exists(directory))
            {
                if (Status != DownloadStatus.Deleted && File.Exists(Operation.ResultFilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{Operation.ResultFilePath}\"");
                }
                else
                {
                    Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
                }
            }
        }

        [RelayCommand]
        public async void Delete(XamlRoot xamlRoot)
        {
            if (Status <= DownloadStatus.Paused)
            {
                Operation.Cancel();
            }
            if (Status != DownloadStatus.Deleted)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(Operation.ResultFilePath);
                    await file.DeleteAsync();
                }
                catch (Exception e)
                {
                    await new ContentDialog()
                    {
                        Title = $"删除 {Path.GetFileName(Operation.ResultFilePath)} 失败",
                        Content = e.Message,
                        XamlRoot = xamlRoot,
                        CloseButtonText = "好的",
                        DefaultButton = ContentDialogButton.Close
                    }.ShowAsync();
                    return;
                }
                Status = DownloadStatus.Deleted;
                Information = "已删除";
            }
        }

        [RelayCommand]
        public void CopyUri()
        {
            DataPackage dataPackage = new();
            dataPackage.SetText(Operation.Uri);
            Clipboard.SetContent(dataPackage);
        }
        
        public Visibility GetDownloadVisibility(DownloadButtonType type)
        {
            switch (type)
            {
                case DownloadButtonType.Resume:
                    return Status == DownloadStatus.Paused ? Visibility.Visible : Visibility.Collapsed;
                case DownloadButtonType.Pause:
                    return Status == DownloadStatus.InProgress ? Visibility.Visible : Visibility.Collapsed;
                case DownloadButtonType.Cancel:
                    return Status is <= DownloadStatus.Paused ? Visibility.Visible : Visibility.Collapsed;
                case DownloadButtonType.OpenFile:
                    return Status == DownloadStatus.Completed ? Visibility.Visible : Visibility.Collapsed;
                case DownloadButtonType.OpenDirectory:
                case DownloadButtonType.Delete:
                    return Status is DownloadStatus.Canceled or DownloadStatus.Completed or DownloadStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
                case DownloadButtonType.Remove:
                    return Status >= DownloadStatus.Canceled ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public double GetProgressBarOpacity => Status is DownloadStatus.Completed or DownloadStatus.Deleted ? 0 : 1;

        public bool GetPaused => Status == DownloadStatus.Paused;

        public TextDecorations GetTitleDecorations => Status == DownloadStatus.Deleted ? TextDecorations.Strikethrough : TextDecorations.None;

        [RelayCommand]
        public void MouseEnter()
        {
            IsHovered = true;
        }

        [RelayCommand]
        public void MouseLeave()
        {
            IsHovered = false;
        }
    }
}