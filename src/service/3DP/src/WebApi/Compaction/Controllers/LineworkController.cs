using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
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
    [Route("api/v2/linework/boundaries")]
    [HttpPost]
    public async Task<IActionResult> GetBoundariesFromLinework(DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");

      var executorRequestObj = LineworkRequest
                               .Create(requestDto)
                               .Validate();

      // TODO Upload file to IONode.

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, null, ConfigStore)
                         .ProcessAsync(executorRequestObj);

      // https://stackoverflow.com/questions/2034540/calculating-area-of-irregular-polygon-in-c-sharp

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson())
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
