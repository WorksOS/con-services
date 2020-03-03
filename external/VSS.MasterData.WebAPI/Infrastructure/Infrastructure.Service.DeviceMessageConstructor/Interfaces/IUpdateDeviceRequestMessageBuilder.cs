using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;
using DbModel.DeviceConfig;
using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface IUpdateDeviceRequestMessageBuilder
    {
        Guid GetUpdateRequestForDeviceType(DeviceDetails deviceDetails,string deviceTypeFamily, IDictionary<string, string> capabilitiesRecord, 
                                            ref IList<DeviceACKMessage> message, ref List<object> kafkaObjects);
    }
}
