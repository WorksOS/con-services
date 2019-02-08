using System;
using MasterDataModels = VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// Request parameters for Raptor/TRex to get DXF linework for an alignment file
  /// </summary>
  public class AlignmentLineworkRequest : ProjectID
  {

    private AlignmentLineworkRequest()
    {
    }

    public static AlignmentLineworkRequest Create(Guid projectUid, long projectId, MasterDataModels.FileDescriptor fileDescr, DxfUnitsType userUnits)
    {
      return new AlignmentLineworkRequest
      {
        ProjectUid = projectUid,
        ProjectId = projectId,
        FileDescriptor = fileDescr,
        UserUnits = userUnits
      };
    }

    public MasterDataModels.FileDescriptor FileDescriptor { get; set; }
    public DxfUnitsType UserUnits { get; set; }
  }
}
