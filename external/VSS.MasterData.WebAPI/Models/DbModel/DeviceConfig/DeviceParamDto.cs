using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class DeviceParamDto
    {
        public string TypeName { get; set; }
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ulong ParameterGroupId { get; set; }
        public string ParameterGroupName { get; set; }
        public ulong AttributeId { get; set; }
        public string AttributeName { get; set; }
    }
}
