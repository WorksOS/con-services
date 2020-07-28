using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Line work file controller.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileImportProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided line work (DXF) file.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromForm] DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy)
                         .ProcessAsync(requestDto) as DxfLineworkFileResult;

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, result.ConvertToGeoJson(requestDto.ConvertLineStringCoordsToPolygon, requestDto.MaxVerticesPerBoundary))
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
