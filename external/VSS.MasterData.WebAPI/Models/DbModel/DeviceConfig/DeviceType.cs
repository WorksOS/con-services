using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class DeviceType
    {
        public long DeviceTypeID { get; set; }
        public string TypeName { get; set; }
        public int fk_DeviceTypeFamilyID { get; set; }
        public string DefaultValueJson { get; set; }
    }
}
