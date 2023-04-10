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
using System.Security.Policy;
using System.Web;
using Newtonsoft.Json;

using OpenAI_API;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CefSharp.DevTools.IO;
using System.Dynamic;

namespace DeskTopTimer
{
    public class WebRequestsTool
    {

        #region BackGround Request
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
                    if (o == null)
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
            if (query == null)
                return null;
            var curQuery = query.ToQuery();
            var requestUrl = wallhavenUrl + curQuery;
            var res = await requestUrl.GetAsync();
            var jsonRes = await res.GetJsonAsync<WallhavenResponse?>();
            if (jsonRes == null)
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

                var JsonOb = jsonContent == null ? null : JObject.Parse(jsonContent.ToString());

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

                var JsonOb = jsonContent == null ? null : JObject.Parse(jsonContent.ToString());

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

        #endregion

        #region Translate

        #region Baidu
        public static string BaiduTranslateUrl = "https://fanyi-api.baidu.com/api/trans/vip/translate";
        private static string BaiduAppId = "20221223001506041";
        private static string BaiduSecretKey = "95Aus8uyjisHA750dWA8";
        /// <summary>
        /// 百度中英文翻译
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public async Task<string?> BaiduTranslate(string? src, CancellationTokenSource? canceller)
        {
            try
            {
                if (true == src?.IsNullOrEmpty())
                    return null;
                var url = BaiduTranslateUrl;
                url += "?q=" + HttpUtility.UrlEncode(src);
                var from = "en";
                var to = "zh";
                if (true == src?.ContainChinese())
                {
                    from = "zh";
                    to = "en";
                }
                Random random = new Random();
                string salt = random.Next(100000).ToString();
                string sign = EncryptString(BaiduAppId + src + salt + BaiduSecretKey);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + BaiduAppId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                if (canceller == null)
                    return null;
                var Res = await url.GetAsync(canceller.Token);
                if (Res.ResponseMessage.IsSuccessStatusCode)
                {
                    var ResponStream = await Res.GetStreamAsync();
                    StreamReader curStreamReader = new StreamReader(ResponStream, Encoding.GetEncoding("utf-8"));
                    string retString = curStreamReader.ReadToEnd();
                    curStreamReader.Close();
                    ResponStream.Close();
                    var jsonRes = JObject.Parse(HttpUtility.HtmlDecode(retString));
                    if (jsonRes != null && true == jsonRes?.TryGetValue("trans_result", out var target))
                    {
                        var arrays = JArray.Parse(target.ToString());
                        var firstRes = arrays?.First();
                        if (firstRes != null)
                        {
                            return firstRes?.Value<string>("dst");
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }

        }
        /// <summary>
        /// 百度MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
        #endregion
        #region YouDao
        public static string YoudaoTranslateUrl = "https://openapi.youdao.com/api";
        private static string YouDaoAppId = "4b25e4343d86be13";
        private static string YouDaoSecretKey = "algtmSFeAIkxOVcSZRxXFsQVrU3sLHf0";

        public async Task<string?> YouDaoTranslate(string? src, CancellationTokenSource? canceller)
        {
            try
            {
                if (true == src?.IsNullOrEmpty())
                    return null;
                Dictionary<String, String> dic = new Dictionary<String, String>();
                string url = "https://openapi.youdao.com/api";
                string q = src;
                string appKey = YouDaoAppId;
                string appSecret = YouDaoSecretKey;
                string salt = DateTime.Now.Millisecond.ToString();
                var from = "en";
                var to = "zh-CHS";
                if (true == src?.ContainChinese())
                {
                    from = "zh-CHS";
                    to = "en";
                }
                url += "?from=Auto";
                url += "&to=" + to;
                url += "&signType=v3";
                TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                long millis = (long)ts.TotalMilliseconds;
                string curtime = Convert.ToString(millis / 1000);
                url += "&curtime=" + curtime;
                string signStr = appKey + Truncate(q) + salt + curtime + appSecret;
                string sign = ComputeHash(signStr, SHA256.Create());
                url += "&q=" + System.Web.HttpUtility.UrlEncode(q);
                url += "&appKey=" + appKey;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                if (canceller == null)
                    return null;
                var Res = await url.PostAsync(null, canceller.Token);
                if (Res.ResponseMessage.IsSuccessStatusCode)
                {
                    var ResponStream = await Res.GetStreamAsync();
                    StreamReader curStreamReader = new StreamReader(ResponStream, Encoding.GetEncoding("utf-8"));
                    string retString = curStreamReader.ReadToEnd();
                    curStreamReader.Close();
                    ResponStream.Close();
                    var jsonRes = JObject.Parse(HttpUtility.HtmlDecode(retString));
                    if (jsonRes != null && jsonRes.TryGetValue("translation", out var Target))
                    {
                        var jarray = JArray.FromObject(Target);
                        return jarray?.First()?.ToString();
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }


        }
        protected static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
        protected static string? Truncate(string q)
        {
            if (q == null)
            {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }
        #endregion
        #endregion

        #region doutu

        public const string emojiRequestUrl = "http://www.dbbqb.com/api/search/json?w=";
        public const string emojiImageUrl = "http://image.dbbqb.com/";
        public async IAsyncEnumerable<string> GetEmoji(string keywords, int page, int size,string storePath)
        {
            Trace.WriteLine($"Get emoji with Key:{keywords}");
            var res = await Dbbqb.SearchAsync(keywords, page, size);
            foreach (var info in res)
            {
                var strPath =storePath.PathCombine($"{info.Id}.jpg");
                if(strPath.IsFileExist())
                {
                    if(strPath.IsGif())
                    {
                        var nstrPath = storePath.PathCombine($"{info.Id}.gif");
                        File.Move(strPath,nstrPath);
                        strPath = nstrPath;
                    }
                    yield return strPath;
                    continue;
                }
                using FileStream fs = File.Open(strPath,FileMode.Create);
                Stream stream = await Dbbqb.Client.GetStreamAsync(info.Path);
                stream.CopyTo(fs);     
                fs.Close();
                if(strPath.IsFileExist())
                {
                    if(strPath.IsGif())
                    {
                        var nstrPath = storePath.PathCombine($"{info.Id}.gif");
                        File.Move(strPath,nstrPath);
                        strPath = nstrPath;
                    }
                }
                yield return strPath;
            }

        }


        /// <summary>
        /// 逗逼表情包信息 Funny sticker infomation
        /// </summary>
        public record class DbbqbInfo
        {
            private static readonly Uri BaseUri = new Uri("https://image.dbbqb.com");

            /// <summary>
            /// 构建实例; Construct a new instance
            /// </summary>
            /// <param name="id">ID</param>
            /// <param name="width">宽度; Width</param>
            /// <param name="height">高度; Height</param>
            /// <param name="path">相对地址; Related path</param>
            /// <param name="description">描述信息; Description</param>
            [System.Text.Json.Serialization.JsonConstructor]
            public DbbqbInfo(int id, int width, int height, string path, string? description)
            {
                Id = id;
                Width = width;
                Height = height;
                Path = new Uri(BaseUri, path).ToString();
                Description = description;
            }

            /// <summary>
            /// ID
            /// </summary>
            [JsonPropertyName("id")]
            public int Id { get; }
            /// <summary>
            /// 宽度
            /// </summary>
            [JsonPropertyName("width")]
            public int Width { get; }
            /// <summary>
            /// 高度
            /// </summary>
            [JsonPropertyName("height")]
            public int Height { get; }
            /// <summary>
            /// 表情地址; Sticker address
            /// </summary>
            [JsonPropertyName("path")]
            public string Path { get; }
            /// <summary>
            /// 描述信息
            /// </summary>
            [JsonPropertyName("desc")]
            public string? Description { get; }
        }
        /// <summary>
        /// 逗逼表情包; Funny stickers
        /// </summary>
        public static class Dbbqb
        {
            static Dbbqb()
            {
                Client = new HttpClient();
                Client.BaseAddress = new Uri(BaseUri);
                Client.DefaultRequestHeaders.Add("User-Agent", "Meow");
                Client.DefaultRequestHeaders.Add("web-agent", "web");
            }

            private const string BaseUri = "https://www.dbbqb.com/";
            public static readonly HttpClient Client;

            /// <summary>
            /// 获取表情包; Get stickers
            /// </summary>
            /// <param name="start">起始索引; Start index</param>
            /// <param name="count">结果数量; Result count</param>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> GetAsync(int start, int count)
            {
                string requestUri = $"api/search/json?start={start}&size={count}";
                DbbqbInfo[]? dbbqbInfos = await Client.GetFromJsonAsync<DbbqbInfo[]>(requestUri);
                if (dbbqbInfos == null)
                    return Array.Empty<DbbqbInfo>();
                return dbbqbInfos;
            }

            /// <summary>
            /// 获取表情包; Get stickers (starts from 0)
            /// </summary>
            /// <param name="count">结果数量; Result count</param>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> GetAsync(int count)
            {
                return await GetAsync(0, count);
            }

            /// <summary>
            /// 获取 100 个表情包; Get 100 stickers
            /// </summary>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> GetAsync()
            {
                return await GetAsync(0, 100);
            }

            /// <summary>
            /// 获取 1 个表情包; Get 1 sticker
            /// </summary>
            /// <returns>结果. null 表示无结果; Result. null for no result</returns>
            public static async Task<DbbqbInfo?> GetSingleAsync()
            {
                DbbqbInfo[] gets = await GetAsync(0, 1);
                return gets.Length != 0 ? gets[0] : null;
            }

            /// <summary>
            /// 搜索表情包; Search for stickers
            /// </summary>
            /// <param name="kwd">关键词; Keyword</param>
            /// <param name="start">起始索引; Start index</param>
            /// <param name="count">结果数量; Result count</param>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> SearchAsync(string kwd, int start, int count)
            {
                string queryStr = Uri.EscapeDataString(kwd);
                string requestUri = $"api/search/json?start={start}&size={count}&w={queryStr}";
                DbbqbInfo[]? dbbqbInfos = await Client.GetFromJsonAsync<DbbqbInfo[]>(requestUri);
                if (dbbqbInfos == null)
                    return Array.Empty<DbbqbInfo>();
                return dbbqbInfos;
            }

            /// <summary>
            /// 搜索表情包; Search for stickers (从零开始. starts from 0)
            /// </summary>
            /// <param name="kwd">关键词; Keyword</param>
            /// <param name="count">结果数量; Result count</param>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> SearchAsync(string kwd, int count)
            {
                return await SearchAsync(kwd, 0, count);
            }

            /// <summary>
            /// 搜索表情包; Search for stickers (从零开始, 数量为100. starts from 0 and count for 100)
            /// </summary>
            /// <param name="kwd">关键词; Keyword</param>
            /// <returns>结果. 空表示无结果; Result. Empty for no result</returns>
            public static async Task<DbbqbInfo[]> SearchAsync(string kwd)
            {
                return await SearchAsync(kwd, 0, 100);
            }

            /// <summary>
            /// 搜索 1 个表情包; Search for one sticker
            /// </summary>
            /// <param name="kwd">关键词; Keyword</param>
            /// <returns>结果. null 表示无结果; Result. null for no result</returns>
            public static async Task<DbbqbInfo?> SearchSingleAsync(string kwd)
            {
                DbbqbInfo[] gets = await SearchAsync(kwd, 0, 1);
                return gets.Length != 0 ? gets[0] : null;
            }
        }

        #endregion

        #region openAi
        private OpenAIAPI? openAiAPI = null;
        public void InitOpenAI(string key)
        {
            openAiAPI = new OpenAIAPI(key);
        }

        public async Task<string?> Ask(string question, double randomPercent = 0.1)
        {
            if (openAiAPI == null)
                return null;
            var result = await openAiAPI.Completions.CreateCompletionAsync(question, temperature: randomPercent);
            return result.ToString();
        }

        #endregion
    }
}
