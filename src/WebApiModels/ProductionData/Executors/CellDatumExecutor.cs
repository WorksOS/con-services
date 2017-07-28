using Microsoft.Extensions.Logging;
using SVOICDecls;
using SVOICFilterSettings;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class CellDatumExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// This constructor allows us to mock raptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        public CellDatumExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public CellDatumExecutor()
        {
        }

        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
            CellDatumRequest request = item as CellDatumRequest;
          try
          {

            TCellProductionData data;

            TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterId, request.filter,request.projectId);
            if (raptorClient.GetCellProductionData
                (request.projectId ?? -1,
                    (int) RaptorConverters.convertDisplayMode(request.displayMode),
                    request.gridPoint != null ? request.gridPoint.x : 0,
                    request.gridPoint != null ? request.gridPoint.y : 0,
                    request.llPoint != null ? RaptorConverters.convertWGSPoint(request.llPoint) : new TWGS84Point(),                    
                    request.llPoint == null,
                    raptorFilter,
                    RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                    RaptorConverters.DesignDescriptor(request.design),
                    out data))
            {
              result = convertCellDatumResult(data);
            }
            else
            {
              throw new ServiceException(HttpStatusCode.BadRequest,  new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  "No cell datum returned"));
            }
          }
          finally
          {
            
          }
          return result;
        }


        private CellDatumResponse convertCellDatumResult(TCellProductionData result)
        {
          return CellDatumResponse.CreateCellDatumResponse(
              RaptorConverters.convertDisplayMode((TICDisplayMode) result.DisplayMode),
                  result.ReturnCode,
                  result.Value,
                  result.TimeStampUTC);
            
        }

        protected override void ProcessErrorCodes()
        {
  
        }
    }
    
    
}