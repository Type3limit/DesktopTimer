using CefSharp;
using CefSharp.Handler;
using CefSharp.Wpf;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DeskTopTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Semaphore semaphore;
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        /// <summary>
        /// 清除CefBrowser缓存
        /// </summary>
        private static void DeleteCefBrowserCache()
        {
            try
            {
                if (Directory.Exists(FileMapper.CefBrowserDataDir))
                {
                    //清除文件夹
                    var dirs = Directory.GetDirectories(FileMapper.CefBrowserDataDir);
                    if (dirs != null && dirs.Length > 0)
                    {
                        foreach (var dir in dirs)
                        {
                            //Local Storage文件夹内存在用户登录信息,不删除
                            //if (!dir.Contains("Local Storage"))
                            //{
                            //    Directory.Delete(dir, true);
                            //}

                            Directory.Delete(dir, true);
                        }
                    }

                    //清除文件
                    var files = Directory.GetFiles(FileMapper.CefBrowserDataDir);
                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            //不清除日志文件
                            if (file != FileMapper.CefBrowserLogPath)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]{Environment.NewLine}" +
                    $"{e}");
            }
        }
        private static readonly bool DebuggingSubProcess = Debugger.IsAttached;
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //  Host.CreateDefaultBuilder(args)
        //  .ConfigureAppConfiguration(config => {
        //       //读取Json配置文件（读取日志的配置，这是设置为热更新）
        //       config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        //  }).ConfigureLogging(log => {
        //       //日志添加到控制台
        //       log.AddConsole();
        //       //日志添加到debug调试窗口
        //       log.AddDebug();
        //  })
        //      .ConfigureWebHostDefaults(webBuilder =>
        //      {
        //          webBuilder.UseStartup<Startup>();
        //      });
        protected override void OnStartup(StartupEventArgs e)
        {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
# endif
            bool createdNew;
            var currentProgramName = Assembly.GetExecutingAssembly().GetName().Name;
            semaphore = new Semaphore(0, 1, currentProgramName, out createdNew);
            if(!createdNew)
            {
               
                Process[] temp = Process.GetProcessesByName(currentProgramName);//在所有已启动的进程中查找需要的进程；  
                if (temp.Length > 0)//如果查找到  
                {
                    IntPtr handle = temp.Last().MainWindowHandle;
                    SwitchToThisWindow(handle, true);    // 激活，显示在最前  
                }
                Environment.Exit(-2);
                return;
            }
            base.OnStartup(e);
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            Unosquare.FFME.Library.FFmpegDirectory = @".\Resources";
            string strMenu = AppDomain.CurrentDomain.BaseDirectory;
            //pepflashplayerDLL 地址
            string flashPath = strMenu + @"\Resources\pepflashplayer64_34_0_0_211.dll";
            DeleteCefBrowserCache();
            var setting = new CefSettings();
            setting.RootCachePath = FileMapper.CefBrowserDataDir;
            setting.PersistSessionCookies = true;
            setting.LogFile = FileMapper.CefBrowserLogPath;
            setting.CachePath = FileMapper.CefBrowserCacheDir;
            setting.UserDataPath = FileMapper.CefBrowserUserDataDir;
            setting.PersistUserPreferences = true;
            setting.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36 Edg/96.0.1054.53";
            //setting.UserAgent= "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.1.3 Safari/605.1.15";
            setting.CefCommandLineArgs.Add("--ignore-urlfetcher-cert-requests", "1");
            setting.CefCommandLineArgs.Add("--ignore-certificate-errors", "1");
            //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            setting.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            setting.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
            setting.CefCommandLineArgs.Add("enable-npapi", "1");
            //setting.CefCommandLineArgs.Add("enable-system-flash", "1"); //Automatically discovered and load a system-wide installation of Pepper Flash
            setting.CefCommandLineArgs.Add("ppapi-flash-path", flashPath); //Load a specific pepper flash version (Step 1 of 2)
            setting.CefCommandLineArgs.Add("ppapi-flash-version", "34.0.0.211"); //Load a specific pepper flash version (Step 2 of 2)
            setting.CefCommandLineArgs.Add("enable-media-stream", "enable-media-stream");
            setting.UncaughtExceptionStackSize = 10;
            bool performDependencyCheck = !DebuggingSubProcess;

            if (!Cef.Initialize(setting,performDependencyCheck,browserProcessHandler:null))
            {
                throw new Exception("Unable to Initialize Cef");
            }
            AddTraceListener();
        }



        private void AddTraceListener()
        {
            LogHelper.ClearLogFiles();
            StreamWriter writer = new StreamWriter(FileMapper.CurrentLogFile, false);
            TextWriterTraceListener listener = new TextWriterTraceListener(writer, "log");
            //TextWriterTraceListener listener = new TextWriterTraceListener(logfile, "log");
            listener.TraceOutputOptions = TraceOptions.Callstack;
            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;
        }
    }
}
