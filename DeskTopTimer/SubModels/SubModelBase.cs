using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DeskTopTimer.SubModels
{
    public class SubModelBase: ObservableObject
    {
        [JsonIgnore]
        private string _name = "";
        [JsonProperty("Name")]
        public string Name
        {
            get => _name; 
            set =>SetProperty(ref _name, value);    
        }

        [JsonIgnore]
        private string _description = "";
        [JsonProperty("Description")]
        public string Description
        {
            get => _description;
            set=>SetProperty(ref _description, value);
        }

        [JsonIgnore]
        private Guid _uniqueId = Guid.NewGuid();
        [JsonProperty("UniqueID")]
        public Guid UniqueId
        {
            get => _uniqueId;
            set =>SetProperty(ref _uniqueId, value);
        }

        [JsonIgnore]
        private string _url = "";
        /// <summary>
        /// 从此处获取详细Json配置
        /// </summary>
        [JsonProperty("URL")]
        public string Url
        {
            get=>_url;
            set =>SetProperty(ref _url, value);
        }

        //public abstract bool LoadSubModel(params object[] param);

        //public abstract bool UnloadSubModel(params object[] param);

        //public abstract object StartSubModel(params object[] param);

        //public abstract object EndSubModel(params object[] param);
    }
}
