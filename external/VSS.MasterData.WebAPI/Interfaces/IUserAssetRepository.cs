using System.Collections.Generic;
using System.Threading.Tasks;
using DbModel;
using DbModel.DeviceConfig;

namespace Interfaces
{
    public interface IUserAssetRepository
    {
        Task<IEnumerable<string>> FetchValidAssetUIds(IList<string> assetUids, UserAssetDto request);
    }
}
