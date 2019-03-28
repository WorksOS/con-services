using System;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using SVOICDecls;
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionCellDatumExecutor : CellDatumExecutor
  {
    private double _northing;
    private double _easting;

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionCellDatumExecutor()
    {
      ProcessErrorCodes();
    }

    private void CheckForCoordinate(WGSPoint coordinate)
    {
      if (coordinate == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No WGS84 coordinates provided"));
      }
    }

    protected override async Task<object> GetTRexCellDatumData(CellDatumRequest request)
    {
      CheckForCoordinate(request.LLPoint);

      // Gett TRex grid coordinates...
      var coordinate = await GetTRexGridCoordinates(request.ProjectUid ?? Guid.Empty, request.LLPoint);

      _northing = coordinate.Y;
      _easting = coordinate.X;

      return base.GetTRexCellDatumData(request);
    }

    private async Task<TwoDConversionCoordinate> GetTRexGridCoordinates(Guid projectUid, WGSPoint latLon)
    {
      var conversionCoordinates = new[] { new TwoDConversionCoordinate(latLon.Lon, latLon.Lat) };

      var conversionRequest = new CoordinateConversionRequest(projectUid, TwoDCoordinateConversionType.NorthEastToLatLon, conversionCoordinates);
      var conversionResult = await trexCompactionDataProxy.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(conversionRequest, "/coordinateconversion", customHeaders);

      if (conversionResult.Code != 0 || conversionResult.ConversionCoordinates.Length == 0)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve long lat for the passed coordinate."));

      return conversionResult.ConversionCoordinates[0];
    }

    protected override CellDatumResult ConvertTRexCellDatumResult(object trexResult)
    {
      // TODO To be implemented once getting cell datum endpoint is exposed in the TRex Gateway WebAPI.
      return null;
    }

#if RAPTOR
    protected override bool GetCellDatumData(CellDatumRequest request, out TCellProductionData data)
    {
      CheckForCoordinate(request.LLPoint);
      
      var pointList = GetGridCoordinates(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.LLPoint);
      
      _northing = pointList.Points.Coords[0].Y;
      _easting = pointList.Points.Coords[0].X;

      return base.GetCellDatumData(request, out data);
    }

    protected override CellDatumResult ConvertCellDatumResult(TCellProductionData result)
    {
      return new CompactionCellDatumResult(
        RaptorConverters.convertDisplayMode((TICDisplayMode)result.DisplayMode),
        (CellDatumReturnCode)result.ReturnCode,
        result.ReturnCode == 0 ? result.Value : (double?)null,
        result.TimeStampUTC,
        _northing,
        _easting);
    }

    private TCoordPointList GetGridCoordinates(long projectId, WGSPoint latLon)
    {
      var latLongs = new TWGS84FenceContainer { FencePoints = new [] { RaptorConverters.ConvertWGSPoint(latLon) } };

      var code = raptorClient.GetGridCoordinates
      (
        projectId,
        latLongs,
        TCoordConversionType.ctLLHtoNEE,
        out var pointList
      );

      if (code != TCoordReturnCode.nercNoError || pointList.Points.Coords == null || pointList.Points.Coords.Length == 0)
        throw CreateServiceException<CompactionCellDatumExecutor>((int)code);

      return pointList;
    }
#endif

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddCoordinateResultErrorMessages(ContractExecutionStates);
#endif
    }

  }
}

