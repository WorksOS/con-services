using CommonModel.Error;
using ClientModel.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IRequestValidator<T> where T: IServiceRequest
    {
        Task<IList<IErrorInfo>> Validate(T request);
    }
}
