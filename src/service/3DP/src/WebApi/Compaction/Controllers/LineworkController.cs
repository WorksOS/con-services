using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Linework (DXF) file controller.
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided linework (DXF) files.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromServices] IRaptorFileUploadUtility fileUploadUtility, DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");

      var customerUid = ((RaptorPrincipal)Request.HttpContext.User).CustomerUid;

      var executorRequestObj = LineworkRequest
                               .Create(requestDto, customerUid)
                               .Validate();

      fileUploadUtility.UploadFile(
        executorRequestObj.FileDescriptor, 
        customerUid,
        executorRequestObj.FileData);

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
                         .ProcessAsync(executorRequestObj);

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson())
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
