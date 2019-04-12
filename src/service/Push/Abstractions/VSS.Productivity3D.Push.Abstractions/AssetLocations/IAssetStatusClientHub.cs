using System;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Push.Abstractions.AssetLocations
{
  /// <summary>
  /// The hub definition for UI clients to request asset movements (events are not triggered via this interface)
  /// </summary>
  public interface IAssetStatusClientHub
  {
    Task StartProcessingAssets(Guid projectUid);
  }
}
