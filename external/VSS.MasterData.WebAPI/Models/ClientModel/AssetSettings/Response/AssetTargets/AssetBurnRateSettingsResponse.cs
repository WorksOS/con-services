using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetFuelBurnRateSettingsResponse : BaseResponse<AssetFuelBurnRateSettingsDetails, AssetErrorInfo>
    {
        public AssetFuelBurnRateSettingsResponse(IList<AssetFuelBurnRateSettingsDetails> lists, IList<AssetErrorInfo> errors) : base(lists) { this.Errors = errors; }

        public AssetFuelBurnRateSettingsResponse(IList<AssetErrorInfo> errors): base(errors) { }

        public AssetFuelBurnRateSettingsResponse(AssetErrorInfo error) : base(error) { }

        public AssetFuelBurnRateSettingsResponse(IList<AssetFuelBurnRateSettingsDetails> lists) : base(lists) { }

        [JsonProperty("assetFuelBurnRateSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetFuelBurnRateSettingsDetails> Lists { get; set; }
    }
}
