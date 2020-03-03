using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class ParamGroup
    {
        public string GroupName { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}
