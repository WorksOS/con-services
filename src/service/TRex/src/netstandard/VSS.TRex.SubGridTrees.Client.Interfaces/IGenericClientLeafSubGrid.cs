namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  /// <summary>
  /// Provides a generic interface that accesses the type cells member of descendant GenericClientLeafSubGrid implementations
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IGenericClientLeafSubGrid<T> : IClientLeafSubGrid
  {
    /// <summary>
    /// The 2 dimensional array of cell values in this generic leaf sub grid
    /// </summary>
    T[,] Cells { get; }

    /// <summary>
    /// Returns the null cell value for elements in this client leaf sub grid
    /// </summary>
    /// <returns></returns>
    T NullCell();
  }
}
