using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response;
using System.Threading.Tasks;

namespace Infrastructure.Service.AssetSettings.Interfaces
{
	public interface IAssetSettingsService<TRequest, TResponse>
        where TResponse : AssetSettingsBase, new()
        where TRequest : AssetSettingsRequestBase
    {
        Task<AssetSettingsServiceResponse<TResponse>> Fetch(TRequest request);
        Task<AssetSettingsServiceResponse<TResponse>> Save(TRequest request);
    }
}

