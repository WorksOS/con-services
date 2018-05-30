using System.Net;
using SVOICDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionCellDatumExecutor : CellDatumExecutor
  {
    private double _northing;
    private double _easting;

    protected override bool GetCellDatumData(CellDatumRequest request, out TCellProductionData data)
    {
      if (request.llPoint == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No WGS84 coordinates provided"));
      }

      // Gett grid coordinates...
      TCoordPointList pointList = GetGridCoordinates(request.ProjectId ?? -1, request.llPoint);


      _northing = pointList.Points.Coords[0].Y;
      _easting = pointList.Points.Coords[0].X;

      return base.GetCellDatumData(request, out data);
    }

    protected override CellDatumResponse ConvertCellDatumResult(TCellProductionData result)
    {
      return new CompactionCellDatumResult(
        RaptorConverters.convertDisplayMode((TICDisplayMode)result.DisplayMode),
        result.ReturnCode,
        result.ReturnCode == 0 ? result.Value : (double?)null,
        result.TimeStampUTC,
        _northing,
        _easting);
    }

    private TCoordPointList GetGridCoordinates(long projectId, WGSPoint latLon)
    {
      TWGS84FenceContainer latLongs = new TWGS84FenceContainer { FencePoints = new TWGS84Point[] { RaptorConverters.convertWGSPoint(latLon) } };

      TCoordReturnCode code = raptorClient.GetGridCoordinates
      (
        projectId,
        latLongs,
        TCoordConversionType.ctLLHtoNEE,
        out var pointList
      );

      if (code != TCoordReturnCode.nercNoError || pointList.Points.Coords == null || pointList.Points.Coords.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "On Cell Datum request. Failed to process coordinate conversion request."));
      }

      return pointList;
    }
  }
}

