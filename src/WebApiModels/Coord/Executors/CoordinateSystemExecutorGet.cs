
using System.Net;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Coord.Executors
{
    /// <summary>
    /// Get coordinate system definition file executor.
    /// </summary>
    /// 
    public class CoordinateSystemExecutorGet : CoordinateSystemExecutor
    {

        /// <summary>
        /// This constructor allows us to mock raptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        /// 
        public CoordinateSystemExecutorGet(ILoggerFactory logger, IASNodeClient raptorClient)
            : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public CoordinateSystemExecutorGet()
        {
        }

        protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
        {
            TCoordinateSystemSettings tempCoordSystemSettings;

            ProjectID request = item as ProjectID;
            TASNodeErrorStatus code = raptorClient.RequestCoordinateSystemDetails(request.projectId ?? -1, out tempCoordSystemSettings);

            if (code == TASNodeErrorStatus.asneOK)
                coordSystemSettings = tempCoordSystemSettings;

            return code;
        }

    }
}