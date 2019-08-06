using System.Threading.Tasks;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;

namespace VSS.Productivity3D.Push.Abstractions.AssetLocations
{
  /// <summary>
  /// This is the hub context, that is used service side to generate asset movement events (can only receive these in the UI, not send)
  /// Hence the two different interfaces (the hub doesn't define this, only the hub context).
  /// </summary>
  public interface IAssetStatusClientHubContext : IAssetStatusClientHub
  {
    Task UpdateAssetStatus(AssetAggregateStatus assets);
  }
}
