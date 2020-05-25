using System;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request parameters for Raptor/TRex to get center line alignment geometry an alignment file
  /// </summary>
  public class AlignmentGeometryRequest : ProjectID
  {
    public Guid DesignUid { get; private set; }

    public AlignmentGeometryRequest(Guid projectUid, Guid designUid)
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
    }
  }
}
