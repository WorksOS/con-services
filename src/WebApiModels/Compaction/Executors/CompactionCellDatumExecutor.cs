using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
      // Gett grid coordinates...
      TCoordPointList pointList = GetGridCoordinates(request.projectId ?? -1, request.llPoint);

      _northing = pointList.Points.Coords[0].Y;
      _easting = pointList.Points.Coords[0].X;

      return base.GetCellDatumData(request, out data);
    }

    protected override CellDatumResponse ConvertCellDatumResult(TCellProductionData result/*, CellDatumRequest request*/)
    {
/*
      // Gett grid coordinates...
      TCoordPointList pointList = GetGridCoordinates(request.projectId ?? -1, request.llPoint);
   
      var northing = pointList.Points.Coords[0].Y;
      var easting = pointList.Points.Coords[0].X;
*/
      return CompactionCellDatumResult.CreateCompactionCellDatumResult(
        RaptorConverters.convertDisplayMode((TICDisplayMode)result.DisplayMode),
        result.ReturnCode,
        result.Value,
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
          $"On Cell Datum request. Failed to process coordinate conversion request with error: {ContractExecutionStates.FirstNameWithOffset((int) code)}."));
      }

      return pointList;
    }
  }
}

