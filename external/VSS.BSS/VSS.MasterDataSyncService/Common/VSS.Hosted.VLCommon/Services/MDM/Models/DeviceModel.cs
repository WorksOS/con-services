using System;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
    public class CreateDeviceEvent
    {
        public Guid DeviceUID { get; set; }

        public string DeviceSerialNumber { get; set; }

        public string DeviceType { get; set; }

        public string DeviceState { get; set; }

        public DateTime? DeregisteredUTC { get; set; }

        public string ModuleType { get; set; }

        public string MainboardSoftwareVersion { get; set; }

        public string RadioFirmwarePartNumber { get; set; }

        public string GatewayFirmwarePartNumber { get; set; }

        public string DataLinkType { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }

    public class UpdateDeviceEvent
    {
        public Guid DeviceUID { get; set; }

        public string DeviceSerialNumber { get; set; }
     
        public string DeviceType { get; set; }
     
        public string DeviceState { get; set; }
     
        public DateTime? DeregisteredUTC { get; set; }
  
        public string ModuleType { get; set; }
     
        public string MainboardSoftwareVersion { get; set; }
    
        public string RadioFirmwarePartNumber { get; set; }
     
        public string GatewayFirmwarePartNumber { get; set; }

        public string DataLinkType { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }

    public class AssociateDeviceAssetEvent 
    {   
        public Guid DeviceUID { get; set; }
        
        public Guid AssetUID { get; set; }
        
        public DateTime ActionUTC { get; set; }
        
        public DateTime ReceivedUTC { get; set; }
    }

    public class DissociateDeviceAssetEvent
    {
        public Guid DeviceUID { get; set; }
     
        public Guid AssetUID { get; set; }
     
        public DateTime ActionUTC { get; set; }
        
        public DateTime ReceivedUTC { get; set; }
    }

    public class DeviceReplacementEvent
    {
       
        public Guid OldDeviceUID { get; set; }
       
        public Guid NewDeviceUID { get; set; }
       
        public Guid AssetUID { get; set; }
       
        public DateTime ActionUTC { get; set; }
       
        public DateTime ReceivedUTC { get; set; }
    }
}
