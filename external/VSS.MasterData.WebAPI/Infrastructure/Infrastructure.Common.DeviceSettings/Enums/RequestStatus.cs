using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.DeviceSettings.Enums
{
    public enum RequestStatus
    {
        Failed = 0,
        Pending = 1,
        Acknowledged = 2,
        Completed = 3
    }
}
