using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.GroupService
{
    public class Modelstate
    {
        [JsonProperty(PropertyName = "group.GroupName")]
        public List<string> GroupName { get; set; }
        [JsonProperty(PropertyName = "group.CustomerUID")]
        public List<string> CustomerUID { get; set; }
        [JsonProperty(PropertyName = "group.UserUID")]
        public List<string> UserUID { get; set; }
        [JsonProperty(PropertyName = "group.AssetUID")]
        public List<string> AssetUID { get; set; }
        [JsonProperty(PropertyName = "group.GroupUID")]
        public List<string> GroupUID { get; set; }
        [JsonProperty(PropertyName = "group.ActionUTC")]
        public List<string> ActionUTC { get; set; }
    }

    public class ErrorResponseModel
    {
        public string Message { get; set; }
        public Modelstate ModelState { get; set; }
    }
}
