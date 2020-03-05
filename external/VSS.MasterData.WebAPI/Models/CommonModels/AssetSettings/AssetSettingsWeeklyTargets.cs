using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
    public class AssetSettingsWeeklyTargets : AssetSettingsBase
    {
        /// <summary>
        /// The asset target runtime hours for the week.
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "runtime", NullValueHandling = NullValueHandling.Ignore)]
        public WeekDays Runtime { get; set; }
        /// <summary>
        /// The asset target idle hours for the week
        /// </summary>
        [JsonProperty(Required = Required.Always, PropertyName = "idle", NullValueHandling = NullValueHandling.Ignore)]
        public WeekDays Idle { get; set; }
    }
}
