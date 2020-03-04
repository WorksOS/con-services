using ClientModel.Response;
using CommonModel.AssetSettings;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{

	public class GetAssetWeeklyTargetsResponse : BaseResponse<AssetSettingsBase, AssetErrorInfo>

    {

        public GetAssetWeeklyTargetsResponse(IList<AssetSettingsBase> list) : base((List<AssetSettingsBase>)list) { }

        public GetAssetWeeklyTargetsResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public GetAssetWeeklyTargetsResponse(AssetErrorInfo error) : base(error) { }

        [JsonProperty("assetTargetSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsBase> Lists
        {
            get;
            set;
        }


    }
}
