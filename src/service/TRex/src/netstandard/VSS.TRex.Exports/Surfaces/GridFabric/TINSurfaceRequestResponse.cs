using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The response returned from the TIN surface export request executor that contains the response code and the
  /// surface generated from the operation
  /// Note: This response does not support Ignite binarizable serialization of the TIN member and is intended as an intermediary
  /// response between the primary application request context and the local TINGEN instance within the same service boundary
  /// </summary>
  public class TINSurfaceRequestResponse : SubGridsPipelinedResponseBase, INonBinarizable
  {
    /// <summary>
    /// The TIN generated from the selected elevations matching the query
    /// </summary>
    public TrimbleTINModel TIN { get; set; }
  }
}
