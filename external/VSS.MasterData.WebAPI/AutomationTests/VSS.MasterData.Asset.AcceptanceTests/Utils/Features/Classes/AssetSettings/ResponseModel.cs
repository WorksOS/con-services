using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings
{

    public class GetAssetSettingsResponseModel
    {
        public List<AssetSetting> assetSettings { get; set; }
        public PageInfo pageInfo { get; set; }
    }
    public class AssetSetting
    {
        public Guid assetUid { get; set; }
        public string assetId { get; set; }
        public string assetSerialNumber { get; set; }
        public string assetModel { get; set; }
        public string assetMakeCode { get; set; }
        public int assetIconKey { get; set; }
        public string deviceSerialNumber { get; set; }
        public bool targetStatus { get; set; }
    }

    public class PageInfo
    {
        public int totalRecords { get; set; }
        public int totalPages { get; set; }
        public int currentPageNumber { get; set; }
        public int currentPageSize { get; set; }
        public int recordsCount { get; set; }
    }
    public class CreateAssetSettingsResponse
    {
        public List<string> assetTargets { get; set; }
        public List<string> assetUid { get; set; }
        public List<Error> errors { get; set; }
    }


    public class Error
    {
        public string assetUid { get; set; }
        public int errorCode { get; set; }
        public string message { get; set; }
    }
    public class RetrieveAssetSettingsResponse
    {
        public List<AssetTargetSetting> assetTargetSettings { get; set; }
        public List<Error> errors { get; set; }
    }

}
