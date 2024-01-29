using Edge.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;


namespace Edge
{
    public sealed partial class ImageViewer : Page
    {
        public string filePath;

        public string fileName;

        public int Angle = 0;

        public ImageViewer(string filepath)
        {
            this.InitializeComponent();
            filePath = filepath;
            GetItemsAsync();
        }

        private async void GetItemsAsync()
        {
            // ��ȡ�ļ���С
            long ImageFileSize = new FileInfo(filePath).Length;

            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            
            // Load Image
            ImageProperties properties = await file.Properties.GetImagePropertiesAsync();

            ItemImage.Source = new BitmapImage(new Uri(filePath));

            imageNameBlock.Text = filePath;
            imageName.Text = fileName = file.DisplayName;
            imageType.Text = file.DisplayType;
            imagePixel.Text = $"{properties.Width} x {properties.Height}";
            imageSize.Text = Other.FormatFileSize(ImageFileSize);

            imageRating.Value = properties.Rating;
        }

        private async void FileNameChanged(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (fileName != imageName.Text)
                {
                    FileInfo fileInfo = new(filePath);
                    string fileExt = fileInfo.Extension;
                    bool changed = await Dialog.ShowMsgDialog("�ļ����Ʊ��ȷ��", $"�Ƿ�Ҫ�����ƴ� {fileName + fileExt} ����Ϊ {imageName.Text + fileExt} ?", "ȡ��", "ȷ��");
                    if (changed)
                    {
                        fileInfo.MoveTo(fileInfo.Directory + "\\" + imageName.Text + fileExt);
                    }
                    else
                    {
                        imageName.Text = fileName;
                    }
                }
            }
        }

        private async void ImageDeleteRequest(object sender, RoutedEventArgs e)
        {
            bool deleted = await Dialog.ShowMsgDialog("�ļ�ɾ��ȷ��", $"�Ƿ�Ҫɾ���ļ� {filePath} ?", "ȡ��", "ȷ��");
            if (deleted)
            {
                FileInfo fileInfo = new(filePath);
                fileInfo.Delete();
            }
        }

        private void ImageRotateRequest(object sender, RoutedEventArgs e)
        {
            Angle = (Angle + 180) % 360;

            RotateTransform rotateTransform = new()
            {
                CenterX = ItemImage.Width / 2,
                CenterY = ItemImage.Height / 2,
                Angle = Angle
            };
            ItemImage.RenderTransform = rotateTransform;
        }

        private void ImageCropRequest(object sender, RoutedEventArgs e)
        {
            (App.Window as MainWindow).AddNewTab(new ImageCropPage(filePath));
        }
    }
}