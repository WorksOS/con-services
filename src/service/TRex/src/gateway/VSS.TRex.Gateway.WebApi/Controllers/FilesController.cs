using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Files;
using VSS.TRex.Gateway.Common.Requests;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to provide functionality related to file manipulations, such as extracting boundaries from a DXF file,
  /// or rendering geometry from an SVL alignment file
  /// </summary>
  public class FilesController : BaseController
  {
    static FilesController()
    {
      ResolverCache.Add(
        nameof(DXFBoundariesRequest),
        new JsonSerializerSettings()
        {
          ContractResolver = new IgnorePropertiesResolver(
            new[] { nameof(DXFBoundariesRequest.CSIBFileData), nameof(DXFBoundariesRequest.DXFFileData) })
        });
    }

    /// <inheritdoc />
    public FilesController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignController>(), serviceExceptionHandler, configStore)
    { }

    /// <summary>
    /// Extracts a collection of boundaries from a DXF file for use a project or site geo fence boundaries
    /// </summary>
    [HttpPost("api/v1/files/dxf/boundaries")]
    public async Task<IActionResult> ExtractBoundariesFromDXF([FromBody] DXFBoundariesRequest request)
    {
      Log.LogInformation($"{nameof(ExtractBoundariesFromDXF)}: {JsonConvert.SerializeObject(request, ResolverCache[nameof(DXFBoundariesRequest)])}");
      request.Validate();

      if (request.FileType == ImportedFileType.Linework || request.FileType == ImportedFileType.SiteBoundary)
      {
        var response = await WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
          .Build<ExtractDXFBoundariesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));

        return Ok(response);
      }

      return BadRequest(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DXF"));
    }
  }
}
