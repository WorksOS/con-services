using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;
using CommonModel.Error;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface ICommonValidator<T> where T : ICommonValidatorBase
    {
        Task<IList<IErrorInfo>> Validate(T request);
    }
}
