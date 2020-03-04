using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class DeviceParamGroupDto
    {
        public string TypeName { get; set; }
        public string Name { get; set; }
        public ulong Id { get; set; }
        public bool IsMultiDeviceTypeSupport { get; set; }
        public bool IsDeviceParamGroup { get; set; }
    }
}
