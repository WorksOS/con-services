using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Creates a simple request to retrieve station range for Alignment file
  /// </summary>
  public class AlignmentStationRangeRequestHelper : DataRequestBase, IAligmentStationRangeRequestHandler
  {
    public AlignmentStationRangeRequestHelper()
    { }

    public AlignmentStationRangeRequestHelper(ILoggerFactory logger)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
    }

    public AlignmentStationRangeRequest CreateAlignmentStationRangeRequest(DesignDescriptor fileDescriptor)
    {
      return new AlignmentStationRangeRequest{projectId = ProjectId, fileDescriptor = fileDescriptor};
    }
  }
}
