using System;
using System.Net;
using System.Xml;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ConfigExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        raptorClient.RequestConfig(out var config);
        log.LogTrace("Received config {0}", config);
        var doc = new XmlDocument();
        doc.LoadXml(config);
        return ConfigResult.Create(config);
      }
      catch (Exception e)
      {
        log.LogError("Exception loading config: {0} at {1}", e.Message, e.StackTrace);
        throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
      }
    }
  }
}
