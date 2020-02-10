using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
    public class ProductivityWeeklyTargetValues : AssetSettingsBase
    {
        /// <summary>
        /// The asset target runtime hours for the week.
        /// </summary>
        [JsonProperty("cycles")]
        public WeekDays Cycles { get; set; }
        /// <summary>
        /// The asset target idle hours for the week
        /// </summary>
        [JsonProperty("volumes")]
        public WeekDays Volumes { get; set; }

        [JsonProperty("payload")]
        public WeekDays Payload { get; set; }
    }
}
