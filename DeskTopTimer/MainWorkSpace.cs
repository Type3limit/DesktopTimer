using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using DeskTopTimer.SubModels;
using System.Windows.Media;
using System.Windows.Markup;

namespace DeskTopTimer
{

    internal class MainWorkSpace : ObservableObject
    {
        #region PerformRegion

        #region TaskBarControl
        private ImageSource? taskbarIcon = null;
        public ImageSource? TaskbarIcon
        {
            get => taskbarIcon;
            set
            {
                SetProperty(ref taskbarIcon, value);
            }
        }
        #endregion

        #region TextControl

        private string _currentTimeStr = string.Empty;
        /// <summary>
        /// 时间字符
        /// </summary>
        public string CurrentTimeStr
        {
            get => _currentTimeStr;
            set => SetProperty(ref _currentTimeStr, value);

        }

        private string _currentWeekTimeStr = string.Empty;
        /// <summary>
        /// 星期字符
        /// </summary>
        public string CurrentWeekTimeStr
        {
            get => _currentWeekTimeStr;
            set => SetProperty(ref _currentWeekTimeStr, value);
        }

        private List<HotKey> shotKeys = new List<HotKey>();
        /// <summary>
        /// 快捷键描述
        /// </summary>
        public List<HotKey> ShotKeys
        {
            get => shotKeys;
            set => SetProperty(ref shotKeys, value);
        }

        #endregion

        #region BackGroundControl

        private bool isTimerBorderVisiable = true;
        /// <summary>
        /// 标识是否显示中心的时钟
        /// </summary>
        public bool IsTimerBorderVisiable
        {
            get => isTimerBorderVisiable;
            set => SetProperty(ref isTimerBorderVisiable, value);
        }

        private bool isBackgroundUsingVideo = false;
        /// <summary>
        /// 标识背景是否使用视频
        /// </summary>
        public bool IsBackgroundUsingVideo
        {
            get => isBackgroundUsingVideo;
            set
            {
                SetProperty(ref isBackgroundUsingVideo, value);
                if (value)
                {


                    if (String.IsNullOrEmpty(CurrentBackgroundVideoPath))
                        VideoPathDir = VideoPathDir;
                    else
                        CurrentBackgroundVideoPath = CurrentBackgroundVideoPath;
                    CloseSese();
                }
                else
                {

                    BackgroundVideoChanged?.Invoke("");
                    _shouldStopSese = false;
                    StartSeSeCacheThread();
                    StartSeSePreviewThread();
                }

            }

        }

        private double backgroundImageOpacity = 1d;
        /// <summary>
        /// 背景透明度
        /// </summary>
        public double BackgroundImageOpacity
        {
            get => backgroundImageOpacity;
            set => SetProperty(ref backgroundImageOpacity, value);
        }

        #region WebControl
        private bool isWebViewVisible = false;
        /// <summary>
        /// 标识网页是否可见
        /// </summary>
        public bool IsWebViewVisible
        {
            get => isWebViewVisible;
            set
            {
                if (isWebViewVisible == value)
                    return;
                if (value)
                {
                    if (IsBackgroundUsingVideo)
                    {
                        BackgroundVideoChanged?.Invoke("");
                    }
                    else
                    {
                        _ = CloseSese();
                        CurrentWebAddress = CurrentWebAddress;
                    }
                }
                else
                {
                    if (IsBackgroundUsingVideo)
                    {
                        BackgroundVideoChanged?.Invoke(CurrentBackgroundVideoPath);
                    }
                    else
                    {
                        _=CloseSese();
                        _shouldStopSese = false;
                        StartSeSeCacheThread();
                        StartSeSePreviewThread();
                    }
                }
                SetProperty(ref isWebViewVisible, value);
            }
        }

        private List<string> webAddresses = new List<string>();
        /// <summary>
        /// 默认网站地址记录
        /// </summary>
        public List<string> WebAddresses
        {
            get => webAddresses;
            set => SetProperty(ref webAddresses, value);
        }

        private string currentWebAddress = string.Empty;
        /// <summary>
        /// 当前开启的网页
        /// </summary>
        public string CurrentWebAddress
        {
            get => currentWebAddress;
            set
            {
                SetProperty(ref currentWebAddress, value);
                WebSiteChanged?.Invoke(value);
            }

        }

        private List<string> histories = new List<string>();
        /// <summary>
        /// 历史记录
        /// </summary>
        public List<string> Histories
        {
            get => histories;
            set => SetProperty(ref histories, value);
        }
        #endregion

        #region ImageControl

        private BitmapImage? currentSePic = null;
        /// <summary>
        /// 当前图片
        /// </summary>
        public BitmapImage? CurrentSePic
        {
            get => currentSePic;
            set => SetProperty(ref currentSePic, value);
        }



        #region ImagePath Control


        private List<string> seSeApis = new List<string>();
        /// <summary>
        /// 所有的在线图库地址
        /// </summary>
        public List<string> SeSeApis
        {
            get => seSeApis;
            set => SetProperty(ref seSeApis, value);
        }

        private string selectedSeSe = string.Empty;
        /// <summary>
        /// 选中的在线图库地址
        /// </summary>
        public string SeletctedSeSe
        {
            get => selectedSeSe;
            set
            {
                if (selectedSeSe != value)
                {
                    SetProperty(ref selectedSeSe, value);
                    ApiChanged();
                }

            }

        }

        private bool isOnlineSeSeMode = true;
        /// <summary>
        /// 标识是否启用在线图库
        /// </summary>
        public bool IsOnlineSeSeMode
        {
            get => isOnlineSeSeMode;
            set
            {
                SetProperty(ref isOnlineSeSeMode, value);
                SwitchSeSeMode();
            }
        }

        private string currentSeSeApi = WebRequestsTool.seseUrlLevel1;
        /// <summary>
        /// 标识当前启用的在线地址
        /// </summary>
        public string CurrentSeSeApi
        {
            get => currentSeSeApi;
            set => SetProperty(ref currentSeSeApi, value);
        }


        private string collectFileStoragePath = FileMapper.LocalCollectionPictureDir;
        /// <summary>
        /// 本地收藏存储地址
        /// </summary>
        public string CollectFileStoragePath
        {
            get=>collectFileStoragePath;
            set=>SetProperty(ref collectFileStoragePath, value);
        }

        private string localFileDir = FileMapper.LocalSeSePictureDir;
        /// <summary>
        /// 本地图库地址
        /// </summary>
        public string LocalFileDir
        {
            get => localFileDir;
            set => SetProperty(ref localFileDir, value);
        }



        private long maxCacheCount = 20;//默认缓存20张
        /// <summary>
        /// 最大缓存数
        /// </summary>
        public long MaxCacheCount
        {
            get => maxCacheCount;
            set 
            {
                if(maxCacheCount!=value)
                {
                    SetProperty(ref maxCacheCount, value);


                }

            }
        }

        private long maxSeSeCount = 10;//默认10秒
        /// <summary>
        /// 刷新时间最大值
        /// </summary>
        public long MaxSeSeCount
        {
            get => maxSeSeCount;
            set => SetProperty(ref maxSeSeCount, value);
        }

        private bool shouldPausePreview = false;
        /// <summary>
        /// 标识是否应该暂停刷新
        /// </summary>
        public bool ShouldPausePreview
        {
            get => shouldPausePreview;
            set => SetProperty(ref shouldPausePreview, value);
        }

        private string currentPreviewFile = string.Empty;
        /// <summary>
        /// 当前的图像本地路径
        /// </summary>
        public string CurrentPreviewFile
        {
            get => currentPreviewFile;
            set => SetProperty(ref currentPreviewFile, value);
        }

        #endregion
        #endregion

        #region VideoControl
        private bool isLoopPlay = true;
        /// <summary>
        /// 标识是否循环播放
        /// </summary>
        public bool IsLoopPlay
        {
            get => isLoopPlay;
            set
            {
                SetProperty(ref isLoopPlay, value);
            }
        }



        private string videoPathDir = FileMapper.VideoCacheDir;
        /// <summary>
        /// 标识视频路径
        /// </summary>
        public string VideoPathDir
        {
            get => videoPathDir;
            set
            {
                SetProperty(ref videoPathDir, value);
                if (Directory.Exists(value))
                {
                    BackgroundVideos = ReadDestVideo(value);
                    CurrentBackgroundVideoPath = BackgroundVideos.FirstOrDefault();
                }

            }
        }

        private bool isVideoMute = false;
        /// <summary>
        /// 标识是否应该静音
        /// </summary>
        public bool IsVideoMute
        {
            get => isVideoMute;
            set
            {
                if (value)
                {
                    originVolume = videoVolume;
                    VideoVolume = 0;
                }
                else
                {
                    VideoVolume = originVolume;
                }
                SetProperty(ref isVideoMute, value);
            }
        }

        private double videoVolume = 1d;
        /// <summary>
        /// 视频音量
        /// </summary>
        public double VideoVolume
        {
            get => videoVolume;
            set
            {
                if (videoVolume != value)
                {
                    SetProperty(ref videoVolume, value);
                    VideoVolumnChanged?.Invoke(value);
                }

            }
        }


        private List<string> backgroundVideos = new List<string>();
        /// <summary>
        /// 本地视频路径下的所有视频
        /// </summary>
        public List<string> BackgroundVideos
        {
            get => backgroundVideos;
            set => SetProperty(ref backgroundVideos, value);
        }

        private string? currentBackgroundVideoPath = string.Empty;
        /// <summary>
        /// 选中的视频路径
        /// </summary>
        public string? CurrentBackgroundVideoPath
        {
            get => currentBackgroundVideoPath;
            set
            {
                SetProperty(ref currentBackgroundVideoPath, value);
                if (!string.IsNullOrEmpty(value) && File.Exists(value) && IsBackgroundUsingVideo)
                {
                    BackgroundVideoChanged?.Invoke(value);
                }
            }
        }

        #endregion

        #endregion

        #region WindowControl

        private double windowWidth = 300d;
        /// <summary>
        /// 窗口宽度（不代表实际图像宽）
        /// </summary>
        public double WindowWidth
        {
            get => windowWidth;
            set => SetProperty(ref windowWidth, value);
        }

        private double windowHeight = 300d;
        /// <summary>
        /// 窗口高度(不代表实际图像高 )
        /// </summary>
        public double WindowHeight
        {
            get => windowHeight;
            set => SetProperty(ref windowHeight, value);
        }


        private bool isOpenSettingFlyout = false;
        /// <summary>
        /// 标识是否应该打开设置窗体
        /// </summary>
        public bool IsOpenSettingFlyout
        {
            get => isOpenSettingFlyout;
            set
            {
                if (isOpenSettingFlyout != value)
                {
                    SetProperty(ref isOpenSettingFlyout, value);
                    WriteCurrentSettingToJson();
                }
            }
        }

        private bool isOpenWebUrlFlyout = false;
        /// <summary>
        /// 标识是否应该打开网址窗体
        /// </summary>
        public bool IsOpenWebUrlFlyOut
        {
            get => isOpenWebUrlFlyout;
            set
            {
                if (!IsWebViewVisible && value)
                    return;
                SetProperty(ref isOpenWebUrlFlyout, value);
            }
        }


        private bool isTopMost = true;
        /// <summary>
        /// 标识当前窗口是否置顶
        /// </summary>
        public bool IsTopMost
        {
            get => isTopMost;
            set => SetProperty(ref isTopMost, value);

        }



        #endregion

        #region FontRelated

        private List<FontFamily> fontFamilies = new List<FontFamily>();
        /// <summary>
        /// 当前系统中能被枚举的字体集合
        /// </summary>
        public List<FontFamily> FontFamilies
        {
            get => fontFamilies;
            set => SetProperty(ref fontFamilies, value);
        }

        private FontFamily? selectedFontFamily = null;
        /// <summary>
        /// 时间字体
        /// </summary>
        public FontFamily? SelectedFontFamily
        {
            get => selectedFontFamily;
            set => SetProperty(ref selectedFontFamily, value);
        }

        private FontFamily? selectedWeekendFontFamily = null;
        /// <summary>
        /// 星期字体
        /// </summary>
        public FontFamily? SelectedWeekendFontFamily
        {
            get => selectedWeekendFontFamily;
            set => SetProperty(ref selectedWeekendFontFamily, value);
        }

        private Color timeFontColor = Colors.White;
        /// <summary>
        /// 时间文字颜色
        /// </summary>
        public Color TimeFontColor
        {
            get => timeFontColor;
            set
            {
                SetProperty(ref timeFontColor, value);
                OnPropertyChanged("TimeFontBrush");
            }
        }

        /// <summary>
        /// 时间文字对应的画刷
        /// </summary>
        public Brush TimeFontBrush
        {
            get => new SolidColorBrush(TimeFontColor);
        }

        private Color weekendFontColor = Colors.White;
        /// <summary>
        /// 星期文字颜色
        /// </summary>
        public Color WeekendFontColor
        {
            get => weekendFontColor;
            set
            {
                SetProperty(ref weekendFontColor, value);
                OnPropertyChanged("WeekendFontBrush");
            }
        }

        /// <summary>
        /// 星期文字对应的画刷
        /// </summary>
        public Brush WeekendFontBrush
        {
            get => new SolidColorBrush(WeekendFontColor);
        }

        private int timeCenterFontSize = 20;
        /// <summary>
        /// 时间字体大小
        /// </summary>
        public int TimeCenterFontSize
        {
            get => timeCenterFontSize;
            set => SetProperty(ref timeCenterFontSize, value);
        }

        private int weekendCenterFontSize = 12;
        /// <summary>
        /// 星期字体大小
        /// </summary>
        public int WeekendCenterFontSize
        {
            get => weekendCenterFontSize;
            set => SetProperty(ref weekendCenterFontSize, value);
        }


        #endregion

        #endregion

        #region Private 
        /// <summary>
        /// 当前的刷新计数
        /// </summary>
        long seseCount = 0;

        /// <summary>
        /// 用于计数的timer
        /// </summary>
        System.Timers.Timer timer = new System.Timers.Timer();

        /// <summary>
        /// 用于在线网络请求(使用flurl)
        /// </summary>
        WebRequestsTool WebRequestsTool = new WebRequestsTool();

        /// <summary>
        /// 原始音量
        /// </summary>
        double originVolume = 0d;

        #region thread Control
        /// <summary>
        /// 标识当前是否应该停止
        /// </summary>
        bool _shouldStopSese = false;
        /// <summary>
        /// 标识当前是否正在写入
        /// </summary>
        bool _IsWritingNow = false;
        /// <summary>
        /// 标识刷新线程是否已经停止
        /// </summary>
        bool _IsPrviewStoped = false;
        /// <summary>
        /// 标识缓存线程是否已经停止
        /// </summary>
        bool _IsCacheStoped = false;

        bool _IsPreviewStarted = false;
        bool _IsCacheStarted = false;
        /// <summary>
        /// 本地文件记录
        /// </summary>
        private List<string> LocalFiles = new List<string>();
        /// <summary>
        /// 控制刷新的线程同步事件
        /// </summary>
        ManualResetEvent PreviewResetEvent = new ManualResetEvent(true);
        /// <summary>
        /// 线程写入同步上锁
        /// </summary>
        object locker = new object();
        /// <summary>
        /// 缓存队列
        /// </summary>
        Queue<Tuple<BitmapImage, string>>? SeSeCache = new Queue<Tuple<BitmapImage, string>>(20);
        /// <summary>
        /// 当前缓存中的文件记录
        /// </summary>
        List<string> CurrentRecord = new List<string>();
        /// <summary>
        /// 当前的缓存个数
        /// </summary>
        long CacheCount = 0;

        List<string> _removeList = new List<string>();
        #endregion
        #endregion

        #region SubModelRelated Properties

        private List<SubModelBase> models = new List<SubModelBase>();
        /// <summary>
        /// 子模块列表（V0.1.1暂未启用）
        /// </summary>
        public List<SubModelBase> Models
        {
            get => models;
            set => SetProperty(ref models, value);
        }

        #endregion

        #region Constructor
        public MainWorkSpace()
        {
            //启动timer
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            TaskbarIcon = ImageTool.GetImage(@".\Resources\timer-2.ico");
        }
        #endregion

        #region delegate
        /// <summary>
        /// 测试计数
        /// </summary>
        Stopwatch? sw = null;
        /// <summary>
        /// 星期对应中文字符
        /// </summary>
        string[] Day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        /// <summary>
        /// 计时器事件，用于触发背景的刷新事件（仅在图片模式下）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (seseCount == 0)
            {
                sw = Stopwatch.StartNew();
            }
            var curDate = DateTime.Now.ToLocalTime();

            CurrentTimeStr = curDate.ToString("yyyy-MM-dd HH:mm:ss");
            string week = Day[Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"))].ToString();
            week += (",今年的第" + DateTime.Now.DayOfYear + "天");
            CurrentWeekTimeStr = week;
            seseCount++;

            if (seseCount > MaxSeSeCount)
            {
                sw?.Stop();

                if (!IsWebViewVisible)
                {
                    Trace.WriteLine($"距离上一次触发刷新过去了{sw?.ElapsedMilliseconds}ms");
                    if (!IsBackgroundUsingVideo)
                        PreviewResetEvent.Set();
                    seseCount = 0;
                    Trace.WriteLine($"{DateTime.Now.ToLocalTime()}触发一次刷新");
                }

            }
        }

        public delegate void CloseWindowHandler();
        /// <summary>
        /// 关闭窗口的事件
        /// </summary>
        public event CloseWindowHandler? CloseWindow;

        public delegate void ChangeBackgroundVideoHandler(string VideoPath);
        /// <summary>
        /// 背景视频变更事件
        /// </summary>
        public event ChangeBackgroundVideoHandler? BackgroundVideoChanged;

        public delegate void ChangeVideoVolumnHandler(double value);
        /// <summary>
        /// 视频音量变更事件
        /// </summary>
        public event ChangeVideoVolumnHandler? VideoVolumnChanged;

        public delegate void ChangeWebSiteHandler(string url);
        /// <summary>
        /// 网址变更事件
        /// </summary>
        public event ChangeWebSiteHandler? WebSiteChanged;

        #endregion

        #region Commands
        ICommand? closeWindowCommand = null;
        /// <summary>
        /// 关闭窗口
        /// </summary>
        public ICommand CloseWindowCommand
        {
            get => closeWindowCommand ?? (closeWindowCommand = new RelayCommand(() =>
                {
                    CloseWindow?.Invoke();
                }));
        }

        ICommand? cleanDirCommand = null;
        /// <summary>
        /// 清理缓存文件夹
        /// </summary>
        public ICommand CleanDirCommand
        {
            get => cleanDirCommand ?? (cleanDirCommand = new RelayCommand(async () =>
                {
                    await CloseSese();
                    string currentDir = System.Environment.CurrentDirectory + "\\PictureCache";
                    if (Directory.Exists(currentDir) && !_IsWritingNow)
                        Directory.Delete(currentDir, true);
                    _shouldStopSese = false;
                    StartSeSeCacheThread();
                    StartSeSePreviewThread();
                }));
        }

        ICommand? iNeedSeseImmediately = null;
        /// <summary>
        /// 立即触发背景刷新（视频下即是立即到下一条视频）
        /// </summary>
        public ICommand INeedSeseImmediately
        {
            get => iNeedSeseImmediately ?? (iNeedSeseImmediately = new RelayCommand(() =>
                 {
                     if (IsBackgroundUsingVideo)
                         NextVideoCommand.Execute(null);
                     else
                     {
                         seseCount = 0;
                         if (ShouldPausePreview)
                             ShouldPausePreview = false;
                         PreviewResetEvent.Set();
                     }

                 }));
        }

        ICommand? openSettingCommand = null;
        /// <summary>
        /// 打开设置页面
        /// </summary>
        public ICommand OpenSettingCommand
        {
            get => openSettingCommand ?? (openSettingCommand = new RelayCommand(() =>
                  {
                      IsOpenSettingFlyout = true;
                  }));
        }

        ICommand? pinCurrentViewCommand = null;
        /// <summary>
        /// 暂停、继续刷新
        /// </summary>
        public ICommand PinCurrentViewCommand
        {
            get => pinCurrentViewCommand ?? (pinCurrentViewCommand = new RelayCommand(() =>
                 {
                     ShouldPausePreview = !ShouldPausePreview;
                 }));
        }

        ICommand? collectCurrentBackCommand = null;
        /// <summary>
        /// 收藏当前的背景
        /// </summary>
        public ICommand CollectCurrentBackCommand
        {
            get => collectCurrentBackCommand ?? (collectCurrentBackCommand = new RelayCommand(() =>
                {
                    try
                    {
                        if (File.Exists(CurrentPreviewFile))
                        {
                            var destFile = Path.Combine(CollectFileStoragePath, Path.GetFileName(CurrentPreviewFile));
                            if (!File.Exists(destFile))
                            {
                                File.Copy(CurrentPreviewFile, destFile);
                            }

                        }
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }));
        }

        ICommand? nextVideoCommand = null;
        /// <summary>
        /// 切换到下一条视频
        /// </summary>
        public ICommand NextVideoCommand
        {
            get => nextVideoCommand ?? (nextVideoCommand = new RelayCommand(() =>
                {
                    var Index = 0;
                    if (!string.IsNullOrEmpty(CurrentBackgroundVideoPath) && BackgroundVideos.Contains(CurrentBackgroundVideoPath))
                        Index = BackgroundVideos.IndexOf(CurrentBackgroundVideoPath);
                    if (Index + 1 >= BackgroundVideos.Count)
                        Index = 0;
                    else
                    {
                        Index += 1;
                    }
                    if (Index >= 0 && BackgroundVideos.Count > 0)
                        CurrentBackgroundVideoPath = BackgroundVideos[Index];
                }));
        }

        ICommand? muteVideoCommand = null;
        /// <summary>
        /// 视频静音
        /// </summary>
        public ICommand MuteVideoCommand
        {
            get => muteVideoCommand ?? (muteVideoCommand = new RelayCommand(() =>
                {

                    IsVideoMute = !IsVideoMute;
                }));
        }

        ICommand? hideTimerCommand = null;
        /// <summary>
        /// 隐藏中心的border
        /// </summary>
        public ICommand HideTimerCommand
        {
            get => hideTimerCommand ?? (hideTimerCommand = new RelayCommand(() =>
                  {
                      IsTimerBorderVisiable = !IsTimerBorderVisiable;
                  }));
        }


        ICommand? showWebUrlCommand = null;
        /// <summary>
        /// 显示网页设置
        /// </summary>
        public ICommand ShowWebUrlCommand
        {
            get => showWebUrlCommand ?? (showWebUrlCommand = new RelayCommand(() =>
                {
                    IsOpenWebUrlFlyOut = !IsOpenWebUrlFlyOut;
                }));
        }

        ICommand? recordHistoryCommand = null;
        /// <summary>
        /// 添加历史记录
        /// </summary>
        public ICommand RecordHistoryCommand
        {
            get => recordHistoryCommand ?? (recordHistoryCommand = new RelayCommand<string>((str) =>
                {
                    if (string.IsNullOrEmpty(str))
                        return;
                    var currentList = new List<string>(Histories);
                    currentList.Add(str);
                    Histories = new List<string>(currentList.Distinct(new StringDistinctItemComparer()));
                }));
        }

        #endregion

        #region private Methods


        /// <summary>
        /// 清理缓存文件夹
        /// </summary>
        private void ClearCacheDir()
        {

            if (!Directory.Exists(FileMapper.PictureCacheDir))

                if (!Directory.Exists(FileMapper.PictureCacheDir))
                    return;
            MyDirectory.GetFiles(FileMapper.PictureCacheDir, @"\.png$|\.jpg$|\.jpeg$|\.bmp$", SearchOption.AllDirectories).ToList().ForEach(o =>
             {
                 if (File.Exists(o))
                     File.Delete(o);
             });

        }

        /// <summary>
        /// api切换时，重启两个线程
        /// </summary>
        private async void ApiChanged()
        {
            if (IsOnlineSeSeMode)
            {
                await CloseSese();
                _shouldStopSese = false;
                StartSeSeCacheThread();
                StartSeSePreviewThread();
            }
        }
        /// <summary>
        /// 当前设置写入json
        /// </summary>
        public void WriteCurrentSettingToJson()
        {
            var curConfig = new Configure();
            curConfig.windowWidth = WindowWidth;
            curConfig.windowHeight = WindowHeight;
            curConfig.localFilePath = LocalFileDir;
            curConfig.backgroundImgOpacity = BackgroundImageOpacity;
            curConfig.isOnlineSeSeMode = IsOnlineSeSeMode;
            curConfig.currentSeSeApi = CurrentSeSeApi;
            curConfig.maxCacheCount = MaxCacheCount;
            curConfig.flushTime = MaxSeSeCount;
            curConfig.isTopmost = IsTopMost;
            curConfig.isUsingVideoBackGround = IsBackgroundUsingVideo;
            curConfig.timeFontIndex = FontFamilies.IndexOf(SelectedFontFamily) < 0 ? 0 : FontFamilies.IndexOf(SelectedFontFamily);
            curConfig.weekendFontIndex = FontFamilies.IndexOf(SelectedWeekendFontFamily) < 0 ? 0 : FontFamilies.IndexOf(SelectedWeekendFontFamily);
            curConfig.videoDir = VideoPathDir;
            curConfig.selectedVideoPath = CurrentBackgroundVideoPath;
            curConfig.timeFontSize = TimeCenterFontSize;
            curConfig.weekendFontSize = WeekendCenterFontSize;
            curConfig.weekendFontColor = ColorToStringHelper.HexConverter(WeekendFontColor);
            curConfig.timeFontColor = ColorToStringHelper.HexConverter(TimeFontColor);
            curConfig.isLoopPlay = IsLoopPlay;
            curConfig.volume = IsVideoMute ? originVolume : VideoVolume;
            curConfig.IsWebViewVisiable = IsWebViewVisible;
            curConfig.WebSiteUrl = CurrentWebAddress;
            curConfig.localCollectdPath = CollectFileStoragePath;
            var str = JsonConvert.SerializeObject(curConfig);
            File.WriteAllText(FileMapper.ConfigureJson, str);
        }
        /// <summary>
        /// 开启刷新控制线程
        /// </summary>
        private void StartSeSePreviewThread()
        {
            if (_IsPreviewStarted)
                return;
            Task.Run(() =>
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]开启预览控制线程");
                try
                {
                    _IsPreviewStarted = true;
                    while (!_shouldStopSese)
                    {
                        _IsPrviewStoped = false;
                        PreviewResetEvent.WaitOne();
                        if (SeSeCache == null)
                            continue;
                        if (ShouldPausePreview)
                            continue;
                        _IsWritingNow = true;
                        Tuple<BitmapImage, string>? curUrl = null;

                        lock (locker)
                        {
                            if (SeSeCache == null || SeSeCache.Count <= 0)
                                continue;
                            curUrl = SeSeCache.Dequeue();
                            if ((curUrl != null))
                            {
                                if(File.Exists(CurrentPreviewFile))
                                    _removeList.Add(CurrentPreviewFile);//获取到新的图像就可以去除上一个了
                                CurrentPreviewFile = curUrl.Item2;
                                CurrentSePic = curUrl.Item1;
                            }
                        }
                        UsedPicClean();
                        PreviewResetEvent.Reset();

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                }
                finally
                {
                    _IsPrviewStoped = true;
                    _IsPreviewStarted = false;
                    _IsWritingNow = false;
                }

            });
        }
        /// <summary>
        /// 开启缓存控制线程
        /// </summary>
        private void StartSeSeCacheThread()
        {
            SeSeCache = new Queue<Tuple<BitmapImage, string>>((int)MaxCacheCount);
            int localCount = 0;
            if (_IsCacheStarted)
                return;
            Task.Run(() =>
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]开启缓存控制线程");
                _IsCacheStarted = true;
                try
                {
                    while (!_shouldStopSese)
                    {

                        _IsCacheStoped = false;
                        if (SeSeCache == null || SeSeCache.Count >= MaxCacheCount)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        _IsWritingNow = true;
                        Action currentAction = IsOnlineSeSeMode ?
                        async () =>
                        {
                            string currentFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_FFFF");
                            switch (SeletctedSeSe)
                            {
                                case WebRequestsTool.seseUrlLevel1:
                                //case WebRequestsTool.seseUrlLevel2:
                                //case WebRequestsTool.seseUrlLevel3:
                                case WebRequestsTool.BackgroundUrl:
                                //case WebRequestsTool.mcUrl:
                                case WebRequestsTool.toubieUrl:
                                case WebRequestsTool.paugramUrl:
                                case WebRequestsTool.dmoeUrl:
                                case WebRequestsTool.yimianUrl:
                                    {
                                        var str = await WebRequestsTool.RequestSeSePic(SeletctedSeSe, FileMapper.NormalSeSePictureDir, currentFileName);
                                        if (File.Exists(str))
                                        {
                                            CurrentRecord.Add(str);
                                            CacheCount++;
                                            lock (locker)
                                            {
                                                SeSeCache?.Enqueue(new Tuple<BitmapImage, string>(ImageTool.GetImage(str), str));
                                            }

                                        }
                                        Debug.WriteLine($"{DateTime.Now.ToLocalTime()}请求一次涩涩{SeletctedSeSe}");
                                        break;
                                    }
                                    //case WebRequestsTool.pixivGetUrl:
                                    //{
                                    //    var res = await WebRequestsTool.RequestGetModePixivSeSePic(SeletctedSeSe, FileMapper.PixivSeSePictureDir, currentFileName);
                                    //    if (res == null)
                                    //        break;
                                    //    if (File.Exists(res.urls.First().Value))
                                    //    {
                                    //        CurrentRecord.Add(res.urls.First().Value);
                                    //        CacheCount++;
                                    //        lock (locker)
                                    //        {
                                    //            SeSeCache?.Enqueue(new Tuple<BitmapImage, string>(ImageTool.GetImage(res.urls.First().Value), res.urls.First().Value));
                                    //        }

                                    //    }

                                    //    Debug.WriteLine($"{DateTime.Now.ToLocalTime()}请求一次P站涩涩{SeletctedSeSe}");
                                    //    break;
                                    //}
                            }

                        }
                        :
                        () =>
                        {
                            if (localCount > LocalFiles.Count)
                                localCount = 0;
                            var currentFile = LocalFiles[localCount];
                            if (File.Exists(currentFile))
                            {
                                lock (locker)
                                {
                                    SeSeCache?.Enqueue(new Tuple<BitmapImage, string>(ImageTool.GetImage(currentFile), currentFile));
                                    localCount++;
                                }

                            }
                        };
                        currentAction();
                        if (CurrentSePic == null)
                        {
                            INeedSeseImmediately.Execute(null);
                        }
                        _IsWritingNow = false;
                        //var SleepCount = new Random().Next(100, 1000) % 1000;
                        Thread.Sleep(100);
                        //AutoClean();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);

                }
                finally
                {
                    _IsCacheStoped = true;
                    _IsCacheStarted = false;
                }

            });
        }
        /// <summary>
        /// 切换在线、本地模式
        /// </summary>
        private void SwitchSeSeMode()
        {
            if (!IsOnlineSeSeMode)
                LocalFiles = ReadLocalSeSe();
        }
        /// <summary>
        /// 读入本地文件夹的图片
        /// </summary>
        /// <returns></returns>
        private List<string> ReadLocalSeSe()
        {
            if (!Directory.Exists(LocalFileDir))
                return new List<string>();
            return MyDirectory.GetFiles(LocalFileDir, @"\.png$|\.jpg$|\.jpeg$|\.bmp$").ToList();
        }
        /// <summary>
        /// 读入目标文件夹的视频
        /// </summary>
        /// <param name="DirPath"></param>
        /// <returns></returns>
        private List<string> ReadDestVideo(string DirPath)
        {
            if (!Directory.Exists(DirPath))
                return new List<string>();
            var files = MyDirectory.GetFiles(DirPath, @"\.mp4$|\.mov$|\.flv$|\.mxf$|\.mkv$").ToList();
            return files;
        }
        /// <summary>
        /// 获取所有可被枚举的字体
        /// </summary>
        private void GetAllFont()
        {
            //InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = Fonts.SystemFontFamilies.ToArray();
            int Count = MyFontFamilies.Length;
            for (int i = 0; i < Count; i++)
            {

                FontFamilies.Add(MyFontFamilies[i]);
            }
            SelectedFontFamily = FontFamilies.FirstOrDefault();
            SelectedWeekendFontFamily = FontFamilies.FirstOrDefault();
        }
        /// <summary>
        /// 读入所有有记录的网站地址
        /// </summary>
        private void ReadWebSites()
        {
            var Webs = JsonConvert.DeserializeObject<WebUrlRecords>(File.ReadAllText(FileMapper.WebSiteJson));
            if (Webs == null)
            {
                Webs = new WebUrlRecords();
                Webs.webUrls = new List<string>();
                Webs.webUrls.Add("https://hfiprogramming.github.io/mikutap/");
                WriteWebSites();
            }
            WebAddresses = Webs.webUrls;
        }
        /// <summary>
        /// 写入网站地址
        /// </summary>
        private void WriteWebSites()
        {
            var str = JsonConvert.SerializeObject(new WebUrlRecords() { webUrls = WebAddresses });
            File.WriteAllText(FileMapper.ConfigureJson, str);
        }
   
        #endregion

        #region SubModelRelated Methods

        /// <summary>
        /// 读入模块后，相当于注册了这个插件
        /// </summary>
        void ReadModels()
        {
            var curModels = JsonConvert.DeserializeObject<ModelsDefination>(FileMapper.ModelsJson);
            if (curModels == null)
                return;
            var ModelsLists = curModels.SubModels.Where(x => x.Value == true);
            Models = ModelsLists.Select(x => x.Key).ToList();
        }


        #endregion

        #region Public methods
        /// <summary>
        /// 初始化，在窗口类初始化后执行
        /// </summary>
        public void Init()
        {


            var curConfig = JsonConvert.DeserializeObject<Configure>(File.ReadAllText(FileMapper.ConfigureJson));
            if (curConfig == null)
            {
                WriteCurrentSettingToJson();
                curConfig = JsonConvert.DeserializeObject<Configure>(File.ReadAllText(FileMapper.ConfigureJson));
            }
            else
            {
                WindowWidth = curConfig.windowWidth;
                WindowHeight = curConfig.windowHeight;
                LocalFileDir = curConfig.localFilePath;
                BackgroundImageOpacity = curConfig.backgroundImgOpacity;
                IsOnlineSeSeMode = curConfig.isOnlineSeSeMode;
                CurrentSeSeApi = curConfig.currentSeSeApi;
                MaxCacheCount = curConfig.maxCacheCount;
                MaxSeSeCount = curConfig.flushTime;
                IsTopMost = curConfig.isTopmost;
                IsWebViewVisible = curConfig.IsWebViewVisiable;
                CurrentWebAddress = curConfig.WebSiteUrl;
                CollectFileStoragePath = curConfig.localCollectdPath;
            }
            ReadWebSites();
            if (string.IsNullOrEmpty(CurrentWebAddress))
                CurrentWebAddress = WebAddresses.FirstOrDefault();
            //CacheResetSemaphore = new Semaphore((int)MaxCacheCount - 1, (int)MaxCacheCount);

            SeSeApis = new List<string>()
            {
                  WebRequestsTool.seseUrlLevel1,
                 //WebRequestsTool.seseUrlLevel2,
                  //WebRequestsTool.seseUrlLevel3,

                  //WebRequestsTool.mcUrl,
                  WebRequestsTool.toubieUrl,
                  WebRequestsTool.BackgroundUrl,
                  WebRequestsTool.paugramUrl,
                  WebRequestsTool.dmoeUrl,
                  WebRequestsTool.yimianUrl,
                   //WebRequestsTool.pixivGetUrl,
            };
            if (!SeSeApis.Contains(CurrentSeSeApi))
                SeSeApis.Add(CurrentSeSeApi);

            SeletctedSeSe = CurrentSeSeApi;
            GetAllFont();
            SelectedFontFamily = FontFamilies.ElementAt(curConfig.timeFontIndex);
            SelectedWeekendFontFamily = FontFamilies.ElementAt(curConfig.weekendFontIndex);
            TimeCenterFontSize = curConfig.timeFontSize;
            WeekendCenterFontSize = curConfig.weekendFontSize;
            TimeFontColor = ColorToStringHelper.HexConverter(curConfig.timeFontColor);
            WeekendFontColor = ColorToStringHelper.HexConverter(curConfig.weekendFontColor);
            ClearCacheDir();
            IsBackgroundUsingVideo = curConfig.isUsingVideoBackGround;
            videoPathDir = curConfig.videoDir;
            BackgroundVideos = ReadDestVideo(videoPathDir);
            IsLoopPlay = curConfig.isLoopPlay;
            VideoVolume = curConfig.volume;
            if (!IsWebViewVisible)
            {
                if (!IsBackgroundUsingVideo)
                {
                    StartSeSeCacheThread();
                    StartSeSePreviewThread();

                }
                else
                {
                    if (!string.IsNullOrEmpty(curConfig.selectedVideoPath))
                        CurrentBackgroundVideoPath = curConfig.selectedVideoPath;

                    else
                        CurrentBackgroundVideoPath = BackgroundVideos.FirstOrDefault();
                }
            }

        }
        /// <summary>
        /// 关闭刷新、缓存线程
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CloseSese()
        {

            PreviewResetEvent.Set();

            CacheCount = 0;
            if(CurrentRecord!=null)
            {
                CurrentRecord.ForEach(x=>
                {
                    if(File.Exists(x))
                    File.Delete(x);
                });
                CurrentRecord.Clear();
            }

            _shouldStopSese = true;
            lock (locker)
            {
                SeSeCache?.Clear();
                SeSeCache = null;
            }

            return await Task.Run(() =>
            {
                while (!_IsCacheStoped || !_IsPrviewStoped)
                    Thread.Sleep(1);
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]已关闭预览和缓存线程");
                return true;
            });

        }

        /// <summary>
        /// 自动清理（根据最大缓存数）
        /// </summary>
        [Obsolete]
        public void AutoClean()
        {
            if (CacheCount <= MaxCacheCount + 1 || !IsOnlineSeSeMode)
                return;
            var RemoveList = new List<string>();

            var diff = CacheCount - MaxCacheCount - 1;

            for (int i = 0; i < diff; i++)
            {
                if (!File.Exists(CurrentRecord[i]))
                {
                    RemoveList.Add(CurrentRecord[i]);
                    continue;
                }

                File.Delete(CurrentRecord[i]);
                RemoveList.Add(CurrentRecord[i]);
            }


            RemoveList.ForEach(o => { CurrentRecord.Remove(o); });
            CacheCount -= RemoveList.Count;
            Trace.WriteLine($"自动清理了{RemoveList.Count}个本地缓存文件,当前缓存数{CurrentRecord.Count}");
        }

        /// <summary>
        /// 清理已被使用过的图片
        /// </summary>
        public void UsedPicClean()
        {
            try
            {
                lock(_removeList)
                {
                    var markList = new List<string>();
                    foreach(var itr in _removeList)
                    {
                        if (File.Exists(itr))
                            File.Delete(itr);
                        markList.Add(itr);
                    }
                    markList.ForEach(x=> _removeList.Remove(x));
                }

            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// 设置快捷键描述
        /// </summary>
        /// <param name="str"></param>
        public void SetShotKeyDiscribe(List<HotKey> hotKeys)
        {
            ShotKeys = hotKeys;
        }
        #endregion
    }
}
