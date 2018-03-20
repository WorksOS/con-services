using ASNodeDecls;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
{
  /// <summary>
  /// Generic coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public CoordinateSystemExecutor() 
        {
      ProcessErrorCodes();
        }

    /// <summary>
    /// Populates ContractExecutionStates with PDS error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
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