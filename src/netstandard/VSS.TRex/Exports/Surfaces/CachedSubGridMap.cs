using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Exports.Surfaces
{
  public struct CachedSubGridMap
  {
    public GenericLeafSubGrid<float> SubGrid;
    public long TriangleScanInvocationNumber;
  }
}
