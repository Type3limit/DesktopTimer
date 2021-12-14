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
using DeskTopTimer.Native;
using MahApps.Metro.Controls;

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
        HotKey hiddenKey = new HotKey(Key.H,KeyModifier.Shift|KeyModifier.Alt,new Action<HotKey>(OnHiddenKey));
        HotKey flashKey = new HotKey(Key.F,KeyModifier.Shift|KeyModifier.Alt, new Action<HotKey>(OnFreshKey));

        #region dependency
        public Brush BackgroundBursh
        {
            get { return (Brush)GetValue(BackgroundBurshProperty); }
            set { SetValue(BackgroundBurshProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBursh.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBurshProperty =
            DependencyProperty.Register("BackgroundBursh", typeof(Brush), typeof(MainWindow), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128,0,0,0))));



        public Brush FontBrush
        {
            get { return (Brush)GetValue(FontBrushProperty); }
            set { SetValue(FontBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontBrushProperty =
            DependencyProperty.Register("FontBrush", typeof(Brush), typeof(MainWindow), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public double BackGroundOpacity
        {
            get { return (double)GetValue(BackGroundOpacityProperty); }
            set { SetValue(BackGroundOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackGroundOpacityProperty =
            DependencyProperty.Register("BackGroundOpacityProperty", typeof(double), typeof(MainWindow), new PropertyMetadata(1d));



        public double  FontOpacity
        {
            get { return (double )GetValue(FontOpacityProperty); }
            set 
                { 
                SetValue(FontOpacityProperty, value);
                }
        }

        // Using a DependencyProperty as the backing store for FontOpacityProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontOpacityProperty =
            DependencyProperty.Register("FontOpacityProperty", typeof(double ), typeof(MainWindow), new PropertyMetadata(1d));

        #endregion

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

        private void SetFontBursh()
        {

        }

        private void SetBackGroundBrush()
        {

        }

        public MainWindow()
        {
            InitializeComponent();
            //WindowBlur.SetIsEnabled(this, true);
            
            this.DataContext = MainWorkSpace;
            MainWorkSpace.CloseWindow += MainWorkSpace_CloseWindow;
            MainWorkSpace.BackgroundVideoChanged += MainWorkSpace_BackgroundVideoChanged;
            MainWorkSpace.VideoVolumnChanged += MainWorkSpace_VideoVolumnChanged;
            Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            Task.Run(() =>
            {
                MainWorkSpace.Init();
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
                        Debug.WriteLine("CloseFailed");
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
       
                        Debug.WriteLine("已关闭当前文件");
                    }
                    
                }
                else
                {
                    Debug.WriteLine($"开启现有路径失败{VideoPath}");
                }

            }
            else
            {
                IsPlayVideoSuccess = await BackgroundVideo.Close();
                if(IsPlayVideoSuccess)
                    Debug.WriteLine("已关闭当前文件");
                else
                    Debug.WriteLine("关闭当前文件失败");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            windowInstance = this;
            IsWindowShow = true;
            BackgroundVideo.MediaStateChanged += BackgroundVideo_MediaStateChanged;
            BackgroundVideo.MessageLogged += BackgroundVideo_MessageLogged;

        }

        private void BackgroundVideo_MessageLogged(object? sender, Unosquare.FFME.Common.MediaLogMessageEventArgs e)
        {
            Debug.WriteLine(e.Message);
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
    }
}
