using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Exports.Surfaces
{
  public enum DecimationResult
  {
    NoError = 0,
    Unknown = 1,
    NoDataStore = 2,
    NoData = 3,
    TrianglesExceeded = 4,
    DestinationTINNotEmpty
  }
}
