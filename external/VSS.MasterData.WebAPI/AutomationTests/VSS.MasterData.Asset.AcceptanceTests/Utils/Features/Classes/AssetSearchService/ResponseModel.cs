using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSearchService
{
    public class AssetDeviceResponseModel
    {
        public int TotalNumberOfPages { get; set; }
        public int PageNumber { get; set; }
        public List<AssetDevice> AssetDevices { get; set; }
    }

    public class AssetDevice
    {
        public string AssetUID { get; set; }
        public string AssetName { get; set; }
        public string AssetSerialNumber { get; set; }
        public string AssetMakeCode { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string DeviceUID { get; set; }
    }
}
