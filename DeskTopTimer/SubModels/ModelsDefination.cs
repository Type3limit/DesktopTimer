using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DeskTopTimer.SubModels
{
    internal class ModelsDefination
    {
        /// <summary>
        /// submodels with enabled status
        /// </summary>
        [JsonIgnore]
        Dictionary<SubModelBase,bool> subModels = new Dictionary<SubModelBase,bool>();
        [JsonProperty("SubModels")]
        public Dictionary<SubModelBase,bool> SubModels
        {
            get=> subModels;
            set=> subModels = value;
        }

    }
}
