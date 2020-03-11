using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings.ConfigNameValues
{
    public class ConfigNameValueCollection 
    {
        public Dictionary<string, string> Values { get; set; }
    }

	//TODO: Remove below classes, when DI supports for Single Interface with multiple objects registeration by name
	public class DeviceConfigRequestToAttributeMaps : ConfigNameValueCollection { }

	public class DeviceConfigAttributeToRequestMaps : ConfigNameValueCollection { }

	public class DeviceConfigParameterAttributeMaps : ConfigNameValueCollection { }

	public class DeviceConfigParameterNames : ConfigNameValueCollection { }
}
