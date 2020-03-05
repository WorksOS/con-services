using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSettings
{
    public class GetAssetSettingsRequestModel
    {
        public string FilterName { get; set; }
        public string FilterValue { get; set; }
        public string SortColumn { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Guid CustomerUid { get; set; }
        public Guid UserUid { get; set; }
    }

    public class AssociateAssetDevice
    {
        public Guid DeviceUID { get; set; }
        public Guid AssetUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }

    public class AssociateAssetCustomer
    {
        public Guid CustomerUID { get; set; }
        public Guid AssetUID { get; set; }
        public string RelationType { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }


    public class CreateAssetSettings
    {
        public List<AssetTargetSetting> assetTargetSettings { get; set; }
        public string UserUID { get; set; }
        public string CustomerUID { get; set; }

    }
    public class Runtime
    {
        public int sunday { get; set; }
        public int monday { get; set; }
        public int tuesday { get; set; }
        public int wednesday { get; set; }
        public int thursday { get; set; }
        public int friday { get; set; }
        public int saturday { get; set; }
    }

    public class Idle
    {
        public int sunday { get; set; }
        public int monday { get; set; }
        public int tuesday { get; set; }
        public int wednesday { get; set; }
        public int thursday { get; set; }
        public int friday { get; set; }
        public int saturday { get; set; }
        public int Sunday { get; internal set; }
    }

    public class AssetTargetSetting
    {
        public Runtime runtime { get; set; }
        public Idle idle { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Guid assetUid { get; set; }
    }


}
