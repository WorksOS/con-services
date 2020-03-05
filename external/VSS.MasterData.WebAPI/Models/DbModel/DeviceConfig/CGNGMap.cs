using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class CGNGMap
    {
        public string Group { get; set; }
        public string CG_Element { get; set; }
        public string CG_Attribute { get; set; }
        public NGParamAttr NGMap { get; set; }
    }

    public class NGParamAttr
    {
        public string NG_Parameter { get; set; }
        public string NG_Attribute { get; set; }
    }
}
