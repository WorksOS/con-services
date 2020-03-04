using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class DeviceTypeGroupParamAttrDto
    {
        public ulong AttributeID { get; set; }
        public string AttributeName { get; set; }
        public ulong DeviceParameterID { get; set; }
        public string ParameterName { get; set; }
        public ulong DeviceParamGroupID { get; set; }
        public string GroupName { get; set; }
        public string TypeName { get; set; }
        public string DefaultValueJSON { get; set; }
        public ulong DeviceTypeParameterID { get; set; }
		public bool IncludeInd { get; set; }
		public ulong DeviceParamAttrID { get; set; }
    }
}
