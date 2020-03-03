using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel.DeviceConfig;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface ICGNGMapCache
    {
        void InitializeCache();
        CGNGMap GetCGNGParamAttr(string element, string attribute);
    }
}
