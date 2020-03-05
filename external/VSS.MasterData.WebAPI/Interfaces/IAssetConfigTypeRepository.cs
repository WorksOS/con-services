using DbModel.AssetSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IAssetConfigTypeRepository
    {
        Task<IEnumerable<AssetConfigTypeDto>> FetchByConfigTypeNames(AssetConfigTypeDto configTypeNames);
    }
}
