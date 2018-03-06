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

    public AlignmentStationRangeRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy)
    {
      this.Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      //TODO: Do we need these?
      this.ConfigurationStore = configurationStore;
      this.FileListProxy = fileListProxy;
    }

    public Task<AlignmentOffsetRequest> CreateAlignmentStationRangeRequest(DesignDescriptor fileDescriptor)
    {
      return Task.FromResult(new AlignmentOffsetRequest(){projectId = this.ProjectId, fileDescriptor = fileDescriptor});
      
    }
  }
}
