using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;


namespace Edge
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SUBCLASSPROC mainWindowSubClassProc;
        private OverlappedPresenter overlappedPresenter;

        private bool _isWindowMaximized;

        public bool IsWindowMaximized
        {
            get => _isWindowMaximized;

            set
            {
                if (_isWindowMaximized != value)
                {
                    _isWindowMaximized = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowMaximized)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            AppWindow.SetIcon("./Assets/icon.ico");
            SetTitleBar(AppTitleBar);

            this.SetBackdrop();
            this.SetThemeColor();

            overlappedPresenter = AppWindow.Presenter as OverlappedPresenter;
            IsWindowMaximized = overlappedPresenter.State == OverlappedPresenterState.Maximized;

            AppWindow.Changed += OnAppWindowChanged;
            mainWindowSubClassProc = new SUBCLASSPROC(MainWindowSubClassProc);
            PInvoke.SetWindowSubclass(new(this.GetWindowHandle()), mainWindowSubClassProc, 0, 0);
        }

        public void AddHomePage()
        {
            string uri = App.settings.SpecificUri;
            if (!string.IsNullOrEmpty(uri))
            {
                AddNewTab(new WebViewPage(new Uri(uri)));
            }
            else
            {
                AddNewTab(new HomePage(), "主页", new FontIconSource() { Glyph = "\ue80f" });
            }
        }

        public void AddNewTab(object content, string header = "正在加载…", IconSource icon = null, int index = -1)
        {
            TabViewItem newTab = new()
            {
                Content = content
            };
            if (content is WebViewPage web)
            {
                StackPanel stackPanel = new () { Orientation = Orientation.Horizontal };
                ProgressRing progressRing = new() { Width = 16, Height = 16, Margin = new Thickness(0,0,16,0) };
                TextBlock textBlock = new() { Text = header };
                stackPanel.Children.Add(progressRing);
                stackPanel.Children.Add(textBlock);
                newTab.Header = stackPanel;
                newTab.ContextFlyout = TabFlyout;
                web.tabViewItem = newTab;
                web.HeaderProgressRing = progressRing;
                web.HeaderTextBlock = textBlock;
            }
            else
            {
                newTab.IconSource = icon ?? new FontIconSource() { Glyph = "\ue774" };
                newTab.Header = header;
            }

            int insertIndex = index >= 0 ? index : tabView.TabItems.Count;
            tabView.TabItems.Insert(insertIndex, newTab);
            tabView.SelectedIndex = insertIndex;
        }

        /// <summary>
        /// 窗口位置变化发生的事件
        /// </summary>
        private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // 窗口位置发生变化
            if (args.DidPositionChange)
            {
                if (TitlebarMenuFlyout.IsOpen)
                {
                    TitlebarMenuFlyout.Hide();
                }

                if (overlappedPresenter is not null)
                {
                    IsWindowMaximized = overlappedPresenter.State is OverlappedPresenterState.Maximized;
                }
            }
        }

        /// <summary>
        /// 窗口还原
        /// </summary>
        private void OnRestoreClicked(object sender, RoutedEventArgs args)
        {
            overlappedPresenter.Restore();
        }

        /// <summary>
        /// 窗口移动
        /// </summary>
        private void OnMoveClicked(object sender, RoutedEventArgs args)
        {
            PInvoke.SendMessage(new(this.GetWindowHandle()), PInvoke.WM_SYSCOMMAND, 0xF010, 0);
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        private void OnSizeClicked(object sender, RoutedEventArgs args)
        {
            PInvoke.SendMessage(new(this.GetWindowHandle()), PInvoke.WM_SYSCOMMAND, 0xF000, 0);
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        private void OnMinimizeClicked(object sender, RoutedEventArgs args)
        {
            overlappedPresenter.Minimize();
        }

        /// <summary>
        /// 窗口最大化
        /// </summary>
        private void OnMaximizeClicked(object sender, RoutedEventArgs args)
        {
            overlappedPresenter.Maximize();
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            this.Close();
        }

        /// <summary>
        /// 应用主窗口消息处理
        /// </summary>
        private LRESULT MainWindowSubClassProc(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
        {
            switch (Msg)
            {
                case PInvoke.WM_NCRBUTTONUP:
                    {
                        if (Content is not null && Content.XamlRoot is not null)
                        {
                            int x = (short)(lParam.Value & 0xFFFF);
                            int y = (short)((lParam.Value >> 16) & 0xFFFF);
                            System.Drawing.Point point = new(x, y);
                            PInvoke.ScreenToClient(hWnd, ref point);

                            uint dpi = PInvoke.GetDpiForWindow(hWnd);
                            float scalingFactor = dpi / 96.0f;

                            point.X = (int)(point.X / scalingFactor);
                            point.Y = (int)(point.Y / scalingFactor);

                            FlyoutShowOptions options = new()
                            {
                                ShowMode = FlyoutShowMode.Standard,
                                Position = new Point(point.X, point.Y)
                            };

                            TitlebarMenuFlyout.ShowAt(Content, options);
                        }
                        return new(0);
                    }
            }
            return PInvoke.DefSubclassProc(hWnd, Msg, wParam, lParam);
        }

        private void CreateNewTabOnRight(object sender, RoutedEventArgs e)
        {
            AddHomePage();
        }

        private void RefreshTab(object sender, RoutedEventArgs e)
        {
            WebView2 webView2 = App.GetWebView2(tabView.SelectedItem as TabViewItem);
            webView2.Reload();
        }

        private void CopyTab(object sender, RoutedEventArgs e)
        {
            WebView2 webView2 = App.GetWebView2(tabView.SelectedItem as TabViewItem);
            AddNewTab(new WebViewPage(webView2.Source), index: tabView.SelectedIndex + 1);
        }

        private void MoveTabToNewWindow(object sender, RoutedEventArgs e)
        {
            WebView2 webView2 = App.GetWebView2(tabView.SelectedItem as TabViewItem);
            var window = App.CreateNewWindow();
            window.AddNewTab(new WebViewPage(webView2.Source));
            window.Activate();
            tabView.TabItems.Remove(tabView.SelectedItem);
        }

        private void CloseTab(object sender, object e)
        {
            TabViewItem item = e is TabViewTabCloseRequestedEventArgs args ? args.Tab : tabView.SelectedItem as TabViewItem;
            if (item.Content is WebViewPage page)
            {
                page.Close();
            }

            tabView.TabItems.Remove(item);

            if (tabView.TabItems.Count <= 0)
            {
                Close();
            }
            else if (App.NeedRestartEnvironment && !App.AnyWebviewPageExists())
            {
                App.WebView2.Close();
            }
        }

        private void CloseOtherTab(object sender, RoutedEventArgs e)
        {
            var selectedItem = tabView.SelectedItem;
            tabView.TabItems.Clear();
            tabView.TabItems.Add(selectedItem);
        }

        private void CloseRightTab(object sender, RoutedEventArgs e)
        {
            while (tabView.TabItems.Count > tabView.SelectedIndex + 1)
            {
                tabView.TabItems.RemoveAt(tabView.SelectedIndex + 1);
            }
            tabView.UpdateLayout();
        }

        private void MuteTab(object sender, RoutedEventArgs e)
        {
            WebView2 webView2 = App.GetWebView2(tabView.SelectedItem as TabViewItem);
            if (!webView2.CoreWebView2.IsMuted)
            {
                webView2.CoreWebView2.IsMuted = true;
                MuteButton.Icon = new FontIcon() { Glyph = "\ue995" };
                MuteButton.Text = "取消标签页静音";
            }
            else
            {
                webView2.CoreWebView2.IsMuted = false;
                MuteButton.Icon = new FontIcon() { Glyph = "\ue74f" };
                MuteButton.Text = "使标签页静音";
            }
        }

        public object SelectedItem
        {
            get => (tabView.SelectedItem as TabViewItem).Content;
            set => tabView.SelectedItem = value;
        }

        public TabView TabView => tabView;

        private void OpenClosedTab(object sender, RoutedEventArgs e)
        {
            AddNewTab(new WebViewPage(new Uri(App.Histories[^1].Source)));
        }

        private void PinTab(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            var item = tabView.SelectedItem as TabViewItem;
            if (item.IsClosable)
            {
                menuItem.Text = "取消固定标签页";
                item.IsClosable = false;
            }
            else
            {
                menuItem.Text = "固定标签页";
                item.IsClosable = true;
            }
        }

        private void TabViewAddTabButtonClick(TabView sender, object args)
        {
            AddHomePage();
        }

        private void TabViewPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint(null).Properties;
            if (properties.IsMiddleButtonPressed)
            {
                var pointerPosition = e.GetCurrentPoint(tabView).Position;
                foreach (var item in tabView.TabItems)
                {
                    if (item is TabViewItem tabViewItem)
                    {
                        var transform = tabViewItem.TransformToVisual(tabView);
                        var itemBounds = transform.TransformBounds(new Rect(0, 0, tabViewItem.ActualWidth, tabViewItem.ActualHeight));
                        if (itemBounds.Contains(pointerPosition))
                        {
                            tabView.TabItems.Remove(tabViewItem);
                            if (tabView.TabItems.Count <= 0) Close();
                            break;
                        }
                    }
                }
            }
        }

        private void OpenBrowserTaskManager(object sender, RoutedEventArgs e)
        {
            TaskManager taskManager = new();
            taskManager.Activate();
        }

        // From WinUI 3 Gallery
        private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            int tabToSelect = 0;

            switch (sender.Key)
            {
                case Windows.System.VirtualKey.Number1:
                    tabToSelect = 0;
                    break;
                case Windows.System.VirtualKey.Number2:
                    tabToSelect = 1;
                    break;
                case Windows.System.VirtualKey.Number3:
                    tabToSelect = 2;
                    break;
                case Windows.System.VirtualKey.Number4:
                    tabToSelect = 3;
                    break;
                case Windows.System.VirtualKey.Number5:
                    tabToSelect = 4;
                    break;
                case Windows.System.VirtualKey.Number6:
                    tabToSelect = 5;
                    break;
                case Windows.System.VirtualKey.Number7:
                    tabToSelect = 6;
                    break;
                case Windows.System.VirtualKey.Number8:
                    tabToSelect = 7;
                    break;
                case Windows.System.VirtualKey.Number9:
                    // Select the last tab
                    tabToSelect = tabView.TabItems.Count - 1;
                    break;
            }

            // Only select the tab if it is in the list
            if (tabToSelect < tabView.TabItems.Count)
            {
                tabView.SelectedIndex = tabToSelect;
            }

            args.Handled = true;
        }
    }
}
