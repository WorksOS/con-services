using System;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using SVOICDecls;
using VLPDDecls;
using VSS.Productivity3D.Common.Proxies;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
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
          return trexData;

        throw CreateNoCellDatumReturnedException();
#if RAPTOR
      }

      if (GetCellDatumData(request, out var data))
        return ConvertCellDatumResult(data);

      throw CreateNoCellDatumReturnedException();
#endif
    }

    protected virtual async Task<CellDatumResult> GetTRexCellDatumData(CellDatumRequest request)
    {
      var trexRequest = new CellDatumTRexRequest(request.ProjectUid.Value, request.DisplayMode, request.LLPoint,
        request.GridPoint, request.Filter, request.Design?.FileUid, request.Design?.Offset, 
        AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings));
      return await trexCompactionDataProxy.SendDataPostRequest<CompactionCellDatumResult, CellDatumTRexRequest>(trexRequest, "/cells/datum", customHeaders);
    }
#if RAPTOR
    protected virtual bool GetCellDatumData(CellDatumRequest request, out TCellProductionData data)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);

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

    protected virtual CellDatumResult ConvertCellDatumResult(TCellProductionData result)
    {
      return new CellDatumResult(
          RaptorConverters.convertDisplayMode((TICDisplayMode) result.DisplayMode),
          (CellDatumReturnCode)result.ReturnCode,
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
