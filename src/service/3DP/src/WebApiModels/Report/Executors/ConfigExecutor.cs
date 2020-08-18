using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ConfigExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        return await trexCompactionDataProxy.SendDataGetRequest<ConfigResult>(string.Empty, $"/configuration", customHeaders);
      }
      catch (Exception e)
      {
        log.LogError(e, "Exception obtaining tRex config");
        throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
      }
    }
  }
}
