using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
    public interface IDeviceEvent
    {
        Guid DeviceUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}
