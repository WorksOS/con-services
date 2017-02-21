using System;
using System.Net;
using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
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
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(config);
                string jsonText = JsonConvert.SerializeXmlNode(doc);
                result = ConfigResult.CreateConfigResult(jsonText);
            }
            catch (Exception e)
            {
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