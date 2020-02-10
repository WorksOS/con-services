using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.FuelBurntRate
{
    public class FuelBurntRateResponse
    {
        public List<AssetFuelBurnRateSetting> assetFuelBurnRateSettings { get; set; }
        public List<Error> errors { get; set; }
    }
    public class AssetFuelBurnRateSetting
    {
        public double idleTargetValue { get; set; }
        public double workTargetValue { get; set; }
        public DateTime startDate { get; set; }
        public string assetUid { get; set; }
    }

    public class Error
    {
        public string assetUid { get; set; }
        public int errorCode { get; set; }
        public string message { get; set; }
    }


}

    