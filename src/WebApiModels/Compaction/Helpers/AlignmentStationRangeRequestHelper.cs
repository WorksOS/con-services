using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Creates a simple request to retrieve station range for Alignment file
  /// </summary>
  public class AlignmentStationRangeRequestHelper : DataRequestBase, IAligmentStationRangeRequestHandler
  {

    public AlignmentStationRangeRequestHelper()
    {
    }

    public AlignmentStationRangeRequestHelper(ILoggerFactory logger)
    {
      this.Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
    }

    public AlignmentStationRangeRequest CreateAlignmentStationRangeRequest(DesignDescriptor fileDescriptor)
    {
      return new AlignmentStationRangeRequest{projectId = this.ProjectId, fileDescriptor = fileDescriptor};
      
    }
  }
}
