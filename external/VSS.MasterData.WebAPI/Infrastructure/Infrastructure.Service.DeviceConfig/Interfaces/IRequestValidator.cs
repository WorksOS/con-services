using ClientModel.DeviceConfig.Request;
using CommonModel.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IRequestValidator<T> where T : IServiceRequest
    {
        Task<IList<IErrorInfo>> Validate(T request);
    }
}
