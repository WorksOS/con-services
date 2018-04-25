using System;

namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  ///   Contains the address of a cell in a data model defined by it's catersian cell address with respect to the origin
  ///   This is copied from ...\RaptorServicesCommon\Models\CellAddress.cs
  /// </summary>
  public class CellAddress
  {
    /// <summary>
    ///   The cell number on the x axis with respect to the origin
    /// </summary>
    public Int32 x { get; set; }

    /// <summary>
    ///   The cell number on the y axis with respect to the origin
    /// </summary>
    public Int32 y { get; set; }
  }
}