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
    public static class FontGetLocalizeName
    {
        public static string GetLocalizedName(this FontFamily font)
        {
            LanguageSpecificStringDictionary familyNames = font.FamilyNames;
            if (familyNames.ContainsKey(XmlLanguage.GetLanguage("zh-cn")))
            {
                if (familyNames.TryGetValue(XmlLanguage.GetLanguage("zh-cn"), out var chineseFontName))
                {
                    return chineseFontName;
                }
            }
            return familyNames.FirstOrDefault().Value;
        }
    }

    internal class MainWorkSpace : ObservableObject
    {
        #region PerformRegion

        #region TextControl

        private string _currentTimeStr = string.Empty;
        public string CurrentTimeStr
        {
            get => _currentTimeStr;
            set => SetProperty(ref _currentTimeStr, value);

        }

        private string _currentWeekTimeStr = string.Empty;
        public string CurrentWeekTimeStr
        {
            get => _currentWeekTimeStr;
            set => SetProperty(ref _currentWeekTimeStr, value);
        }

        #endregion



        #region BackGroundControl


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


        #region ImageControl

        private BitmapImage? currentSePic = null;
        public BitmapImage? CurrentSePic
        {
            get => currentSePic;
            set => SetProperty(ref currentSePic, value);
        }

        private double backgroundImageOpacity = 1d;
        public double BackgroundImageOpacity
        {
            get => backgroundImageOpacity;
            set => SetProperty(ref backgroundImageOpacity, value);
        }

        #region ImagePath Control


        private List<string> seSeApis = new List<string>();
        public List<string> SeSeApis
        {
            get => seSeApis;
            set => SetProperty(ref seSeApis, value);
        }

        private string selectedSeSe = string.Empty;
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
        public string CurrentSeSeApi
        {
            get => currentSeSeApi;
            set => SetProperty(ref currentSeSeApi, value);
        }

        private string localFileDir = FileMapper.LocalSeSePictureDir;
        public string LocalFileDir
        {
            get => localFileDir;
            set => SetProperty(ref localFileDir, value);
        }



        private long maxCacheCount = 20;//默认缓存20张
        public long MaxCacheCount
        {
            get => maxCacheCount;
            set => SetProperty(ref maxCacheCount, value);
        }

        private long maxSeSeCount = 10;//默认10秒
        public long MaxSeSeCount
        {
            get => maxSeSeCount;
            set => SetProperty(ref maxSeSeCount, value);
        }

        private bool shouldPausePreview = false;
        public bool ShouldPausePreview
        {
            get => shouldPausePreview;
            set => SetProperty(ref shouldPausePreview, value);
        }

        private string currentPreviewFile = string.Empty;
        public string CurrentPreviewFile
        {
            get => currentPreviewFile;
            set => SetProperty(ref currentPreviewFile, value);
        }

        #endregion
        #endregion

        #region VideoControl
        private bool isLoopPlay = true;
        public bool IsLoopPlay
        {
            get => isLoopPlay;
            set
            {
                SetProperty(ref isLoopPlay, value);
            }
        }



        private string videoPathDir = FileMapper.VideoCacheDir;
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
        public List<string> BackgroundVideos
        {
            get => backgroundVideos;
            set => SetProperty(ref backgroundVideos, value);
        }

        private string? currentBackgroundVideoPath = string.Empty;
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
        public double WindowWidth
        {
            get => windowWidth;
            set => SetProperty(ref windowWidth, value);
        }

        private double windowHeight = 300d;
        public double WindowHeight
        {
            get => windowHeight;
            set => SetProperty(ref windowHeight, value);
        }


        private bool isOpenSettingFlyout = false;
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

        private bool isTopMost = true;
        public bool IsTopMost
        {
            get => isTopMost;
            set => SetProperty(ref isTopMost, value);

        }

        #endregion

        #region FontRelated

        private List<FontFamily> fontFamilies = new List<FontFamily>();
        public List<FontFamily> FontFamilies
        {
            get => fontFamilies;
            set => SetProperty(ref fontFamilies, value);
        }

        private FontFamily? selectedFontFamily = null;
        public FontFamily? SelectedFontFamily
        {
            get => selectedFontFamily;
            set => SetProperty(ref selectedFontFamily, value);
        }

        private FontFamily? selectedWeekendFontFamily = null;
        public FontFamily? SelectedWeekendFontFamily
        {
            get => selectedWeekendFontFamily;
            set => SetProperty(ref selectedWeekendFontFamily, value);
        }

        private Color timeFontColor = Colors.White;
        public Color TimeFontColor
        {
            get => timeFontColor;
            set
            {
                SetProperty(ref timeFontColor, value);
                OnPropertyChanged("TimeFontBrush");
            }
        }

        public Brush TimeFontBrush
        {
            get => new SolidColorBrush(TimeFontColor);
        }

        private Color weekendFontColor = Colors.White;
        public Color WeekendFontColor
        {
            get => weekendFontColor;
            set
            {
                SetProperty(ref weekendFontColor, value);
                OnPropertyChanged("WeekendFontBrush");
            }
        }

        public Brush WeekendFontBrush
        {
            get => new SolidColorBrush(WeekendFontColor);
        }

        private int timeCenterFontSize = 20;
        public int TimeCenterFontSize
        {
            get => timeCenterFontSize;
            set => SetProperty(ref timeCenterFontSize, value);
        }

        private int weekendCenterFontSize = 12;
        public int WeekendCenterFontSize
        {
            get => weekendCenterFontSize;
            set => SetProperty(ref weekendCenterFontSize, value);
        }


        #endregion

        #endregion

        #region Private 
        long seseCount = 0;


        System.Timers.Timer timer = new System.Timers.Timer();

        WebRequestsTool WebRequestsTool = new WebRequestsTool();



        #region thread Control
        bool _shouldStopSese = false;
        bool _IsWritingNow = false;

        bool _IsPrviewStoped = false;
        bool _IsCacheStoped = false;

        private List<string> LocalFiles = new List<string>();
        ManualResetEvent PreviewResetEvent = new ManualResetEvent(true);
        object locker = new object();
        Queue<Tuple<BitmapImage, string>>? SeSeCache = new Queue<Tuple<BitmapImage, string>>(20);

        List<string> CurrentRecord = new List<string>();
        long CacheCount = 0;
        #endregion
        #endregion

        #region SubModelRelated Properties

        private List<SubModelBase> models = new List<SubModelBase>();
        public List<SubModelBase> Models
        {
            get => models;
            set => SetProperty(ref models, value);
        }

        #endregion

        #region Constructor
        public MainWorkSpace()
        {
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        #endregion

        #region delegate

        double originVolume = 0d;
        Stopwatch? sw = null;
        string[] Day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
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
                Trace.WriteLine($"距离上一次触发刷新过去了{sw?.ElapsedMilliseconds}ms");
                PreviewResetEvent.Set();
                seseCount = 0;
                Trace.WriteLine($"{DateTime.Now.ToLocalTime()}触发一次刷新");
            }
        }

        public delegate void CloseWindowHandler();
        public event CloseWindowHandler? CloseWindow;

        public delegate void ChangeBackgroundVideoHandler(string VideoPath);
        public event ChangeBackgroundVideoHandler? BackgroundVideoChanged;

        public delegate void ChangeVideoVolumnHandler(double value);
        public event ChangeVideoVolumnHandler? VideoVolumnChanged;


        #endregion

        #region Commands
        ICommand? closeWindowCommand = null;
        public ICommand CloseWindowCommand
        {
            get => closeWindowCommand ?? (closeWindowCommand = new RelayCommand(() =>
                {
                    CloseWindow?.Invoke();
                }));
        }

        ICommand? cleanDirCommand = null;
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
        public ICommand OpenSettingCommand
        {
            get => openSettingCommand ?? (openSettingCommand = new RelayCommand(() =>
                  {
                      IsOpenSettingFlyout = true;
                  }));
        }

        ICommand? pinCurrentViewCommand = null;
        public ICommand PinCurrentViewCommand
        {
            get => pinCurrentViewCommand ?? (pinCurrentViewCommand = new RelayCommand(() =>
                 {
                     ShouldPausePreview = !ShouldPausePreview;
                 }));
        }

        ICommand? collectCurrentBackCommand = null;
        public ICommand CollectCurrentBackCommand
        {
            get => collectCurrentBackCommand ?? (collectCurrentBackCommand = new RelayCommand(() =>
                {
                    if (File.Exists(CurrentPreviewFile))
                    {
                        File.Copy(CurrentPreviewFile, Path.Combine(FileMapper.LocalCollectionPictureDir, Path.GetFileName(CurrentPreviewFile)));
                    }
                }));
        }

        ICommand? nextVideoCommand = null;
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
        public ICommand MuteVideoCommand
        {
            get => muteVideoCommand ?? (muteVideoCommand = new RelayCommand(() =>
                {

                    IsVideoMute = !IsVideoMute;
                }));
        }

        #endregion

        #region private Methods



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
            var str = JsonConvert.SerializeObject(curConfig);
            File.WriteAllText(FileMapper.ConfigureJson, str);
        }

        private void StartSeSePreviewThread()
        {
            Task.Run(() =>
            {
                try
                {
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
                            if (SeSeCache==null||SeSeCache.Count <= 0)
                                continue;
                            curUrl = SeSeCache.Dequeue();
                            if ((curUrl != null))
                            {
                                CurrentPreviewFile = curUrl.Item2;
                                CurrentSePic = curUrl.Item1;
                                //CacheResetSemaphore?.Release(1);
                            }
                        }

                        PreviewResetEvent.Reset();
                        _IsWritingNow = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                }
                finally
                {
                    _IsPrviewStoped = true;
                }

            });
        }

        private void StartSeSeCacheThread()
        {
            SeSeCache = new Queue<Tuple<BitmapImage, string>>((int)MaxCacheCount);
            int localCount = 0;
            Task.Run(() =>
            {
                while (!_shouldStopSese)
                {

                    _IsCacheStoped = false;
                    if (SeSeCache == null || SeSeCache.Count >= MaxCacheCount)
                        continue;
                    _IsWritingNow = true;
                    Action currentAction = IsOnlineSeSeMode ?
                    async () =>
                    {
                        string currentFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_FFFF");
                        switch (SeletctedSeSe)
                        {
                            case WebRequestsTool.seseUrlLevel1:
                            case WebRequestsTool.seseUrlLevel2:
                            case WebRequestsTool.seseUrlLevel3:

                            case WebRequestsTool.mcUrl:
                            case WebRequestsTool.toubieUrl:

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
                            case WebRequestsTool.pixivGetUrl:
                                {
                                    var res = await WebRequestsTool.RequestGetModePixivSeSePic(SeletctedSeSe, FileMapper.PixivSeSePictureDir, currentFileName);
                                    if (res == null)
                                        break;
                                    if (File.Exists(res.urls.First().Value))
                                    {
                                        CurrentRecord.Add(res.urls.First().Value);
                                        CacheCount++;
                                        lock (locker)
                                        {
                                            SeSeCache?.Enqueue(new Tuple<BitmapImage, string>(ImageTool.GetImage(res.urls.First().Value), res.urls.First().Value));
                                        }

                                    }

                                    Debug.WriteLine($"{DateTime.Now.ToLocalTime()}请求一次P站涩涩{SeletctedSeSe}");
                                    break;
                                }
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
                            }

                        }
                    };
                    currentAction();
                    if (CurrentSePic == null)
                    {
                        INeedSeseImmediately.Execute(null);
                    }
                    _IsWritingNow = false;
                    var SleepCount = Random.Shared.Next(500, 1000) % 1000;
                    Thread.Sleep(SleepCount);
                    AutoClean();
                }
                _IsCacheStoped = true;
            });
        }

        private void SwitchSeSeMode()
        {
            if (!IsOnlineSeSeMode)
                LocalFiles = ReadLocalSeSe();
        }

        private List<string> ReadLocalSeSe()
        {
            if (!Directory.Exists(LocalFileDir))
                return new List<string>();
            return MyDirectory.GetFiles(LocalFileDir, @"\.png$|\.jpg$|\.jpeg$|\.bmp$").ToList();
        }

        private List<string> ReadDestVideo(string DirPath)
        {
            if (!Directory.Exists(DirPath))
                return new List<string>();
            var files = MyDirectory.GetFiles(DirPath, @"\.mp4$|\.mov$|\.flv$|\.mxf$|\.mkv$").ToList();
            return files;
        }

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
                WindowWidth = curConfig.windowWidth.Value;
                WindowHeight = curConfig.windowHeight.Value;
                LocalFileDir = curConfig.localFilePath;
                BackgroundImageOpacity = curConfig.backgroundImgOpacity.Value;
                IsOnlineSeSeMode = curConfig.isOnlineSeSeMode.Value;
                CurrentSeSeApi = curConfig.currentSeSeApi;
                MaxCacheCount = curConfig.maxCacheCount.Value;
                MaxSeSeCount = curConfig.flushTime.Value;
                IsTopMost = curConfig.isTopmost;
            }
            //CacheResetSemaphore = new Semaphore((int)MaxCacheCount - 1, (int)MaxCacheCount);

            SeSeApis = new List<string>()
            {
                WebRequestsTool.seseUrlLevel1,
                 WebRequestsTool.seseUrlLevel2,
                  WebRequestsTool.seseUrlLevel3,

                  WebRequestsTool.mcUrl,
                  WebRequestsTool.toubieUrl,

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
            if (!IsBackgroundUsingVideo)
            {
                StartSeSeCacheThread();
                StartSeSePreviewThread();

            }
            else
            {
                CurrentBackgroundVideoPath = curConfig.selectedVideoPath;


            }
        }

        public async Task<bool> CloseSese()
        {

            PreviewResetEvent.Set();

            CacheCount = 0;
            CurrentRecord.Clear();
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
                return true;
            });

        }

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

        #endregion
    }
}
