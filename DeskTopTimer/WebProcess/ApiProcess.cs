using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskTopTimer.WebProcess
{
    /// <summary>
    /// 用于配置额外的拓展api
    /// </summary>
    public class ApiJsonConfigure
    {
        /// <summary>
        /// 发起请求的目标url(唯一Key)
        /// </summary>
        [JsonProperty("apiUrl")]
        public string? ApiUrl { set;get;}
        /// <summary>
        /// 请求的执行方法
        /// </summary>
        [JsonProperty("method")]
        public string? Method { set;get;}
        /// <summary>
        /// 请求体(如果需要)
        /// </summary>
        [JsonProperty("content")]
        public string? Content { set;get;}
        /// <summary>
        /// 请求头(如果需要)
        /// </summary>
        [JsonProperty("headers")]
        public Dictionary<string, string>? Headers { set;get;}

        
        /// <summary>
        /// 具体资源的对应列表的 JsonPath key(可为空)
        /// </summary>
        [JsonProperty("resKeys")]
        public List<string>? ResourcesKeys { set;get;}
    }

}
