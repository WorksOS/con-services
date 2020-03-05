using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.DeviceTypeGroupParameterAttribute
{
    public class DeviceTypeGroupParameterAttributeRequest : DeviceConfigRequestBase
    {
        public string GroupName { get; set; }
    }
}
