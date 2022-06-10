using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeskTopTimer
{
    public class SearchResult : ObservableObject
    {
        public bool HasResult { set; get; }
        public string? ErrorDescribe { set; get; }
        public string? Name { set; get; }
        public string? FullPath { set; get; }
        public DateTime? LastModified { set; get; }
        public long? FileSize { set; get; }
        public bool IsFolder { set; get; }

        public ImageSource? FileThumb { set; get; }

        private ICommand? _runCurrentProcess = null;
        public ICommand? RunCurrentProcess
        {
            get => _runCurrentProcess ?? (_runCurrentProcess = new RelayCommand<string?>((arg) =>
               {
                   try
                   {

                       if (string.IsNullOrEmpty(FullPath))
                           return;
                       Process.Start("Explorer.exe", FullPath);
                   }
                   catch (Exception ex)
                   {
                       Trace.WriteLine(ex);
                   }

               }));
        }
        private ICommand? _openInExplorer = null;
        public ICommand? OpenInExplorer
        {
            get => _openInExplorer ?? (_openInExplorer = new RelayCommand(() =>
                {
                    try
                    {

                        if (string.IsNullOrEmpty(FullPath))
                            return;
                        if (IsFolder)
                        {
                            RunCurrentProcess?.Execute(null);
                            return;
                        }
                        else
                        {
                            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                            psi.Arguments = "/e,/select," + FullPath;
                            System.Diagnostics.Process.Start(psi);
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }

                }));
        }

    }


    public class EverythingApi
    {
        #region everything defination
        const int EVERYTHING_OK = 0;
        const int EVERYTHING_ERROR_MEMORY = 1;
        const int EVERYTHING_ERROR_IPC = 2;
        const int EVERYTHING_ERROR_REGISTERCLASSEX = 3;
        const int EVERYTHING_ERROR_CREATEWINDOW = 4;
        const int EVERYTHING_ERROR_CREATETHREAD = 5;
        const int EVERYTHING_ERROR_INVALIDINDEX = 6;
        const int EVERYTHING_ERROR_INVALIDCALL = 7;

        const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
        const int EVERYTHING_REQUEST_PATH = 0x00000002;
        const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;
        const int EVERYTHING_REQUEST_EXTENSION = 0x00000008;
        const int EVERYTHING_REQUEST_SIZE = 0x00000010;
        const int EVERYTHING_REQUEST_DATE_CREATED = 0x00000020;
        const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
        const int EVERYTHING_REQUEST_DATE_ACCESSED = 0x00000080;
        const int EVERYTHING_REQUEST_ATTRIBUTES = 0x00000100;
        const int EVERYTHING_REQUEST_FILE_LIST_FILE_NAME = 0x00000200;
        const int EVERYTHING_REQUEST_RUN_COUNT = 0x00000400;
        const int EVERYTHING_REQUEST_DATE_RUN = 0x00000800;
        const int EVERYTHING_REQUEST_DATE_RECENTLY_CHANGED = 0x00001000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_PATH = 0x00004000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000;

        const int EVERYTHING_SORT_NAME_ASCENDING = 1;
        const int EVERYTHING_SORT_NAME_DESCENDING = 2;
        const int EVERYTHING_SORT_PATH_ASCENDING = 3;
        const int EVERYTHING_SORT_PATH_DESCENDING = 4;
        const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
        const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
        const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
        const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
        const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
        const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
        const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
        const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
        const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
        const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
        const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
        const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
        const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
        const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
        const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
        const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
        const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
        const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;

        const int EVERYTHING_TARGET_MACHINE_X86 = 1;
        const int EVERYTHING_TARGET_MACHINE_X64 = 2;
        const int EVERYTHING_TARGET_MACHINE_ARM = 3;

        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern UInt32 Everything_SetSearchW(string lpSearchString);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetMatchPath(bool bEnable);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetMatchCase(bool bEnable);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetRegex(bool bEnable);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetMax(UInt32 dwMax);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetOffset(UInt32 dwOffset);

        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetMatchPath();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetMatchCase();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetMatchWholeWord();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetRegex();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetMax();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetOffset();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern IntPtr Everything_GetSearchW();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetLastError();

        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_QueryW(bool bWait);

        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SortResultsByPath();

        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetNumFileResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetNumFolderResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetNumResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetTotFileResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetTotFolderResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetTotResults();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_IsVolumeResult(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_IsFolderResult(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_IsFileResult(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathNameW(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_Reset();

        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileName(UInt32 nIndex);

        // Everything 1.4
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetSort(UInt32 dwSortType);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetSort();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetResultListSort();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern void Everything_SetRequestFlags(UInt32 dwRequestFlags);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetRequestFlags();
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetResultListRequestFlags();
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultExtension(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultSize(UInt32 nIndex, out long lpFileSize);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultDateCreated(UInt32 nIndex, out long lpFileTime);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultDateModified(UInt32 nIndex, out long lpFileTime);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultDateAccessed(UInt32 nIndex, out long lpFileTime);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetResultAttributes(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileListFileName(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetResultRunCount(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultDateRun(UInt32 nIndex, out long lpFileTime);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_GetResultDateRecentlyChanged(UInt32 nIndex, out long lpFileTime);
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileName(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPath(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(UInt32 nIndex);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_GetRunCountFromFileName(string lpFileName);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern bool Everything_SetRunCountFromFileName(string lpFileName, UInt32 dwRunCount);
        [DllImport(".\\Resources\\Everything64.dll")]
        public static extern UInt32 Everything_IncRunCountFromFileName(string lpFileName);

        #endregion

        /// <summary>
        /// 搜索特定关键字
        /// </summary>
        /// <param name="SearchKey"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SearchResult>> SearchFile(string SearchKey, uint SortOrder = EVERYTHING_SORT_DATE_ACCESSED_ASCENDING, uint maxSearchCount = 100)
        {
            return await Task.Run(async () =>
            {
                List<SearchResult> results = new List<SearchResult>();

                Func<uint, SearchResult> ErrorStep = new Func<uint, SearchResult>((res) =>
                  {
                      var cur = new SearchResult();
                      cur.Name = "错误的搜索结果";
                      cur.HasResult = false;
                      cur.ErrorDescribe = GetErrorDescribe((int)res);
                      return cur;
                  });

                var res = Everything_SetSearchW(SearchKey);
                if (res != 0)
                {
                    var cur = ErrorStep(res);
                    results.Add(cur);
                    return results;
                }

                Everything_SetRequestFlags(EVERYTHING_REQUEST_FILE_NAME | EVERYTHING_REQUEST_PATH | EVERYTHING_REQUEST_DATE_RUN | EVERYTHING_REQUEST_SIZE);

                Everything_SetSort(SortOrder);


                Everything_QueryW(true);
                var ResCount = Everything_GetNumResults();


                for (UInt32 i = 0; i < ResCount && i < maxSearchCount; i++)
                {
                    var currentSearch = new SearchResult();
                    currentSearch.HasResult = true;
                    currentSearch.ErrorDescribe = "";
                    currentSearch.Name = Marshal.PtrToStringUni(Everything_GetResultFileName(i));
                    if(currentSearch.Name!=null&&currentSearch.Name.StartsWith("~$"))//回收站内容忽略
                        continue;
                    long date_modified;
                    long size;

                    Everything_GetResultDateModified(i, out date_modified);
                    Everything_GetResultSize(i, out size);
                    currentSearch.FileSize = size;
                    currentSearch.IsFolder = Everything_IsFolderResult(i);
                    //currentSearch.LastModified = DateTime.FromFileTime(date_modified);
                    var curBuidler = new StringBuilder(260);
                    Everything_GetResultFullPathNameW(i, curBuidler, 260);
                    currentSearch.FullPath = curBuidler.ToString();
                    //currentSearch.FileThumb = ImageTool.ImageSourceFromBitmap(ImageTool.GetThumbnailByPath(currentSearch.FullPath));
                    System.Drawing.Icon? curIcon = null;
                    if (currentSearch.IsFolder)
                    {
                        curIcon = ImageTool.GetDirectoryIcon(currentSearch.FullPath, true);
                    }
                    else
                    {
                        curIcon = ImageTool.GetFileIcon(currentSearch.FullPath, true);
                    }
                    
                    using (MemoryStream ms = new MemoryStream())
                    {
                        curIcon?.ToBitmap()?.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.Freeze();
                        currentSearch.FileThumb = bi;
                    }


                    results.Add(currentSearch);
                }

                Everything_Reset();
                return results;
            });
        }


        public static void ResetSearchStatus()
        {
            Everything_Reset();
        }



        /// <summary>
        /// 获取错误码描述
        /// </summary>
        /// <param name="currentErrorCode"></param>
        /// <returns></returns>
        public static string GetErrorDescribe(int currentErrorCode) =>
          currentErrorCode switch
          {
              EVERYTHING_OK => "正常",
              EVERYTHING_ERROR_MEMORY => "内存问题导致失败",
              EVERYTHING_ERROR_IPC => "进程通信导致失败",
              EVERYTHING_ERROR_REGISTERCLASSEX => "注册失败",
              EVERYTHING_ERROR_CREATEWINDOW => "窗体创建失败",
              EVERYTHING_ERROR_CREATETHREAD => "线程创建失败",
              EVERYTHING_ERROR_INVALIDINDEX => "错误的序号",
              EVERYTHING_ERROR_INVALIDCALL => "错误的调用",
              _ => $"未知错误{currentErrorCode}"
          };


    }
}
