using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Types
{
  /// <summary>
  /// LiftDetectionType defines the method by which the server builds cell pass profiles
  /// </summary>
  public enum LiftDetectionType
    {
       Automatic,
       MapReset,
       AutoMapReset,
       Tagfile,
       None
    }
}
