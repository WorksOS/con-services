namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  ///   Contains the address of a cell in a data model defined by it's catersian cell address with respect to the origin
  /// </summary>
  public class CellAddress
  {
    /// <summary>
    ///   The cell number on the x axis with respect to the origin
    /// </summary>
    public int x { get; private set; }

    /// <summary>
    ///   The cell number on the y axis with respect to the origin
    /// </summary>
    public int y { get; private set; }

    public CellAddress()
    {
      x = int.MaxValue;
      y = int.MaxValue;
    }

    public static CellAddress CreateCellAddress(int x, int y)
    {
      return new CellAddress
      {
        x = x,
        y = y
      };
    }
  }
}