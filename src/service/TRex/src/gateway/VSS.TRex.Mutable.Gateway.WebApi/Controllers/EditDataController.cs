using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for editing production data using machine events.
  /// HttpGet endpoints use the immutable endpoint (at present VSS.TRex.Gateway.WebApi)
  /// </summary>
  public class EditDataController : BaseController
  {
    /// <summary>
    /// Constructor with injection
    /// </summary>
    public EditDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<TagFileController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Applies an edit to production data to correct data that has been recorded wrongly in Machines by Operator.
    /// </summary>
    [Route("api/v1/productiondataedit")]
    [HttpPost]
    public Task<ContractExecutionResult> AddDataEdit([FromBody]TRexEditData request)
    {
      Log.LogInformation($"{nameof(AddDataEdit)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();
      return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
        .Build<AddEditDataExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
        .ProcessAsync(request));
    }

    /// <summary>
    /// Removes a previously applied edit to production data.
    /// </summary>
    [Route("api/v1/productiondataedit")]
    [HttpDelete]
    public Task<ContractExecutionResult> RemoveDataEdit([FromBody]TRexEditData request)
    {
      Log.LogInformation($"{nameof(RemoveDataEdit)}: {JsonConvert.SerializeObject(request)}");
      request.Validate();
      return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
        .Build<RemoveEditDataExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
        .ProcessAsync(request));
    }

  }
}
