using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.FuelBurntRate
{
   public class FuelBurntRateRequest
    {
        public List<string> assetUIds { get; set; }
        public double idleTargetValue { get; set; }
        public double workTargetValue { get; set; }
    }
}
