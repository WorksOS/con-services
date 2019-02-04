using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
  /// Linework file controller.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class LineworkController : BaseController<LineworkController>
  {
    /// <inheritdoc />
    public LineworkController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Get all boundaries from provided linework (DXF) file.
    /// </summary>
    [HttpPost("api/v2/linework/boundaries")]
    public async Task<IActionResult> GetBoundariesFromLinework([FromServices] IRaptorFileUploadUtility fileUploadUtility, [FromBody] DxfFileRequest requestDto)
    {
      Log.LogDebug($"{nameof(GetBoundariesFromLinework)}: {requestDto}");
#if RAPTOR
      var customerUid = ((RaptorPrincipal)Request.HttpContext.User).CustomerUid;
      var uploadPath = Path.Combine(ConfigStore.GetValueString("SHAREUNC"), "Temp", "LineworkFileUploads", customerUid);
      requestDto.Filename = Guid.NewGuid().ToString();

      var executorRequestObj = new LineworkRequest(requestDto, uploadPath).Validate();

      (bool uploadSuccess, string message) = fileUploadUtility.UploadFile(executorRequestObj.FileDescriptor, executorRequestObj.FileData);

      if (!uploadSuccess) return StatusCode((int)HttpStatusCode.BadRequest, message);

      var result = await RequestExecutorContainerFactory
                         .Build<LineworkFileExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore)
                         .ProcessAsync(executorRequestObj);

      fileUploadUtility.DeleteFile(Path.Combine(executorRequestObj.FileDescriptor.Path, executorRequestObj.FileDescriptor.FileName));

      return result.Code == 0
        ? StatusCode((int)HttpStatusCode.OK, ((DxfLineworkFileResult)result).ConvertToGeoJson())
        : StatusCode((int)HttpStatusCode.BadRequest, result);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
