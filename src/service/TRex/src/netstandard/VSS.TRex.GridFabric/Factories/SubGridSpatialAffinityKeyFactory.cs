using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Factories
{
  /// <summary>
  /// Creates instances of the spatial subgrid affinity key
  /// </summary>
  public class SubGridSpatialAffinityKeyFactory : ISubGridSpatialAffinityKeyFactory
  {
    public ISubGridSpatialAffinityKey NewInstance() => new SubGridSpatialAffinityKey();
  }
}
