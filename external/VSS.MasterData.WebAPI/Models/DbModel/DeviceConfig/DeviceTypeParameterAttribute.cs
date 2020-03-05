using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class DeviceTypeParameterAttribute
    {
        public long DeviceConfigID { get; set; }
        public DateTime? LastAttrEventUTC { get; set; }
        public DateTime? FutureAttrEventUTC { get; set; }
        public ulong DeviceTypeParameterID { get; set; }
        public ulong DeviceParamAttrID { get; set; }
    }
}
