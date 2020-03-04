using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class Parameter
    {
        public List<AttributeDetails> Attributes { get; set; }
        public string ParameterName { get; set; }
        public DateTime? EventUTC { get; set; }
    }
}
