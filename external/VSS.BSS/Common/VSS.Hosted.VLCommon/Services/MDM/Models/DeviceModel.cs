using System;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
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
