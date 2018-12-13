using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The response returned from the TIN surface export request executor that contains the response code and the
  /// surface generated from the operation
  /// </summary>
  public class TINSurfaceRequestResponse : SubGridsPipelinedResponseBase
  {
    /// <summary>
    /// The TIN generated from the selected elevations matching the query
    /// </summary>
    public TrimbleTINModel TIN { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      throw new TRexNonBinarizableException();
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      throw new TRexNonBinarizableException();
    }
  }
}
