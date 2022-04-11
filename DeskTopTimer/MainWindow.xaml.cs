using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using DeskTopTimer.Native;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using static System.Net.WebRequestMethods;

namespace DeskTopTimer
{

    public class WindowBlur
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(WindowBlur),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (true.Equals(e.OldValue))
                {
                    GetWindowBlur(window)?.Detach();
                    window.ClearValue(WindowBlurProperty);
                }
                if (true.Equals(e.NewValue))
                {
                    var blur = new WindowBlur();
                    blur.Attach(window);
                    window.SetValue(WindowBlurProperty, blur);
                }
            }
        }

        public static readonly DependencyProperty WindowBlurProperty = DependencyProperty.RegisterAttached(
            "WindowBlur", typeof(WindowBlur), typeof(WindowBlur),
            new PropertyMetadata(null, OnWindowBlurChanged));

        public static void SetWindowBlur(DependencyObject element, WindowBlur value)
        {
            element.SetValue(WindowBlurProperty, value);
        }

        public static WindowBlur GetWindowBlur(DependencyObject element)
        {
            return (WindowBlur)element.GetValue(WindowBlurProperty);
        }

        private static void OnWindowBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                (e.OldValue as WindowBlur)?.Detach();
                (e.NewValue as WindowBlur)?.Attach(window);
            }
        }

        private Window _window;

        private void Attach(Window window)
        {
            _window = window;
            var source = (HwndSource)PresentationSource.FromVisual(window);
            if (source == null)
            {
                window.SourceInitialized += OnSourceInitialized;
            }
            else
            {
                AttachCore();
            }
        }

        private void Detach()
        {
            try
            {
                DetachCore();
            }
            finally
            {
                _window = null;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            ((Window)sender).SourceInitialized -= OnSourceInitialized;
            AttachCore();
        }

        private void AttachCore()
        {
            EnableBlur(_window);
        }

        private void DetachCore()
        {
            _window.SourceInitialized += OnSourceInitialized;
        }

        private static void EnableBlur(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
    }

    namespace Native
    {
        internal enum AccentState
        {
            ACCENT_DISABLED,
            ACCENT_ENABLE_GRADIENT,
            ACCENT_ENABLE_TRANSPARENTGRADIENT,
            ACCENT_ENABLE_BLURBEHIND,
            ACCENT_INVALID_STATE,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // 省略其他未使用的字段
            WCA_ACCENT_POLICY = 19,
            // 省略其他未使用的字段
        }
    }


    internal class OpenPageSelf : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
            chromiumWebBrowser.Load(targetUrl);
            //Window currentWindow = new Window();
            //var current = new ChromiumWebBrowser()  {Name = "CurrentBrowser"};
            //current.Load(targetUrl);
            //currentWindow.Content = current;
            //currentWindow.Show();
            return true; //Return true to cancel the popup
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :MetroWindow    
    {
        static MainWindow windowInstance = null;
        static bool IsWindowShow= false;
        MainWorkSpace MainWorkSpace = new MainWorkSpace();
        bool IsPlayVideoSuccess = false;
        bool IsBackgroundVideoChangedRaised = false;


        HotKey hiddenKey = new HotKey(Key.H,KeyModifier.Shift|KeyModifier.Alt,new Action<HotKey>(OnHiddenKey), "窗口隐藏\\显示");
        HotKey flashKey = new HotKey(Key.F,KeyModifier.Shift|KeyModifier.Alt, new Action<HotKey>(OnFreshKey),"刷新");
        HotKey setKey = new HotKey(Key.S,KeyModifier.Shift|KeyModifier.Alt,new Action<HotKey>(OnSetKey),"设置显示\\隐藏");
        HotKey hiddenTimerKey = new HotKey(Key.T,KeyModifier.Shift|KeyModifier.Alt,new Action<HotKey>(OnHiddenTimerKey), "时间隐藏\\显示");
        HotKey showWebFlyOut = new HotKey(Key.U,KeyModifier.Shift|KeyModifier.Alt,new Action<HotKey>(OnShowWebFlyOut),"网页地址显示\\隐藏");
        HotKey showEveryThingFlyOut = new HotKey(Key.E, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnShowEveryThingFlyOut), "搜索本机文件");

        static public void OnHiddenKey(HotKey currentKey)
        {
            if(windowInstance==null)
                return;
            if(IsWindowShow)
            {
                windowInstance.Hide();
                IsWindowShow = false;
            }
            else
            {
                windowInstance.Show();
                windowInstance.Activate();
                IsWindowShow = true;
            }

        }

        static public void OnFreshKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if(windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
            {
                mainWorkSpace?.INeedSeseImmediately.Execute(null);
            }
        }
        
        static public void OnSetKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
            {
                mainWorkSpace?.OpenSettingCommand.Execute(null);
            }
        }

        static public void OnHiddenTimerKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
            {
                mainWorkSpace?.HideTimerCommand.Execute(null);
            }
        }

        static public void OnShowWebFlyOut (HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
            {
                mainWorkSpace?.ShowWebUrlCommand.Execute(null);
            }
        }

        static public void OnShowEveryThingFlyOut(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
            {
                mainWorkSpace.IsOpenSearchFlyOut = true;
            }
        }

        private void SetFontBursh()
        {

        }

        private void SetBackGroundBrush()
        {

        }
        private Dictionary<string,CookieVisitor> UrlCookies = new Dictionary<string, CookieVisitor>();

     

        public MainWindow()
        {
            InitializeComponent();
            //WindowBlur.SetIsEnabled(this, true);
            
            this.DataContext = MainWorkSpace;

            WebView.LifeSpanHandler = new OpenPageSelf();
            BrowserSettings bset = new BrowserSettings();
            
            bset.WindowlessFrameRate = 60;
            bset.WebGl = CefState.Enabled;
            WebView.BrowserSettings = bset;
            WebView.IsBrowserInitializedChanged += (x,y) => 
            {
                if (WebView.IsBrowserInitialized)
                {
                    Cef.UIThreadTaskFactory.StartNew(() =>
                    {
                        //string error = "";
                        //var requestContext = WebView.GetBrowser().GetHost().RequestContext;
                        //requestContext.SetPreference("profile.default_content_setting_values.plugins", 1, out error);
                        //Trace.WriteLine(error);
                    });
            } };
            WebView.FrameLoadStart += WebView_FrameLoadStart;
            WebView.FrameLoadEnd += WebView_FrameLoadEnd;
            MainWorkSpace.CloseWindow += MainWorkSpace_CloseWindow;
            MainWorkSpace.BackgroundVideoChanged += MainWorkSpace_BackgroundVideoChanged;
            MainWorkSpace.VideoVolumnChanged += MainWorkSpace_VideoVolumnChanged;
            MainWorkSpace.WebSiteChanged += MainWorkSpace_WebSiteChanged;
            MainWorkSpace.BusyNow += MainWorkSpace_BusyNow;
            Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            
           
        }

        private async Task<MahApps.Metro.Controls.Dialogs.ProgressDialogController> MainWorkSpace_BusyNow(string busyReason)
        {
            return await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                return await this.ShowProgressAsync("正在处理...", busyReason);
            });
        }

        private void WebView_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async() =>
            {
                try
                {
                    var currentUrl = WebView.GetFocusedFrame().Url;
                    if (UrlCookies.ContainsKey(currentUrl))
                    {
                        var currentVisitor = UrlCookies[currentUrl];
                        var currentCookies = new Cookie()
                        {
                            Domain = new Uri(currentUrl).Host,
                            Name = currentVisitor.name,
                            Value = currentVisitor.value,
                        };
                        var ok = await WebView.GetCookieManager().SetCookieAsync(currentUrl, currentCookies);
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                
            });
        }

        private void WebView_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                try
                {
                    var cookieManager = WebView.GetCookieManager();
                    var currentVisitor = new CookieVisitor();
                    if (cookieManager.VisitAllCookies(currentVisitor))
                    {
                        var currentUrl = WebView.GetFocusedFrame().Url;
                        if (UrlCookies.ContainsKey(currentUrl))
                        {
                            UrlCookies[currentUrl] = currentVisitor;
                        }
                        else
                        {
                            UrlCookies.Add(currentUrl, currentVisitor);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            });
        }

        private void MainWorkSpace_WebSiteChanged(string url)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (WebView.IsBrowserInitialized)
                {
                    //WebView.Address = url;
                    MainWorkSpace.RecordHistoryCommand.Execute(url);
                    WebView.Load(url);
                }
            });



        }

        private void MainWorkSpace_VideoVolumnChanged(double value)
        {
            BackgroundVideo.Volume = value;
        }

        private async void MainWorkSpace_BackgroundVideoChanged(string VideoPath)
        {
            if(MainWorkSpace.IsBackgroundUsingVideo)
            {
                IsBackgroundVideoChangedRaised = true;
                if (IsPlayVideoSuccess)
                {
                    IsPlayVideoSuccess = await BackgroundVideo.Close();
                    if(!IsPlayVideoSuccess)
                        Trace.WriteLine("CloseFailed");
                    else
                        IsPlayVideoSuccess =false;
                }

                if(!IsPlayVideoSuccess)
                {
                    if(!string.IsNullOrEmpty(VideoPath))
                    {
                        IsPlayVideoSuccess = await BackgroundVideo.Open(new Uri(VideoPath));
                        IsPlayVideoSuccess = await BackgroundVideo.Play();
                    }
                    else
                    {
       
                        Trace.WriteLine("已关闭当前文件");
                    }
                    
                }
                else
                {
                    Trace.WriteLine($"开启现有路径失败{VideoPath}");
                }

            }
            else
            {
                IsPlayVideoSuccess = await BackgroundVideo.Close();
                if(IsPlayVideoSuccess)
                    Trace.WriteLine("已关闭当前文件");
                else
                    Trace.WriteLine("关闭当前文件失败");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                MainWorkSpace.Init();
            });
            windowInstance = this;
            IsWindowShow = true;
            BackgroundVideo.MediaStateChanged += BackgroundVideo_MediaStateChanged;
            BackgroundVideo.MessageLogged += BackgroundVideo_MessageLogged;
//#if DEBUG
//            WebView.ShowDevTools();
//#endif
            MainWorkSpace.SetShotKeyDiscribe(new List<HotKey>() {hiddenKey,flashKey,setKey,hiddenTimerKey,showWebFlyOut,showEveryThingFlyOut});
        }

        private void BackgroundVideo_MessageLogged(object? sender, Unosquare.FFME.Common.MediaLogMessageEventArgs e)
        {
            Trace.WriteLine(e.Message);
        }

        private void BackgroundVideo_MediaStateChanged(object? sender, Unosquare.FFME.Common.MediaStateChangedEventArgs e)
        {
            if(IsBackgroundVideoChangedRaised)
            {
                IsBackgroundVideoChangedRaised = false;
                return;
            }

            if(e.OldMediaState==Unosquare.FFME.Common.MediaPlaybackState.Play && e.MediaState== Unosquare.FFME.Common.MediaPlaybackState.Stop)
            {
                if(MainWorkSpace.IsLoopPlay)
                    MainWorkSpace_BackgroundVideoChanged(MainWorkSpace.CurrentBackgroundVideoPath);
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWorkSpace.CloseSese();
        }

        private void MainWorkSpace_CloseWindow()
        {
            Close();
        }


        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        public void OpenOptionsWindow()
        {
            OptionsWindow optW = new OptionsWindow();
            optW.Owner = this;
            optW.Show();
        }

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(MainWorkSpace._IsInitComplete)
                MainWorkSpace.WriteCurrentSettingToJson();
        }


        private void BrowseFileDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            var res= folderBrowserDialog.ShowDialog();
            if(res == System.Windows.Forms.DialogResult.OK)
            {
                MainWorkSpace.VideoPathDir = folderBrowserDialog.SelectedPath;
            }
        }

        private void TopMostMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWorkSpace.IsTopMost = !MainWorkSpace.IsTopMost;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Back();
        }

        private void forwordButton_Click(object sender, RoutedEventArgs e)
        {
            WebView.Forward();
        }

        private void JumpTo_Click(object sender, RoutedEventArgs e)
        {
            MainWorkSpace.ShowWebUrlCommand.Execute(null);
            
        }

        private void CheckLocalStorageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            var res = folderBrowserDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                MainWorkSpace.CollectFileStoragePath = folderBrowserDialog.SelectedPath;
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainWorkSpace?.RunCurrentSelectedResultCommand?.Execute(null);
        }
    }
}
