using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CSIBController : BaseController<CSIBController>
  {
    /// <inheritdoc />
    public CSIBController(IConfigurationStore configStore)
      : base(configStore)
    { }

    /// <summary>
    /// Get the CSIB for a given project.
    /// </summary>
    [HttpGet("api/v1/csib")]
    public async Task<IActionResult> GetCSIBForProject([FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetCSIBForProject)}");

      var projectId = await GetLegacyProjectId(projectUid);

      var result = await RequestExecutorContainerFactory.Build<CSIBExecutor>(LoggerFactory,
#if RAPTOR
          RaptorClient,
#endif
          configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(new ProjectID(projectId, projectUid));

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, result)
        : StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
