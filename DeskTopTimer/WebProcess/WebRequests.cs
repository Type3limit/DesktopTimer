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
using DeskTopTimer.WebProcess;

namespace DeskTopTimer
{
    public class WebRequestsTool
    {
        public const string seseUrlLevel1 = @"https://iw233.cn/API/Random.php";
        //public const string seseUrlLevel3= @"https://iw233.cn/API/ghs.php";
        //public const string seseUrlLevel2 = @"https://iw233.cn/API/cos.php";

        //public const string mcUrl= @"https://acg.xydwz.cn/mcapi/mcapi.php";

        public const string toubieUrl = @"https://acg.toubiec.cn/random.php";
        //public const string pixivGetUrl = @"https://api.lolicon.app/setu/v2?size=original&size=regular";
        //public const string pixivPostUrl = @"https://api.lolicon.app/setu/v2";
        public const string BackgroundUrl = @"https://api.sunweihu.com/api/sjbz/api.php?lx=dongman";
        public const string paugramUrl = @"https://api.paugram.com/wallpaper";
        public const string dmoeUrl = @"http://www.dmoe.cc/random.php";
        public const string yimianUrl = @"https://api.yimian.xyz/img";
        public const string wallhavenUrl = @"https://wallhaven.cc/api/v1/search";




        public async Task<List<string?>?> RequestSeSePic(string url, string DownloadPath, string FileName)
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

                var Dres = await url.DownloadFileAsync(DownloadPath, FileName + $".{ex}");
                if (!File.Exists(Dres))
                    return null;

                return new List<string?>() { Dres };
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Get方式获取的p站涩图
        /// </summary>
        /// <param name="url"></param>
        /// <param name="DownloadPath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public async Task<PixivSeSe?> RequestGetModePixivSeSePic(string url, string DownloadPath, string FileName)
        {
            try
            {
                var res = await url.GetAsync();
                var JsonRes = await res.GetJsonAsync<PixivResponse>();
                //默认是original
                if (JsonRes == null || !string.IsNullOrEmpty(JsonRes.error))
                {
                    Trace.WriteLine($"无法获取到涩涩{JsonRes?.error}");
                    return null;
                }
                JsonRes?.data?.ForEach(async o =>
                {
                    if(o==null)
                        return;
                    foreach (var itr in o.urls)
                    {
                        var FileFullName = itr.Value + "." + o.ext;
                        var currentFile = await itr.Value.DownloadFileAsync(DownloadPath, FileFullName);
                        if (File.Exists(currentFile))
                            o.urls[itr.Key] = currentFile;
                    }
                });
                return JsonRes?.data?.FirstOrDefault();

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// get方式获取wallHaven涩图
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<WallhavenResponse?> RequestWallHavenPic(WallhavenRequestQuery query)
        {
            if(query==null)
                return null;
            var curQuery =  query.ToQuery();
            var requestUrl = wallhavenUrl+curQuery;
            var res = await requestUrl.GetAsync();
            var jsonRes = await res.GetJsonAsync<WallhavenResponse?>();
            if(jsonRes==null)
                return null;
            return jsonRes;
        }

        public async Task<List<string?>?> RequestWithJsonConfigure(ApiJsonConfigure curConfigure, string DownloadPath, string FileName)
        {
            if (curConfigure == null || string.IsNullOrEmpty(curConfigure.ApiUrl))
                return null;
            try
            {
                Task<List<string?>?>? resTask = null;
                switch (curConfigure.Method?.ToLower())
                {
                    case "get":
                        resTask = RequestWithGetMethod(curConfigure, DownloadPath, FileName);
                        break;
                    case "post":
                        resTask = RequestWithPostMethod(curConfigure, curConfigure.Content, DownloadPath, FileName);
                        break;
                    default:
                        resTask = RequestSeSePic(curConfigure.ApiUrl, DownloadPath, FileName);
                        break;
                }

                return await resTask;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("在解析配置时发生错误" + ex);
                return null;
            }
        }


        public async Task<List<string?>?> RequestWithGetMethod(ApiJsonConfigure configure, string DownloadPath, string FileName)
        {
            try
            {
                var requestUrl = configure.ApiUrl;
                IFlurlRequest? request = null;
                if (configure.Headers != null)
                {
                    foreach (var header in configure.Headers)
                    {
                        request = request == null ? requestUrl.WithHeader(header.Key, header.Value) : request.WithHeader(header.Key, header.Value);
                    }
                }
                IFlurlResponse res = request == null ? await requestUrl.GetAsync() : await request.GetAsync();

                if (res == null)
                    return null;
                var jsonContent = await res.GetJsonAsync();

                var JsonOb = jsonContent==null?null:JObject.Parse(jsonContent.ToString());

                if (JsonOb == null)
                    return null;
                //read response key
                List<JToken?> resourceValue = new List<JToken?>();
                if (configure.ResourcesKeys != null && configure.ResourcesKeys.Count > 0)
                {
                    foreach (var itr in configure.ResourcesKeys)
                    {
                        resourceValue.AddRange(JsonOb.SelectTokens(itr));
                    }
                }
                if (resourceValue == null || resourceValue.Count <= 0)
                    return null;

                List<string?> resultLists = new List<string?>();
                int count = 0;
                resourceValue.ForEach(async x =>
                {
                    if(x==null)
                        return;
                    if (resourceValue.Count > 1)
                        FileName += $"[{count++}]";

                    var res = await RequestSeSePic(x.ToString(), DownloadPath, FileName);
                    if (!string.IsNullOrEmpty(res?.FirstOrDefault()))
                        resultLists.Add(res?.FirstOrDefault());
                });

                return resultLists;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Get请求时发生错误" + ex);
                return null;
            }
        }

        public async Task<List<string?>?> RequestWithPostMethod(ApiJsonConfigure configure, string? content, string DownloadPath, string FileName)
        {
            try
            {
                var requestUrl = configure.ApiUrl;
                IFlurlRequest? request = null;
                if (configure.Headers != null)
                {
                    foreach (var header in configure.Headers)
                    {
                        request = request == null ? requestUrl.WithHeader(header.Key, header.Value) : request.WithHeader(header.Key, header.Value);
                    }
                }
                IFlurlResponse res = request == null ? await requestUrl.PostJsonAsync(content) : await request.PostJsonAsync(content);

                if (res == null)
                    return null;
                var jsonContent = await res.GetJsonAsync();

                var JsonOb = jsonContent ==null?null:JObject.Parse(jsonContent.ToString());

                if (JsonOb == null)
                    return null;
                //read response key
                List<JToken?> resourceValue = new List<JToken?>();
                if (configure.ResourcesKeys != null && configure.ResourcesKeys.Count > 0)
                {
                    foreach (var itr in configure.ResourcesKeys)
                    {
                        resourceValue.AddRange(JsonOb.SelectTokens(itr));
                    }
                }
                if (resourceValue == null || resourceValue.Count <= 0)
                    return null;

                List<string?> resultLists = new List<string?>();
                int count = 0;
                resourceValue.ForEach(async x =>
                {
                    if (x == null)
                        return;
                    if (resourceValue.Count > 1)
                        FileName += $"[{count++}]";

                    var res = await RequestSeSePic(x.ToString(), DownloadPath, FileName);
                    if (!string.IsNullOrEmpty(res?.FirstOrDefault()))
                        resultLists.Add(res?.FirstOrDefault());
                });

                return resultLists;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Get请求时发生错误" + ex);
                return null;
            }
        }
    }
}
