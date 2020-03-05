using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetDetail
{
    public class AssetDetailErrorResponseModel
    {
        public string Message { get; set; }
        public ModelState ModelState { get; set; }
    }

    public class ModelState
    {
        public List<string> assetUID { get; set; }
        public List<string> deviceUID { get; set; }
    }
}
