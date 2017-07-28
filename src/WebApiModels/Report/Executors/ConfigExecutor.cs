using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Xml;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  public class ConfigExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// This constructor allows us to mock raptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        public ConfigExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public ConfigExecutor()
        {
        }

        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
            string config = String.Empty;
            try
            {
                raptorClient.RequestConfig(out config);
                log.LogTrace("Received config {0}", config);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(config);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                result = ConfigResult.CreateConfigResult(config);
            }
            catch (Exception e)
            {
                log.LogError("Exception loading config: {0} at {1}",e.Message,e.StackTrace);
                throw new ServiceException(HttpStatusCode.InternalServerError,
                        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message));
            }
            finally
            {
                //TODO: clean up
            }
            return result;


        }

        protected override void ProcessErrorCodes()
        {
        }
    }
}