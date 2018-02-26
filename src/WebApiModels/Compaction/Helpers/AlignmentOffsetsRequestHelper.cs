using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Creates a simple request to retrieve offsets for Alignment file
  /// </summary>
  public class AlignmentOffsetsRequestHelper : DataRequestBase, IAligmentOffsetRequestHandler
  {
    private IASNodeClient raptorClient;
    private IAlignmentTileService alignmentService;

    public AlignmentOffsetsRequestHelper()
    {
    }

    public AlignmentOffsetsRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, IAlignmentTileService alignmentService)
    {
      this.Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      this.ConfigurationStore = configurationStore;
      this.FileListProxy = fileListProxy;
      this.alignmentService = alignmentService;
    }

    public AlignmentOffsetsRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      this.raptorClient = raptorClient;
      return this;
    }

    public Task<AlignmentOffsetRequest> CreateExportAlignmentOffsetsRequest(DesignDescriptor fileDescriptor)
    {
      return Task.FromResult(new AlignmentOffsetRequest(){projectId = this.ProjectId, fileDescriptor = fileDescriptor});
      
    }
  }
}
