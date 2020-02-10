using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigMessageParser<T>
    {
        T GetDataTransferObject(JToken jToken, DeviceConfigParameterGroups deviceConfigParameterGroups);
    }
}
