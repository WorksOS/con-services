using System;

namespace ClientModel.DeviceConfig.Request
{
    public interface IServiceRequest
    {
        Guid? CustomerUID { get; set; }
        Guid? UserUID { get; set; }
    }
}
