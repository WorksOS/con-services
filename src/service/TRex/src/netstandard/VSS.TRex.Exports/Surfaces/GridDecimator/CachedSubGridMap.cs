using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  public struct CachedSubGridMap
  {
    public GenericLeafSubGrid<float> SubGrid;
    public long TriangleScanInvocationNumber;
  }
}
