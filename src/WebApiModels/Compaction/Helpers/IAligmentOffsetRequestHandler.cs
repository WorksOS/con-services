using System.Threading.Tasks;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface IAligmentOffsetRequestHandler
  {
    AlignmentOffsetsRequestHelper SetRaptorClient(IASNodeClient raptorClient);

    Task<AlignmentOffsetRequest> CreateExportAlignmentOffsetsRequest(
      DesignDescriptor fileDescriptor);
  }
}