using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CefSharp;
using Microsoft.Win32;
using Newtonsoft.Json;
namespace DeskTopTimer
{
    /// <summary>
    /// Bitmap 转为BitmapImage
    /// </summary>
    public class BitmapToImageSourceHelper
    {
        /// <summary>
        /// 将bitmap转换为BitmapImage
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        static public BitmapImage Convert(Bitmap src)
        {
            try
            {
                if (src == null)
                    return null;
                MemoryStream ms = new MemoryStream();
                ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                return image;


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }
    /// <summary>
    /// 枚举文件
    /// </summary>
    public static class MyDirectory
    {   // Regex version
        public static IEnumerable<string> GetFiles(string path,
                            string searchPatternExpression = "",
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                            .Where(file =>
                                     reSearchPattern.IsMatch(Path.GetExtension(file)));
        }

        // Takes same patterns, and executes in parallel
        public static IEnumerable<string> GetFiles(string path,
                            string[] searchPatterns,
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel()
                   .SelectMany(searchPattern =>
                          Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }
    /// <summary>
    /// 图像相关处理
    /// </summary>
    public class ImageTool
    {
        public struct Dpi
        {
            public double X { get; set; }

            public double Y { get; set; }

            public Dpi(double x, double y)
            {
                X = x;
                Y = y;
            }
        }


        public static BitmapImage GetImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = null;

                if (imagePath.StartsWith("pack://"))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    Uri current;
                    if (Uri.TryCreate(imagePath, UriKind.RelativeOrAbsolute, out current))
                    {
                        bitmap.UriSource = current;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
                else if (File.Exists(imagePath))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                    {
                        if (ms.Length <= 0)
                        {
                            bitmap.EndInit();
                            bitmap.Freeze();
                            return bitmap;
                        }
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new BitmapImage();
            }

        }



        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);


        public static Dpi GetDpiByGraphics()
        {
            double dpiX;
            double dpiY;

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            return new Dpi(dpiX, dpiY);
        }

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }


        static public DrawingImage CreateABitMap(string DrawingText, double FontSize, Typeface cur)
        {
            var pixels = new byte[1080 * 1080 * 4];
            for (int i = 0; i < 1080 * 1080 * 4; i += 4)
            {
                pixels[i] = 0;
                pixels[i + 1] = 0;
                pixels[i + 2] = 0;
                pixels[i + 3] = 255;
            }
            BitmapSource bitmapSource = BitmapSource.Create(1080, 1080, 96, 96, PixelFormats.Pbgra32, null, pixels, 1080 * 4);
            var visual = new DrawingVisual();

            var CenterX = 540;
            var CenterY = 540;
            var Dpi = GetDpiByGraphics();//GetSystemDpi
            var formatText = new FormattedText(DrawingText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        cur, FontSize, System.Windows.Media.Brushes.White, Dpi.X / 96d);
            System.Windows.Point textLocation = new System.Windows.Point(CenterX - formatText.WidthIncludingTrailingWhitespace / 2, CenterY - formatText.Height / 2);

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, 1080, 1080));
                drawingContext.DrawText(formatText, textLocation);
            }
            return new DrawingImage(visual.Drawing);
        }

        static public bool SaveDrawingToFile(DrawingImage drawing, string fileName, double scale = 1d)
        {
            drawing.Freeze();
            return System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var drawingImage = new System.Windows.Controls.Image { Source = drawing };
                    var width = drawing.Width * scale;
                    var height = drawing.Height * scale;
                    drawingImage.Arrange(new Rect(0, 0, width, height));

                    var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(drawingImage);

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            });


        }




    }

    /// <summary>
    /// 路径定义
    /// </summary>
    public static class FileMapper
    {
        /// <summary>
        /// 图片缓存目录
        /// </summary>
        public static string PictureCacheDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PictureCache";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        /// <summary>
        /// 视频缓存目录
        /// </summary>
        public static string VideoCacheDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Videos";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        /// <summary>
        /// 普通背景目录
        /// </summary>
        public static string NormalSeSePictureDir
        {
            get
            {
                var cur = Path.Combine(PictureCacheDir, "Normal");
                if (!Directory.Exists(cur))
                    Directory.CreateDirectory(cur);
                return cur;
            }
        }
        /// <summary>
        /// 特定的背景目录
        /// </summary>
        public static string PixivSeSePictureDir
        {
            get
            {
                var cur = Path.Combine(PictureCacheDir, "Pixiv");
                if (!Directory.Exists(cur))
                    Directory.CreateDirectory(cur);
                return cur;
            }
        }
        /// <summary>
        /// 本地背景目录
        /// </summary>
        public static string LocalSeSePictureDir
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Local");
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// 收藏目录
        /// </summary>
        public static string LocalCollectionPictureDir
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Collect");
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// json配置文件路径
        /// </summary>
        public static string ConfigureJson
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration.Json");
                if (!File.Exists(currentFile))
                    File.Create(currentFile).Close();
                return currentFile;
            }
        }
        /// <summary>
        /// 模块json配置路径
        /// </summary>
        public static string ModelsJson
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Models.Json");
                if (!File.Exists(currentFile))
                    File.Create(currentFile).Close();
                return currentFile;
            }
        }
        /// <summary>
        /// 网址记录json路径
        /// </summary>
        public static string WebSiteJson
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Webs.Json");
                if (!File.Exists(currentFile))
                    File.Create(currentFile).Close();
                return currentFile;
            }
        }
        /// <summary>
        /// CefBrowser缓存数据根目录
        /// </summary>
        public static string CefBrowserDataDir
        {
            get
            {
                string cefBrowserData = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"CefBrowserData\");
                if (!Directory.Exists(cefBrowserData))
                {
                    Directory.CreateDirectory(cefBrowserData);
                }
                return cefBrowserData;
            }
        }
        /// <summary>
        /// 网络日志目录
        /// </summary>
        public static string CefBrowserLogPath
        {
            get
            {
                string cefBrowserLogPath = Path.Combine(CefBrowserDataDir, "CefBrowser.log");
                if (!File.Exists(cefBrowserLogPath))
                {
                    File.Create(cefBrowserLogPath);
                }
                return cefBrowserLogPath;
            }
        }
        /// <summary>
        /// 网络缓存目录
        /// </summary>
        public static string CefBrowserCacheDir
        {
            get
            {
                string cefBrowserCache = Path.Combine(CefBrowserDataDir, @"Cache\");
                if (!Directory.Exists(cefBrowserCache))
                {
                    Directory.CreateDirectory(cefBrowserCache);
                }
                return cefBrowserCache;
            }
        }
        /// <summary>
        /// 网络用户信息缓存
        /// </summary>
        public static string CefBrowserUserDataDir
        {
            get
            {
                string cefBrowserUserData = Path.Combine(CefBrowserDataDir, @"UserData\");
                if (!Directory.Exists(cefBrowserUserData))
                {
                    Directory.CreateDirectory(cefBrowserUserData);
                }
                return cefBrowserUserData;
            }
        }
    }

    /// <summary>
    /// 设置类定义
    /// </summary>
    public class Configure
    {
        public bool isOnlineSeSeMode { set;get;}

        public string? localFilePath { set;get;}

        public string? currentSeSeApi { set;get;}

        public double windowWidth { set;get;}

        public double windowHeight { set;get;}

        public double backgroundImgOpacity { set;get;}

        public long maxCacheCount { set;get;}

        public long flushTime { set;get;}
        
        public bool isTopmost { set;get;}=true;

        public int timeFontIndex { set;get;}=0;

        public int weekendFontIndex { set;get;}=0;

        public int timeFontSize { set;get;} =20;

        public int weekendFontSize { set;get;}=12;

        public string? timeFontColor { set;get;}
        
        public string? weekendFontColor { set;get;}

        public bool isUsingVideoBackGround { set; get; }=false;

        public string? videoDir { set; get; }

        public string? selectedVideoPath { set; get; }

        public string? localCollectdPath { set;get;}

        public string? WebSiteUrl { set;get;}

        public bool IsWebViewVisiable { set;get;}

        public bool isLoopPlay { set;get;}=true;

        public double volume { set;get;} =1d;
    }

    /// <summary>
    /// 网络地址记录
    /// </summary>
    public class WebUrlRecords
    {
        public List<string>? webUrls{set;get;} 
    }

    /// <summary>
    /// string 唯一
    /// </summary>
    class StringDistinctItemComparer : IEqualityComparer<string>
    {

        public bool Equals(string x, string y)
        {
            return x == y;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// 通用方法帮助类
    /// </summary>
    public static class CommonFuncTool
    {
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="DestName">压缩文件的存储绝对路径</param>
        /// <param name="FilePathToEntryDic">文件名：压缩文件内文件名Map</param>
        /// <param name="compressionLevel">压缩等级</param>
        /// <returns></returns>
        public static bool ZipFiles(string DestName, Dictionary<string, string> FilePathToEntryDic, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            try
            {
               // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (ZipArchive zipArchive = ZipFile.Open(DestName, ZipArchiveMode.Create, Encoding.GetEncoding("UTF8")))
                {

                    foreach (var itr in FilePathToEntryDic.Keys)
                    {
                        zipArchive.CreateEntryFromFile(itr, FilePathToEntryDic[itr], compressionLevel);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="ZipFilePath">压缩文件所在路径</param>
        /// <param name="UnZipPath">解压到</param>
        /// <returns></returns>
        public static bool UnZipFile(string ZipFilePath, string UnZipPath)
        {
            try
            {
                var startPath = System.IO.Path.GetDirectoryName(ZipFilePath);
                if (Directory.Exists(UnZipPath))
                {
                    Directory.Delete(UnZipPath, true);
                }
                ZipFile.ExtractToDirectory(ZipFilePath, UnZipPath);
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("解压出现问题" + ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// 颜色转字符、字符转颜色
    /// </summary>
    public static class ColorToStringHelper
    {
        public static String HexConverter(System.Windows.Media.Color c, bool UsingAlpha = true)
        {
            if (UsingAlpha)
                return "#" + c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            else
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static int HexToInt(char hexChar)
        {
            hexChar = char.ToUpper(hexChar);  // may not be necessary

            return (int)hexChar < (int)'A' ?
                ((int)hexChar - (int)'0') :
                10 + ((int)hexChar - (int)'A');
        }

        public static System.Windows.Media.Color HexConverter(string color, bool UsingAlpha = true)
        {
           
            if(string.IsNullOrEmpty(color))
                return Colors.White;
            if (UsingAlpha)
            {
                var currentArray = color.Skip(1).ToArray();
                byte a = Convert.ToByte(currentArray[0].ToString()+currentArray[1].ToString(),16);
                byte r = Convert.ToByte(currentArray[2].ToString() + currentArray[3].ToString(),16);
                byte g = Convert.ToByte(currentArray[4].ToString() + currentArray[5].ToString(),16);
                byte b = Convert.ToByte(currentArray[6].ToString() + currentArray[7].ToString(),16);
                return System.Windows.Media.Color.FromArgb(a,r,g,b);
            }
            else
            {
                var currentArray = color.ToArray();
                byte r = Convert.ToByte(currentArray[0].ToString() + currentArray[1].ToString(),16);
                byte g = Convert.ToByte(currentArray[2].ToString() + currentArray[3].ToString(),16);
                byte b = Convert.ToByte(currentArray[4].ToString() + currentArray[5].ToString(),16);
                return System.Windows.Media.Color.FromRgb(r, g, b);
            }
                
        }

        public static String RGBConverter(System.Windows.Media.Color c)
        {
            return "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
        }

    }
    /// <summary>
    /// 获取字体的本地化名称
    /// </summary>
    public static class FontGetLocalizeName
    {
        public static string GetLocalizedName(this System.Windows.Media.FontFamily font)
        {
            LanguageSpecificStringDictionary familyNames = font.FamilyNames;
            if (familyNames.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn")))
            {
                if (familyNames.TryGetValue(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn"), out var chineseFontName))
                {
                    return chineseFontName;
                }
            }
            return familyNames.FirstOrDefault().Value;
        }
    }
    /// <summary>
    /// web Cookie
    /// </summary>
    class CookieVisitor : ICookieVisitor
    {
        public string name { set;get;}
        public string value { set;get;}
        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            name = cookie.Name;
            value = cookie.Value;
            return true;
        }
        public void Dispose()
        {
        }
    }
    /// <summary>
    /// 获取已安装的程序列表
    /// </summary>
    public static class AppListor
    {
        public static Dictionary<string,string> GetInstalledSoftwareList()
        {
            string displayName,execPath;
            var gInstalledSoftware = new Dictionary<string,string>();
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    execPath = subkey.GetValue("DisplayIcon") as string;
                    if (string.IsNullOrEmpty(displayName)||string.IsNullOrEmpty(execPath)||!File.Exists(execPath))
                        continue;

                    gInstalledSoftware.Add(displayName,execPath);
                }
            }

            //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                var key = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    execPath = subkey.GetValue("DisplayIcon") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(execPath) || !File.Exists(execPath))
                        continue;
                    continue;

                    gInstalledSoftware.Add(displayName,execPath);
                }
            }

            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var key = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    execPath = subkey.GetValue("DisplayIcon") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(execPath) || !File.Exists(execPath))
                        continue;
                    continue;

                    gInstalledSoftware.Add(displayName,execPath);
                }
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false))
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    execPath = subkey.GetValue("DisplayIcon") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(execPath) || !File.Exists(execPath))
                        continue;
                    continue;

                    gInstalledSoftware.Add(displayName,execPath);
                }
            }
            return gInstalledSoftware;
        }

        public static Icon GetIconFromFile(string Path)
        {
            if(string.IsNullOrEmpty(Path)||!File.Exists(Path))
                return null;
            return Icon.ExtractAssociatedIcon(Path);
        }
    }

}
