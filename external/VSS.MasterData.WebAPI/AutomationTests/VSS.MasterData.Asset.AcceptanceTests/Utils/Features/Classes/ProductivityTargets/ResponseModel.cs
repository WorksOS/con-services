using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.ProductivityTargets
{
    public class ProductivityTargetsResponseModel
    {
        public List<string> assetUID { get; set; }

         [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Error error { get; set; }

    }

   public class RetrieveProductivityDetails
    {
        public List<Targets> productivitytargets { get; set; }
    }

    public class Error
    {
        public int status { get; set; }
        public int errorCode { get; set; }
        public string message { get; set; }
    }

 

}
