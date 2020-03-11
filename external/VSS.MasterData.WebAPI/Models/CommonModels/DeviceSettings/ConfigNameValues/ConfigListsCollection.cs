using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings.ConfigNameValues
{
    public class ConfigListsCollection
    {
        public List<string> Values { get; set; }
    }

	//TODO: Remove below classes, when DI supports for Single Interface with multiple objects registeration by name
	public class ParameterGroupsNonMandatoryLists : ConfigListsCollection { }

	public class ParametersNonMandatoryLists : ConfigListsCollection { }

	public class AttributesNonMandatoryLists : ConfigListsCollection { }
}
