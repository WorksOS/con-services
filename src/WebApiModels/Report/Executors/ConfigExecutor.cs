using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Xml;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  public class ConfigExecutor : RequestExecutorContainer
    {
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result;
            string config = string.Empty;

            try
            {
                raptorClient.RequestConfig(out config);
                log.LogTrace("Received config {0}", config);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(config);
                result = ConfigResult.CreateConfigResult(config);
            }
            catch (Exception e)
            {
                log.LogError("Exception loading config: {0} at {1}",e.Message,e.StackTrace);
                throw new ServiceException(HttpStatusCode.InternalServerError,
                        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
            }

          return result;
        }
    }
}