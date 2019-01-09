using System.Net;
using SVOICDecls;
using VLPDDecls;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CellDatumRequest;

      if (request == null)
        ThrowRequestTypeCastException<CellDatumRequest>();

      if (GetCellDatumData(request, out var data))
        return ConvertCellDatumResult(data);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "No cell datum returned"));
    }

    protected virtual bool GetCellDatumData(CellDatumRequest request, out TCellProductionData data)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.filter);

      return raptorClient.GetCellProductionData
      (request.ProjectId ?? -1,
        (int)RaptorConverters.convertDisplayMode(request.displayMode),
        request.gridPoint?.x ?? 0,
        request.gridPoint?.y ?? 0,
        request.llPoint != null ? RaptorConverters.ConvertWGSPoint(request.llPoint) : new TWGS84Point(),
        request.llPoint == null,
        raptorFilter,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        RaptorConverters.DesignDescriptor(request.design),
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
  }
}
