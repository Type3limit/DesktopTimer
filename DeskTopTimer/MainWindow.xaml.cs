using CefSharp;
using CefSharp.Wpf;
using DeskTopTimer.Native;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private Window? _window;

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
            if (_window != null)
                EnableBlur(_window);
        }

        private void DetachCore()
        {
            if(_window!=null)
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
    public partial class MainWindow : MetroWindow
    {
        static MainWindow windowInstance = null;
        static OptionsWindow translateWindow = null;
        static bool IsWindowShow = false;
        MainWorkSpace MainWorkSpace = null;
        bool IsPlayVideoSuccess = false;
        bool IsBackgroundVideoChangedRaised = false;

        int rectangleCount = 100;
        List<Rectangle> rects = new List<Rectangle>();


        HotKey hiddenKey = new HotKey(Key.H, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnHiddenKey), "窗口隐藏\\显示");
        HotKey flashKey = new HotKey(Key.F, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnFreshKey), "刷新");
        HotKey setKey = new HotKey(Key.S, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnSetKey), "设置显示\\隐藏");
        HotKey hiddenTimerKey = new HotKey(Key.T, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnHiddenTimerKey), "时间隐藏\\显示");
        HotKey showWebFlyOut = new HotKey(Key.U, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnShowWebFlyOut), "网页地址显示\\隐藏");
        //暂时屏蔽Everything api
        //HotKey showEveryThingFlyOut = new HotKey(Key.E, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnShowEveryThingFlyOut), "搜索本机文件");
        HotKey showTranslate = new HotKey(Key.Z,KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnTranslate), "唤起翻译窗口");

        static public void OnHiddenKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (IsWindowShow)
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
            if (windowInstance?.DataContext is MainWorkSpace mainWorkSpace)
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

        static public void OnShowWebFlyOut(HotKey currentKey)
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


        static public void OnTranslate(HotKey currentKey)
        {
            if(translateWindow!=null&&!translateWindow.IsClosed)
            {
                translateWindow.WindowClose();
                return;
            }
            else
            {
                translateWindow = new OptionsWindow();
                translateWindow.DataContext = windowInstance.DataContext;
                translateWindow.Show();
                translateWindow.Activate();
                translateWindow.Focus();
            }

        }

        private void SetFontBursh()
        {

        }

        private void SetBackGroundBrush()
        {

        }
        private Dictionary<string, CookieVisitor> UrlCookies = new Dictionary<string, CookieVisitor>();



        public MainWindow()
        {
            InitializeComponent();
            //WindowBlur.SetIsEnabled(this, true);

            MainWorkSpace = (MainWorkSpace)this.DataContext;

            WebView.LifeSpanHandler = new OpenPageSelf();
            BrowserSettings bset = new BrowserSettings();

            bset.WindowlessFrameRate = 60;
            bset.WebGl = CefState.Enabled;


            WebView.BrowserSettings = bset;
            WebView.IsBrowserInitializedChanged += (x, y) =>
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
                }
            };
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
            Application.Current.Dispatcher.Invoke(async () =>
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
                catch (Exception ex)
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
                catch (Exception ex)
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

        private async void MainWorkSpace_BackgroundVideoChanged(string? VideoPath)
        {
            if (MainWorkSpace.IsBackgroundUsingVideo)
            {
                IsBackgroundVideoChangedRaised = true;
                if (IsPlayVideoSuccess)
                {
                    IsPlayVideoSuccess = await BackgroundVideo.Close();
                    if (!IsPlayVideoSuccess)
                        Trace.WriteLine("CloseFailed");
                    else
                        IsPlayVideoSuccess = false;
                }

                if (!IsPlayVideoSuccess)
                {
                    if (!string.IsNullOrEmpty(VideoPath))
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
                if (IsPlayVideoSuccess)
                    Trace.WriteLine("已关闭当前文件");
                else
                    Trace.WriteLine("关闭当前文件失败");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                MainWorkSpace.AudioVisualizer.WaveDataChanged += AudioVisualizer_WaveDataChanged;
                MainWorkSpace.AudioVisualizer.WaveParamChanged += AudioVisualizer_WaveParamChanged;
                MainWorkSpace.Init();

            });
            Random random = new Random();
            for (int i = 0; i < rectangleCount; i++)
            {
                //var offsetPart = random.Next()%10;
                //GradientStopCollection gradients = new GradientStopCollection();

                //for (int j = 0 ;j<=offsetPart;j++)
                //{
                //    gradients.Add(new GradientStop(Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255)),j/10d));
                //}

                rects.Add(new Rectangle()
                {
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                    Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255)))
                    //Fill = new LinearGradientBrush(gradients) { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1)}
                });
            }



            windowInstance = this;
            IsWindowShow = true;
            BackgroundVideo.MediaStateChanged += BackgroundVideo_MediaStateChanged;
            BackgroundVideo.MessageLogged += BackgroundVideo_MessageLogged;
            //#if DEBUG
            //            WebView.ShowDevTools();
            //#endif
            MainWorkSpace.SetShotKeyDiscribe(new List<HotKey>() { hiddenKey, flashKey, setKey, hiddenTimerKey, showWebFlyOut,showTranslate });
        }

        private void AudioVisualizer_WaveParamChanged(int RectCount, double DrawingBorderWidth, bool UsingRadomColor, Color? spColor, double RectRadius)
        {

            System.Windows.Application.Current?.Dispatcher?.Invoke(new Action(() =>
            {
                MainWorkSpace.AudioVisualizer.WaveDataChanged -= AudioVisualizer_WaveDataChanged;
                for (int i = 0; i < rectangleCount; i++)
                {

                    if (AudioVisualizerDrawArea.Children.Contains(rects[i]))
                        AudioVisualizerDrawArea.Children.Remove(rects[i]);

                }


                rectangleCount = RectCount;
                Random random = new Random();
                for (int i = 0; i < rectangleCount; i++)
                {
                    var Brush = new SolidColorBrush(spColor ?? Colors.White);
                    if (UsingRadomColor)
                        Brush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255)));
                    rects.Add(new Rectangle()
                    {
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = DrawingBorderWidth,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = true,
                        RadiusX = RectRadius,
                        RadiusY = RectRadius,
                        Fill = Brush,
                        //Fill = new LinearGradientBrush(gradients) { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1)}
                    });
                }
                MainWorkSpace.AudioVisualizer.WaveDataChanged += AudioVisualizer_WaveDataChanged;
            }));

        }

        private void AudioVisualizer_WaveDataChanged(float[] samples)
        {
            if (!MainWorkSpace.IsUsingAudiVisualizer)
                return;
            if (samples.Length == 0)
            {
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < rectangleCount; i++)
                    {

                        if (AudioVisualizerDrawArea.Children.Contains(rects[i]))
                            AudioVisualizerDrawArea.Children.Remove(rects[i]);

                    }
                }));
                return;
            }
            var widthPercent = AudioVisualizerDrawArea.ActualWidth / (rectangleCount-1);//ActualWidth / finalData.Count();

            int diffCount = samples.Length / rectangleCount;

            System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                //Stopwatch sp = new Stopwatch();
                //sp.Start();
                Random random = new Random();
                for (int i = 0; i < rectangleCount; i++)
                {
                    var curRect = rects[i];
                    curRect.Width = widthPercent;
                    curRect.Height = (i * diffCount >= samples.Length) ? 1 : ((samples[i * diffCount]) < 0 ? 0 : (samples[i * diffCount]));
                    curRect.RenderTransform = new RotateTransform() { Angle = 180 };
                    curRect.RadiusX = MainWorkSpace.AudioVisualizer.DrawingRectRadius;
                    curRect.RadiusY = MainWorkSpace.AudioVisualizer.DrawingRectRadius;
                    curRect.StrokeThickness = MainWorkSpace.AudioVisualizer.DrawingRectBorderWidth;
                    curRect.Stroke = new SolidColorBrush(MainWorkSpace.AudioVisualizer.SpStrokeColor ?? Colors.White);
                    curRect.Fill = MainWorkSpace.AudioVisualizer.IsUsingRandomColor ?
                    ((curRect.Fill as SolidColorBrush)?.Color == MainWorkSpace.AudioVisualizer.SpColor ?
                    new SolidColorBrush(Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255))) : curRect.Fill)
                    : new SolidColorBrush(MainWorkSpace.AudioVisualizer.SpColor ?? Colors.White);
                    //   new SolidColorBrush(Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255))) : new SolidColorBrush(MainWorkSpace.AudioVisualizer.SpColor ?? Colors.White);
                    Canvas.SetLeft(curRect, (double)(i * widthPercent));
                    Canvas.SetTop(curRect, ActualHeight);
                    if (AudioVisualizerDrawArea.Children.Contains(curRect))
                        AudioVisualizerDrawArea.Children.Remove(curRect);
                    AudioVisualizerDrawArea.Children.Add(curRect);
                }
                //sp.Stop();
                //Debug.WriteLine($"drawing finish with {sp.ElapsedMilliseconds}ms");
            }));


        }


        private void BackgroundVideo_MessageLogged(object? sender, Unosquare.FFME.Common.MediaLogMessageEventArgs e)
        {
            Trace.WriteLine(e.Message);
        }

        private void BackgroundVideo_MediaStateChanged(object? sender, Unosquare.FFME.Common.MediaStateChangedEventArgs e)
        {
            if (IsBackgroundVideoChangedRaised)
            {
                IsBackgroundVideoChangedRaised = false;
                return;
            }

            if (e.OldMediaState == Unosquare.FFME.Common.MediaPlaybackState.Play && e.MediaState == Unosquare.FFME.Common.MediaPlaybackState.Stop)
            {
                if (MainWorkSpace.IsLoopPlay)
                    MainWorkSpace_BackgroundVideoChanged(MainWorkSpace.CurrentBackgroundVideoPath);
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWorkSpace?.CloseSese();
            MainWorkSpace?.AudioVisualizer?.StopRecord();
            if(translateWindow!=null&&!translateWindow.IsClosed)
                translateWindow.WindowClose();
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
            //if(MainWorkSpace._IsInitComplete)
            //    MainWorkSpace.WriteCurrentSettingToJson();
        }


        private void BrowseFileDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            var res = folderBrowserDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
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

        private void StartSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MainWorkSpace?.StartSeach();
        }

        private void AudioVisual_Click(object sender, RoutedEventArgs e)
        {
            MainWorkSpace.IsUsingAudiVisualizer = !MainWorkSpace.IsUsingAudiVisualizer;
        }
    }
}
