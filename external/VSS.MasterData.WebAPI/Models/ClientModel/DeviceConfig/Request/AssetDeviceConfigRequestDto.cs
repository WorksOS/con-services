using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request
{
    public class AssetDeviceConfigRequestDto
    {
        public IEnumerable<Guid> DeviceUIDs { get; set; }
        public string GroupName { get; set; }
        public string ParameterName { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public DateTime ActionUTC { get; set; }
        public string DeviceType { get; set; }
    }
}
