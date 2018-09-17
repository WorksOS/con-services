namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  /// <summary>
  /// Provides a genericised interface that accesses the type cells member of descendant GenericClientLeafSubGrid implementations
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IGenericClientLeafSubGrid<T> : IClientLeafSubGrid
  {
    /// <summary>
    /// The 2 dimensional array of cell values in this generic leaf subgrid
    /// </summary>
    T[,] Cells { get; set; }
  }
}
