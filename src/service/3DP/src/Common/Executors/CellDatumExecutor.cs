using System;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using SVOICDecls;
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Executors
{
  public class CellDatumExecutor : RequestExecutorContainer
  {
    private ServiceException CreateNoCellDatumReturnedException()
    {
      return new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "No cell datum returned"));
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CellDatumRequest>(item);
#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_CELL_DATUM"))
      {
#endif
        var trexData = await GetTRexCellDatumData(request);

        if (trexData != null)
          return ConvertTRexCellDatumResult(trexData);

        throw CreateNoCellDatumReturnedException();
#if RAPTOR
      }

      if (GetCellDatumData(request, out var data))
        return ConvertCellDatumResult(data);

      throw CreateNoCellDatumReturnedException();
#endif
    }

    protected virtual async Task<object> GetTRexCellDatumData(CellDatumRequest request)
    {
      // TODO To be implemented once getting cell datum endpoint is exposed in the TRex Gateway WebAPI.
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
    }

    protected virtual CellDatumResponse ConvertTRexCellDatumResult(object result)
    {
      // TODO To be implemented once getting cell datum endpoint is exposed in the TRex Gateway WebAPI.
      return null;
    }
#if RAPTOR
    protected virtual bool GetCellDatumData(CellDatumRequest request, out TCellProductionData data)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter);

      return raptorClient.GetCellProductionData
      (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        (int)RaptorConverters.convertDisplayMode(request.DisplayMode),
        request.GridPoint?.x ?? 0,
        request.GridPoint?.y ?? 0,
        request.LLPoint != null ? RaptorConverters.ConvertWGSPoint(request.LLPoint) : new TWGS84Point(),
        request.LLPoint == null,
        raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        RaptorConverters.DesignDescriptor(request.Design),
        out data);
    }

    protected virtual CellDatumResponse ConvertCellDatumResult(TCellProductionData result)
    {
      return new CellDatumResponse(
          RaptorConverters.convertDisplayMode((TICDisplayMode) result.DisplayMode),
              result.ReturnCode,
              result.ReturnCode == 0 ? result.Value : (double?)null,
              result.TimeStampUTC);
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
