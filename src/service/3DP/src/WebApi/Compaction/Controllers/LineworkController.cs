using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Linework (DXF) file controller.
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided linework (DXF) files.
    /// </summary>
    [Route("api/v2/linework/boundaries")]
    [HttpPost]
    public async Task<IActionResult> GetBoundariesFromLinework(DxfFileRequest requestDto)
    {
      // TODO Request logging?

      // TODO Upload file to temp folder on Raptor ASNode host.

      var requestObj = LineworkRequest.Create(
        "aarons survey.dxf",
        @"D:\VLPDProductionData\Temp\12121212\",
        string.Empty,
        TVLPDDistanceUnits.vduImperialFeet,
        null);

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
                         .ProcessAsync(requestObj);

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, result)
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
