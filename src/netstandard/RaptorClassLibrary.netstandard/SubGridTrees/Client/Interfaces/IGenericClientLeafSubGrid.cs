namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  /// <summary>
  /// Provides a genericised interface that access the type cells member of descenedant GenericClientLeafSubGrid implementations
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IGenericClientLeafSubGrid<T>
  {
    /// <summary>
    /// The 2 dimensional array of cell values in this generic leaf subgrid
    /// </summary>
    T[,] Cells { get; set; }
  }
}
