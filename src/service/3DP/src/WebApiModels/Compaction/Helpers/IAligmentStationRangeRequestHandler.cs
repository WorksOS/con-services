using System.Threading.Tasks;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface IAligmentStationRangeRequestHandler
  {
    AlignmentStationRangeRequest CreateAlignmentStationRangeRequest(
      DesignDescriptor fileDescriptor);
  }
}
