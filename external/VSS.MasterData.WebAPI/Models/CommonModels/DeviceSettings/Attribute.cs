using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class AttributeDetails
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public DateTime SentUTC { get; set; }
    }
}
