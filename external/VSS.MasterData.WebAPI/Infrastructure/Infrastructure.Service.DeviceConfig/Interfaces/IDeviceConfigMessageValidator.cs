using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigMessageValidator
    {
        bool ValidateDeviceConfigMessage(DateTime messageEventUTC, DateTime? lastAttrEventUTC);
    }
}
