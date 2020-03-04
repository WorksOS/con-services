using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class DeviceConfigParameterGroups
    {
        public List<DeviceConfigParameterGroup> Groups { get; set; }
    }

    public class DeviceConfigParameterGroup
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<DeviceConfigParameterGroupProperty> Elements { get; set; }
    }

    public class DeviceConfigParameterGroupProperty
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ValuePath { get; set; }
    }
}
