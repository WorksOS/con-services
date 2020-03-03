using DbModel.AssetSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IAssetConfigRepository
    {
        Task<IEnumerable<AssetSettingsDto>> FetchAssetConfig(List<string> assetUids, AssetSettingsDto request);
    }
}
