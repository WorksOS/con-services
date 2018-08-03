using VSS.TRex.Designs.TTM;
using VSS.TRex.Pipelines;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The response returned from the TIN surface export request executor that contains the response code and the
  /// surface generated from the operation
  /// </summary>
  public class TINSurfaceRequestResponse : SubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The TIN generated from the selected elevations matching the query
    /// </summary>
    public TrimbleTINModel TIN { get; set; }
  }
}
