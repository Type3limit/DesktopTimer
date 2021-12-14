using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Flurl;
using Flurl.Http;
namespace DeskTopTimer
{
    public class WebRequestsTool
    {
        public const string seseUrlLevel1 = @"https://iw233.cn/API/Random.php";
        public const string seseUrlLevel3= @"https://iw233.cn/API/ghs.php";
        public const string seseUrlLevel2 = @"https://iw233.cn/API/cos.php";

        public const string mcUrl= @"https://acg.xydwz.cn/mcapi/mcapi.php";

        public const string toubieUrl = @"https://acg.toubiec.cn/random.php";
        public const string pixivGetUrl = @"https://api.lolicon.app/setu/v2?size=original&size=regular";
        public const string pixivPostUrl = @"https://api.lolicon.app/setu/v2";

        public async Task<string> RequestSeSePic(string url,string DownloadPath,string FileName)
        {
            try
            {
                var res = await url.GetAsync();
                if (res == null)
                    return null;

                var type = res.Headers.Where(x => x.Name.ToLower() == "content-type").FirstOrDefault().Value;
                if (type == null)
                    return null;
                var ex = type.Split('/').Last();
                
                var Dres = await url.DownloadFileAsync(DownloadPath, FileName+$".{ex}");
                if(!File.Exists(Dres))
                    return null;
                
                return Dres;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        public class PixivSeSe
        {
            /// <summary>
            /// 作品 pid
            /// </summary>
            public int pid { set;get;}
            /// <summary>
            /// 作品所在页
            /// </summary>
            public int p { set;get;}
            /// <summary>
            /// 作者 uid
            /// </summary>
            public int uid { set;get;}
            /// <summary>
            /// 作品标题
            /// </summary>
            public string title { set;get;}
            /// <summary>
            /// 作者名（入库时，并过滤掉 @ 及其后内容）
            /// </summary>
            public string author { set;get;}
            /// <summary>
            /// 是否 R18（在库中的分类，不等同于作品本身的 R18 标识）
            /// </summary>
            public bool r18 { set;get;}
            /// <summary>
            /// 原图宽度 px
            /// </summary>
            public int width { set;get;}
            /// <summary>
            /// 原图高度 px
            /// </summary>
            public int height { set;get;}
            /// <summary>
            /// 作品标签，包含标签的中文翻译（有的话）
            /// </summary>
            public List<string> tags { set;get;}

            /// <summary>
            /// 图片扩展名
            /// </summary>
            public string ext { set;get;}
            /// <summary>
            /// 作品上传日期；时间戳，单位为毫秒
            /// </summary>
            public ulong uploadDate { set;get;}
            /// <summary>
            /// 包含了所有指定size的图片地址
            /// </summary>
            public Dictionary<string,string> urls { set;get;}

        }


        public class PixivResponse
        {
            public string error { set;get;}
            public List<PixivSeSe> data { set;get;}
        }

        /// <summary>
        /// Get方式获取的p站涩图
        /// </summary>
        /// <param name="url"></param>
        /// <param name="DownloadPath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public async Task<PixivSeSe> RequestGetModePixivSeSePic(string url,string DownloadPath,string FileName)
        {
            try
            {
                var res = await url.GetAsync();
                var JsonRes = await res.GetJsonAsync<PixivResponse>();
                //默认是original
                if(JsonRes==null||!string.IsNullOrEmpty(JsonRes.error))
                {
                    Trace.WriteLine($"无法获取到涩涩{JsonRes?.error}");
                    return null;
                }
                JsonRes.data.ForEach(async o => 
                {
                    foreach (var itr in o.urls)
                    {
                        var FileFullName = itr.Value+"."+o.ext;
                       var currentFile = await itr.Value.DownloadFileAsync(DownloadPath, FileFullName);
                        if(File.Exists(currentFile))
                            o.urls[itr.Key] = currentFile;
                    }
                });
                return JsonRes.data[0];

            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        //public async Task<string> RequestPostModePixivSeSePic(string url,string DownloadPath,string FileName)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex);
        //        return null;
        //    }
        //}
    }
}
