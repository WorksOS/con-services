using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class DeviceTransferEvent : IDeviceEvent
    {
        public Guid DeviceUID { get; set; }
        public Guid OldAssetUID { get; set; }
        public Guid NewAssetUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
