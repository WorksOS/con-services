using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Files;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to provide functionality related to file manipulations, such as extracting boundaries from a DXF file,
  /// or rendering geometry from an SVL alignment file
  /// </summary>
  public class FilesController : BaseController
  {
    /// <inheritdoc />
    public FilesController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Extracts a collection of boundaries from a DXF file for use a project or site geo fence boundaries
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Route("api/v1/files/dxf/boundaries")]
    [HttpPost]
    public Task<ContractExecutionResult> ExtractBoundariesFromDXF([FromBody] DXFBoundariesRequest request)
    {
      Log.LogInformation($"{nameof(ExtractBoundariesFromDXF)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();

      if (request.FileType == ImportedFileType.Linework || request.FileType == ImportedFileType.SiteBoundary)
      {
        return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
          .Build<ExtractDXFBoundariesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DXF"));
    }
  }
}
