using System;
using VSS.TRex.Designs;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the Patchs request
  /// </summary>
  [Serializable]
  public class TINSurfaceRequestArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The tolerance to use (in meters) when decimating the elevation surface into a TIN
    /// </summary>
    public double Tolerance { get; set; }
  }
}
