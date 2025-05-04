using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Microsoft.Web.WebView2.Core;

namespace Edge
{
    public enum UriType
    {
        WithProtocol,
        WithoutProtocol,
        PlainText
    }

    public static class Utilities
    {
        public static void SetBackdrop(this Window window)
        {
            Effect effect = App.settings.BackgroundEffect;
            if (effect == Effect.Mica && MicaController.IsSupported())
            {
                window.SystemBackdrop = new MicaBackdrop();
            }
            else if (effect == Effect.MicaAlt && MicaController.IsSupported())
            {
                window.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
            }
            else if (effect == Effect.Acrylic)
            {
                window.SystemBackdrop = new DesktopAcrylicBackdrop();
            }
            else
            {
                window.SystemBackdrop = null;
            }
        }

        public static void SetThemeColor(this Window window)
        {
            string appearance = App.settings.Appearance;
            if (window.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = Enum.Parse<ElementTheme>(appearance);
                appearance = appearance == "Default" ? "UseDefaultAppMode" : appearance;
                window.AppWindow.TitleBar.PreferredTheme = Enum.Parse<TitleBarTheme>(appearance);
            }
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            return Win32Interop.GetWindowFromWindowId(window.AppWindow.Id);
        }

        public static IntPtr GetWindowHandle(this UIElement element)
        {
            Window window = App.GetWindowForElement(element);
            return window.GetWindowHandle();
        }

        public static unsafe string Win32SaveFile(string fileName, IntPtr hwnd)
        {
            try
            {
                FileInfo info = new(fileName);

                PInvoke.CoCreateInstance<IFileSaveDialog>(
                    typeof(FileSaveDialog).GUID,
                    null,
                    CLSCTX.CLSCTX_INPROC_SERVER,
                    out var fsd);
                List<COMDLG_FILTERSPEC> extensions = [
                    new()
                    {
                        pszSpec = (char* )Marshal.StringToHGlobalUni(info.Extension),
                        pszName = (char* )Marshal.StringToHGlobalUni(info.Extension),
                    }
                ];
                fsd.SetFileTypes(extensions.ToArray());
                string path = UserDataPaths.GetDefault().Downloads;

                PInvoke.SHCreateItemFromParsingName(
                    path,
                    null,
                    typeof(IShellItem).GUID,
                    out var directoryShellItem);

                fsd.SetFolder((IShellItem)directoryShellItem);
                fsd.SetDefaultFolder((IShellItem)directoryShellItem);
                fsd.SetFileName(info.Name);
                fsd.SetDefaultExtension(info.Extension);
                fsd.Show(new(hwnd));
                fsd.GetResult(out var ppsi);

                ppsi.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out PWSTR pwFileName);
                return pwFileName.ToString();
            }
            catch (Exception) { return string.Empty; }
        }

        public static async Task<string> WSPSaveFile(string fileName, IntPtr hwnd)
        {
            FileSavePicker savePicker = new ()
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = Path.GetFileName(fileName)
            };
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            string extension = Path.GetExtension(fileName);
            savePicker.FileTypeChoices.Add(extension, [extension]);
            StorageFile file = await savePicker.PickSaveFileAsync();
            return file != null ? file.Path : string.Empty;
        }

        public static string ToGlyph(this string name)
        {
            return name switch
            {
                "back" => "\ue72b",
                "forward" => "\ue72a",
                "reload" => "\ue72c",
                "saveAs" => "\ue792",
                "print" => "\ue749",
                "share" => "\ue72d",
                "emoji" => "\ue899",
                "undo" => "\ue7a7",
                "redo" => "\ue7a6",
                "cut" => "\ue8c6",
                "copy" => "\ue8c8",
                "paste" => "\ue77f",
                "openLinkInNewWindow" => "\ue737",
                "copyLinkLocation" => "\ue71b",
                "webCapture" => "\uf406",
                "inspectElement" => "\uf8a5",
                _ => string.Empty,
            };
        }

        public static UriType DetectUri(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return UriType.PlainText;
            }
            if (text.Contains("://"))
            {
                return Uri.IsWellFormedUriString(text, UriKind.Absolute) ? UriType.WithProtocol : UriType.PlainText;
            }

            string domainPattern = @"^(?!-)[A-Za-z0-9-]+(\.[A-Za-z]{2,})+$";
            if (Regex.IsMatch(text, domainPattern))
            {
                return UriType.WithoutProtocol;
            }
            return UriType.PlainText;
        }

        public static async Task<string> GetBingImageUrlAsync()
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1");
            var json = JsonDocument.Parse(response);
            return "https://cn.bing.com" + json.RootElement.GetProperty("images")[0].GetProperty("url").GetString();
        }

        public static void Search(string text, MainWindow mainWindow)
        {
            UriType uriType = text.DetectUri();
            if (uriType == UriType.WithProtocol)
            {
                Navigate(text, mainWindow);
            }
            else if (uriType == UriType.WithoutProtocol)
            {
                Navigate("https://" + text, mainWindow);
            }
            else if (File.Exists(text))
            {
                FileInfo fileInfo = new(text);
                string ext = fileInfo.Extension;
                if (Info.LanguageDict.TryGetValue(ext, out var _))
                {
                    mainWindow.AddNewTab(new TextFilePage(fileInfo), fileInfo.Name);
                }
                else if (Info.ImageDict.TryGetValue(ext, out var _))
                {
                    mainWindow.AddNewTab(new ImageViewer(fileInfo), fileInfo.Name);
                }
                else
                {
                    Navigate(text, mainWindow);
                }
            }
            else
            {
                Navigate(Info.SearchEngineList.First(x => x.Name == App.settings.SearchEngine).Uri + text, mainWindow);
            }
        }

        public static void Navigate(string site, MainWindow mainWindow)
        {
            Uri uri = new(site);
            if ((mainWindow.TabView.SelectedItem != null) && (mainWindow.SelectedItem is WebViewPage webviewPage))
            {
                webviewPage.WebView2.Source = uri;
            }
            else
            {
                mainWindow.AddNewTab(new WebViewPage(uri));
            }
        }

        public static async Task ClearBrowsingData()
        {
            if (App.settings.ClearBrowsingDataKindsOnExit == 0)
            {
                return;
            }
            for (int i = 0; i < 16; i++)
            {
                if (((App.settings.ClearBrowsingDataKindsOnExit >> i) & 1) == 1)
                {
                    await App.CoreWebView2Profile.ClearBrowsingDataAsync((CoreWebView2BrowsingDataKinds)(1 << i));
                }
            }
        }
    }
}
