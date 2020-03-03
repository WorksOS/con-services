using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService
{
    #region Valid DeviceServiceCreateRequest

    public class CreateDeviceModel
    {
        public Guid DeviceUID { get; set; }
        public string DeviceSerialNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceState { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeregisteredUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ModuleType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MainboardSoftwareVersion { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RadioFirmwarePartNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GatewayFirmwarePartNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DataLinkType { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }

    #endregion

    #region Valid DeviceAssetAssociationRequest

    public class DeviceAssetAssociationModel
    {
        public Guid DeviceUID { get; set; }
        public Guid AssetUID { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }

    #endregion

    #region Valid DeviceAssetAssociationRequest

    public class DeviceAssetDissociationModel
    {
        public Guid DeviceUID { get; set; }
        public Guid AssetUID { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }

    #endregion
}
