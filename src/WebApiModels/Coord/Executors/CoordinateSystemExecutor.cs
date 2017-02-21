
using System.Net;
using VSS.Raptor.Service.WebApiModels.Coord.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VLPDDecls;

namespace VSS.Raptor.Service.WebApiModels.Coord.Executors
{
    /// <summary>
    /// Generic coordinate system definition file executor.
    /// </summary>
    /// 
    public class CoordinateSystemExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// This constructor allows us to mock raptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        /// 
        public CoordinateSystemExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public CoordinateSystemExecutor() 
        {
        }

    /// <summary>
    /// Populates ContractExecutionStates with PDS error messages.
    /// </summary>
    /// 
    protected override void ProcessErrorCodes()
        {
            RaptorResult.AddErrorMessages(ContractExecutionStates);
        }

        /// <summary>
        /// Converts Production Data Server (PDS) client CS data set to Coordinate Service one.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// 
        protected static CoordinateSystemSettings ConvertResult(TCoordinateSystemSettings settings)
        {
            return CoordinateSystemSettings.CreateCoordinateSystemSettings(
              settings.CSName,
              settings.CSGroup,
              settings.CSIB,
              settings.DatumName,
              settings.SiteCalibration,
              settings.GeoidFileName,
              settings.GeoidName,
              settings.IsDatumGrid,
              settings.LatitudeDatumGridFileName,
              settings.LongitudeDatumGridFileName,
              settings.HeightDatumGridFileName,
              settings.ShiftGridName,
              settings.SnakeGridName,
              settings.VerticalDatumName,
              settings.UnsupportedProjection
            );
        }

        /// <summary>
        /// Reference to Coordinate System settings. 
        /// </summary>
        /// 
        protected TCoordinateSystemSettings coordSystemSettings;

        /// <summary>
        /// Sends a request to Production Data Server (PDS) client.
        /// </summary>
        /// <param name="item">A domain object.</param>
        /// <returns>Result of the processed request from PDS.</returns>
        /// 
        protected virtual TASNodeErrorStatus SendRequestToPDSClient(object item)
        {
            return TASNodeErrorStatus.asneUnknown;
        }

        /// <summary>
        /// Coordinate system definition file executor (Post/Get).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">A domain object.</param>
        /// <returns></returns>
        /// 
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
          ContractExecutionResult result = null;
          try
          {
            TASNodeErrorStatus code = SendRequestToPDSClient(item);
            
            if (code == TASNodeErrorStatus.asneOK)
                result = ConvertResult(coordSystemSettings);
            else
              throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                                         string.Format("Failed to get requested coordinate system with error: {0}.", ContractExecutionStates.FirstNameWithOffset((int)code))));  
          }
          finally
          {
              ContractExecutionStates.ClearDynamic();
          }

          return result;
        }

    }
}