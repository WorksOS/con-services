using System.Threading.Tasks;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigService<TRequest, TResponse>
        where TResponse : DeviceConfigResponseBase, new()
        where TRequest : DeviceConfigRequestBase
    {
        Task<DeviceConfigServiceResponse<TResponse>> Fetch(TRequest request);
        Task<DeviceConfigServiceResponse<TResponse>> Save(TRequest request);
    }
}
