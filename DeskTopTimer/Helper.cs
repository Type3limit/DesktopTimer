using CefSharp;
using Microsoft.Win32;
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
                Trace.WriteLine(ex.ToString());
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
    /// 图像工具（包含常用转换，获取图标Image等）
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

        public static async Task<BitmapImage?> LoadImg(string imagePath)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(imagePath))
                    return null;
                BitmapImage bi = new BitmapImage();

                // Begin initialization.
                bi.BeginInit();
                bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                // Set properties.
                bi.CacheOption = BitmapCacheOption.OnLoad;

                using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    if (ms.Length <= 0)
                    {
                        bi.EndInit();
                        bi.Freeze();
                        return bi;
                    }
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();
                }
                return bi;
            });
        }
        public static BitmapImage? GetImage(string imagePath)
        {
            try
            {
                BitmapImage? bitmap = null;

                if (imagePath.StartsWith("pack://"))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
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
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
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
                    Trace.WriteLine(ex.ToString());
                    return false;
                }
            });


        }



        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo
        (
            string pszPath, //一个包含要取得信息的文件相对或绝对路径的缓冲。它可以处理长或短文件名。（也就是指定的文件路径）注[1]
            uint dwFileAttributes,//资料上说，这个参数仅用于uFlags中包含SHGFI_USEFILEATTRIBUTES标志的情况(一般不使用)。如此，它应该是文件属性的组合：存档，只读，目录，系统等。
            out SHFILEINFO psfi,
            uint cbfileInfo,//简单地给出上项结构的尺寸。
            SHGFI uFlags//函数的核心变量，通过所有可能的标志，你就能驾驭函数的行为和实际地得到信息。
        );


        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
            }
            public IntPtr hIcon;//图标句柄
            public int iIcon;//系统图标列表的索引
            public uint dwAttributes; //文件的属性
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;//文件的路径等 文件名最长256（ANSI），加上盘符（X:\）3字节，259字节，再加上结束符1字节，共260
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;//文件的类型名 固定80字节
        };



        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,//Retrieve the display name for the file, which is the name as it appears in Windows Explorer. The name is copied to the szDisplayName member of the structure specified in psfi. The returned display name uses the long file name, if there is one, rather than the 8.3 form of the file name. Note that the display name can be affected by settings such as whether extensions are shown.
            Typename = 0x00000400,  //Retrieve the string that describes the file's type. The string is copied to the szTypeName member of the structure specified in psfi.
            SysIconIndex = 0x00004000, //Retrieve the index of a system image list icon. If successful, the index is copied to the iIcon member of psfi. The return value is a handle to the system image list. Only those images whose indices are successfully copied to iIcon are valid. Attempting to access other images in the system image list will result in undefined behavior.
            UseFileAttributes = 0x00000010 //Indicates that the function should not attempt to access the file specified by pszPath. Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes. This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
        }

        /// <summary>
        /// 根据文件扩展名得到系统扩展名的图标
        /// </summary>
        /// <param name="fileName">文件名(如：win.rar;setup.exe;temp.txt)</param>
        /// <param name="largeIcon">图标的大小</param>
        /// <returns></returns>
        public static Icon GetFileIcon(string fileName, bool largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
            else
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
            IntPtr IconIntPtr = SHGetFileInfo(fileName, 256, out info, (uint)cbFileInfo, flags);
            if (IconIntPtr.Equals(IntPtr.Zero))
                return null;
            return Icon.FromHandle(info.hIcon);
        }

        /// <summary>  
        /// 获取文件夹图标
        /// </summary>  
        /// <returns>图标</returns>  
        public static Icon GetDirectoryIcon(string path, bool largeIcon)
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();
            int cbFileInfo = Marshal.SizeOf(_SHFILEINFO);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon;
            else
                flags = SHGFI.Icon | SHGFI.SmallIcon;

            IntPtr IconIntPtr = SHGetFileInfo(path, 256, out _SHFILEINFO, (uint)cbFileInfo, flags);
            if (IconIntPtr.Equals(IntPtr.Zero))
                return null;
            Icon _Icon = Icon.FromHandle(_SHFILEINFO.hIcon);
            return _Icon;
        }





    }

    /// <summary>
    /// 路径定义
    /// </summary>
    public class FileMapper
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
        /// 配置文件目录
        /// </summary>
        public static string ConfigureDir
        {
            get
            {
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration");
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
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
                string currentFile = Path.Combine(ConfigureDir, "Configuration.Json");
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
                string currentFile = Path.Combine(ConfigureDir, "Models.Json");
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
                string currentFile = Path.Combine(ConfigureDir, "Webs.Json");
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
        /// <summary>
        /// 日志文件目录
        /// </summary>
        public static string CurrentLogFileDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Logs";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        private static string currentLogFile = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        /// <summary>
        /// 当前日志文件
        /// </summary>
        public static string CurrentLogFile
        {
            get
            {
                var FileName = Path.Combine(CurrentLogFileDir, currentLogFile + ".log");
                if (!File.Exists(FileName))
                {
                    File.Create(FileName).Close();
                }
                return FileName;
            }
        }
    }

    /// <summary>
    /// 设置类定义
    /// </summary>
    public class Configure
    {
        public bool isOnlineSeSeMode { set; get; }

        public string? localFilePath { set; get; }

        public string? currentSeSeApi { set; get; }

        public double windowWidth { set; get; }

        public double windowHeight { set; get; }

        public double backgroundImgOpacity { set; get; }

        public long maxCacheCount { set; get; }

        public long flushTime { set; get; }

        public bool isTopmost { set; get; } = true;

        public int timeFontIndex { set; get; } = 0;

        public int weekendFontIndex { set; get; } = 0;

        public int timeFontSize { set; get; } = 20;

        public int weekendFontSize { set; get; } = 12;

        public string? timeFontColor { set; get; }

        public string? weekendFontColor { set; get; }

        public bool isUsingVideoBackGround { set; get; } = false;

        public string? videoDir { set; get; }

        public string? selectedVideoPath { set; get; }

        public string? localCollectdPath { set; get; }

        public string? WebSiteUrl { set; get; }

        public bool IsWebViewVisiable { set; get; }

        public bool isLoopPlay { set; get; } = true;

        public double volume { set; get; } = 1d;

        public bool isUsingAudioVisualize { set; get; }

        public AudioVisualizerSetting audioVisualizerSetting { set; get; } = new AudioVisualizerSetting();
    }

    public class AudioVisualizerSetting
    {
        public int drawingRectCount { set; get; } = 100;
        public double drawingRectBorderThickness { set; get; } = 1;
        public double drawingRectRectRadius { set; get; } = 2;
        public bool usingRamdomColor { set; get; } = true;
        public string? drawingRectFillColor { set; get; } = Colors.White.ToString();
        public string? drawingRectStrokeColor { set; get; } = Colors.White.ToString();
        public double visualOpacity { set; get; } = 1;
        public double dataWeight { set; get; } = 0.5;
    }


    /// <summary>
    /// 网络地址记录
    /// </summary>
    public class WebUrlRecords
    {
        public List<string>? webUrls { set; get; }
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

        public static System.Windows.Media.Color HexConverter(string? color, bool UsingAlpha = true)
        {

            if (string.IsNullOrEmpty(color))
                return Colors.White;
            if (UsingAlpha)
            {
                var currentArray = color.Skip(1).ToArray();
                byte a = Convert.ToByte(currentArray[0].ToString() + currentArray[1].ToString(), 16);
                byte r = Convert.ToByte(currentArray[2].ToString() + currentArray[3].ToString(), 16);
                byte g = Convert.ToByte(currentArray[4].ToString() + currentArray[5].ToString(), 16);
                byte b = Convert.ToByte(currentArray[6].ToString() + currentArray[7].ToString(), 16);
                return System.Windows.Media.Color.FromArgb(a, r, g, b);
            }
            else
            {
                var currentArray = color.ToArray();
                byte r = Convert.ToByte(currentArray[0].ToString() + currentArray[1].ToString(), 16);
                byte g = Convert.ToByte(currentArray[2].ToString() + currentArray[3].ToString(), 16);
                byte b = Convert.ToByte(currentArray[4].ToString() + currentArray[5].ToString(), 16);
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
        public string name { set; get; }
        public string value { set; get; }
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
        public static Dictionary<string, string> GetInstalledSoftwareList()
        {
            string displayName, execPath;
            var gInstalledSoftware = new Dictionary<string, string>();
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    execPath = subkey.GetValue("DisplayIcon") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(execPath) || !File.Exists(execPath))
                        continue;

                    gInstalledSoftware.Add(displayName, execPath);
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

                    gInstalledSoftware.Add(displayName, execPath);
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

                    gInstalledSoftware.Add(displayName, execPath);
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

                    gInstalledSoftware.Add(displayName, execPath);
                }
            }
            return gInstalledSoftware;
        }

        public static Icon GetIconFromFile(string Path)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
                return null;
            return Icon.ExtractAssociatedIcon(Path);
        }
    }
    public static class LogHelper
    {
        public static void ClearLogFiles()
        {
            var Dirs = Directory.EnumerateDirectories(FileMapper.CurrentLogFileDir);
            foreach (var itr in Dirs)
            {
                if (!Directory.Exists(itr))
                    continue;
                DirectoryInfo info = new DirectoryInfo(itr);
                if ((info.LastAccessTime - DateTime.Now) > TimeSpan.FromDays(15))
                {
                    info.Delete(true);
                }
            }
        }
    }
}
