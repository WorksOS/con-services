using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  /// <summary>
  /// Factory that creates structures that contain a 'latest pass' records for cells in a sub grid or segment
  /// </summary>
  public class SubGridCellLatestPassesDataWrapperFactory : ISubGridCellLatestPassesDataWrapperFactory
  {
    /// <summary>
    /// Constructs a mutable (non static) segment cell pass wrapper which is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewMutableWrapper()
    {
      var result = new SubGridCellLatestPassDataWrapper_NonStatic();
      result.ClearPasses();

      return result;
    }

    /// <summary>
    /// Constructs an immutable (static) segment cell pass wrapper which is immutable projection with lower fidelity and which
    /// is compressed (with trivial loss level), few memory blocks allocated
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewImmutableWrapper()
    {
      var result = new SubGridCellLatestPassDataWrapper_StaticCompressed();
      result.ClearPasses();

      return result;
    }
  }
}
