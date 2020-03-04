using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class ExpandoMessage
    {
        public ExpandoObject Data { get; set; }
        public long? Partition { get; set; }
        public long Id { get; set; }
        public string Topic { get; set; }
    }
}
