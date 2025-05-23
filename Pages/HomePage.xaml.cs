using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Edge
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += InstallWebView2;

            View.ItemsSource = Info.Favorites;

            if (App.settings.ShowBackground)
            {
                if (!string.IsNullOrEmpty(App.settings.BackgroundImage))
                {
                    Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage()
                        {
                            UriSource = new Uri(App.settings.BackgroundImage)
                        },
                        Stretch = Stretch.UniformToFill
                    };
                }
                else
                {
                    LoadBingImage();
                }
            }

            favoriteList.Visibility = App.settings.MenuStatus != "Never" ? Visibility.Visible : Visibility.Collapsed;
        }

        public async void LoadBingImage()
        {
            string url = await Utilities.GetBingImageUrlAsync();
            Background = new ImageBrush()
            {
                ImageSource = new BitmapImage()
                {
                    UriSource = new Uri(url)
                },
                Stretch = Stretch.UniformToFill
            };
        }

        public async void InstallWebView2(object sender, RoutedEventArgs e)
        {
            string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            string bootstrapUri = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
            string bootstrapPath = Path.Combine(AppContext.BaseDirectory, "MicrosoftEdgeWebview2Setup.exe");
            try
            {
                if (string.IsNullOrEmpty(version))
                {
                    using HttpClient client = new();
                    var response = await client.GetAsync(bootstrapUri);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(bootstrapPath, FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = bootstrapPath,
                        Arguments = "/silent /install",
                        UseShellExecute = true
                    });
                }
            }
            catch (HttpRequestException)
            {
                await warningDialog.ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri(bootstrapUri));
                App.GetWindowForElement(this).Close();
            }
        }

        private void OpenFavoriteWebsite(object sender, ItemClickEventArgs e)
        {
            MainWindow mainWindow = App.GetWindowForElement(this);
            mainWindow.AddNewTab(new WebViewPage((e.ClickedItem as WebsiteInfo).Uri));
        }

        private void WebSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (modeBox.SelectedIndex == 1)
            {
                string text = args.QueryText;
                sender.Text = string.Empty;
                MainWindow mainWindow = App.GetWindowForElement(this);
                Utilities.Navigate("https://chat.deepseek.com/", mainWindow);
            }
            else
            {
                string text = args.QueryText;
                sender.Text = string.Empty;
                MainWindow mainWindow = App.GetWindowForElement(this);
                Utilities.Search(text, mainWindow);
            }
        }
    }
}