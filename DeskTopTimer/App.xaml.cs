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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            Unosquare.FFME.Library.FFmpegDirectory = @".\Resources";

            DeleteCefBrowserCache();
            var setting = new CefSettings();
            setting.RootCachePath = FileMapper.CefBrowserDataDir;
            setting.LogFile = FileMapper.CefBrowserLogPath;
            setting.CachePath = FileMapper.CefBrowserCacheDir;
            setting.UserDataPath = FileMapper.CefBrowserUserDataDir;
            setting.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102";
            setting.UncaughtExceptionStackSize = 10;
            bool performDependencyCheck = !DebuggingSubProcess;

            if (!Cef.Initialize(setting, performDependencyCheck: performDependencyCheck))
            {
                throw new Exception("Unable to Initialize Cef");
            }
        }
    }
}
