using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  /// <summary>
  /// Factory that creates structures that contain a 'latest pass' records for cells in a sub grid or segment
  /// </summary>
  public class SubGridCellLatestPassesDataWrapperFactory : ISubGridCellLatestPassesDataWrapperFactory
  {
    /// <summary>
    /// Constructs a global mutable (non static) cell pass wrapper which is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewMutableWrapper_Global()
    {
      var result = new SubGridCellLatestPassDataWrapper_NonStatic();
      result.ClearPasses();

      return result;
    }

    /// <summary>
    /// Constructs a mutable (non static) segment cell pass wrapper which is a fully mutable high fidelity representation (most memory blocks allocated)
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewMutableWrapper_Segment()
    {
      var result = new SubGridCellLatestPassDataWrapper_NonStatic();
      result.ClearPasses();

      return result;
    }

    /// <summary>
    /// Constructs a global immutable (static) cell pass wrapper which is immutable projection with lower fidelity and which
    /// is compressed (with trivial loss level), few memory blocks allocated
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewImmutableWrapper_Global()
    {
      var result = new SubGridCellLatestPassDataWrapper_StaticCompressed();
      result.ClearPasses();

      return result;
    }

    /// <summary>
    /// Constructs an immutable (static) segment cell pass wrapper which is immutable projection with lower fidelity and which
    /// is compressed (with trivial loss level), few memory blocks allocated
    /// Note: Immutable contexts currently do not use the latest cell pass information in segments. Mutable contexts use this to
    /// support efficient determination of global latest cell pass information.
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper NewImmutableWrapper_Segment()
    {
      return null;
    }
  }
}
