using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
    public class DeviceParameterAttribute
    {
        public string GroupName { get; set; }
        public string ParameterName { get; set; }
        public long DeviceTypeParameterID { get; set; }
        public long DeviceParamAttrID { get; set; }
        public string AttributeName { get; set; }
        public string DeviceTypeName { get; set; }
    }
}
