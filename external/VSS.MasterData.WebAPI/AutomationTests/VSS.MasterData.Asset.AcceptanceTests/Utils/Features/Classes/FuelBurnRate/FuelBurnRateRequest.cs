using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.FuelBurnRate
{
   public class FuelBurnRateRequest
    {
        public List<string> assetUIds { get; set; }
        public double IdleTargetValue { get; set; }
        public int workTargetValue { get; set; }
    }
}
