using System;

namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  ///   Contains the address of a cell in a data model defined by it's catersian cell address with respect to the origin
  /// </summary>
  public class CellAddress
  {
    /// <summary>
    ///   The cell number on the x axis with respect to the origin
    /// </summary>
    public Int32 x { get; private set; }

    /// <summary>
    ///   The cell number on the y axis with respect to the origin
    /// </summary>
    public Int32 y { get; private set; }

      public static CellAddress HelpSample {
          get
          {
              return new CellAddress()
                     {
                             x = 100,
                             y = 200
                     };
          }
      }

    public CellAddress()
    {
      x = int.MaxValue;
      y = int.MaxValue;
    }

      public static CellAddress CreateCellAddress(Int32 x, Int32 y)
    {
      return new CellAddress
      {
        x = x,
        y = y
      };
    }
  }
}