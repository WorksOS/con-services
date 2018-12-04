using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.ConfigurationStore;
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
  [ProjectVerifier]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from a linework (DXF) file.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v2/linework/boundaries")]
    [HttpGet]
//    public async Task<IActionResult> GetBoundariesFromLinework([FromBody] LineworkFileRequest requestDto)
    public async Task<IActionResult> GetBoundariesFromLinework(
      Guid projectUid,
      Guid designUid
      )
    {
    //  var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(requestDto, "Data");
  //    Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: " + serializedRequest);

      Task<long> projectId = GetLegacyProjectId(projectUid);
      Task<DesignDescriptor> designDescriptorTask = GetAndValidateDesignDescriptor(projectUid, designUid);

      var requestObj = LineworkRequest.Create(
        projectId.Result,
        designDescriptorTask.Result,
        TVLPDDistanceUnits.vduImperialFeet,
        null
        );

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
                         .ProcessAsync(requestObj).ConfigureAwait(false);

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, result)
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
