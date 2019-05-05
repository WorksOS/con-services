using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  /// <summary>
  /// Factory that creates sub grid segments that contain collections of cell passes
  /// </summary>
  public class SubGridCellSegmentPassesDataWrapperFactory : ISubGridCellSegmentPassesDataWrapperFactory
  {
    /// <summary>
    /// Constructs a mutable (non static) cell pass wrappers that is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    public ISubGridCellSegmentPassesDataWrapper NewMutableWrapper() => new SubGridCellSegmentPassesDataWrapper_NonStatic();

    /// <summary>
    /// Constructs an immutable (static compressed) cell pass wrappers that is a lower fidelity projection of the mutable data and which is
    /// compressed (with trivial loss level), few memory block allocated
    /// </summary>
    /// <returns></returns>
    public ISubGridCellSegmentPassesDataWrapper NewImmutableWrapper() => new SubGridCellSegmentPassesDataWrapper_StaticCompressed();
 }
}
