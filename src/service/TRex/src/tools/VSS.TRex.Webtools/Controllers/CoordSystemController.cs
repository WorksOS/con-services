using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreXModels;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.TRex.CoordinateSystems.GridFabric.Arguments;
using VSS.TRex.CoordinateSystems.GridFabric.Requests;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;


namespace VSS.TRex.Webtools.Controllers
{
  public class CoordSystemController : ControllerBase
  {
    private const string STR_DATUM_DIRECTION_TO_WGS84 = "Local To WGS84";
    private const string STR_DATUM_DIRECTION_TO_LOCAL = "WGS84 To Local";
    private const string AZIMUTH_STR = "Azimuth";
    private const string NORTH_STR = "North";
    private const string SOUTH_STR = "South";
    private const string EAST_STR = "East";
    private const string WEST_STR = "West";

    /// <summary>
    /// Gets a coordinate system (CS) definition assigned to a TRex's site model/project with a unique identifier.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    [HttpGet("api/projects/{siteModelID}/coordsystem")]
    public JsonResult GetCoordinateSystem([FromRoute] string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out var UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}.</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable.</b>";
        else
        {
          var sw = new Stopwatch();
          sw.Start();

          var csib = siteModel.CSIB();

          if (csib == string.Empty)
            resultToReturn = $"<b>The project does not have Coordinate System definition data. Project UID: {UID}.</b>";
          else
          {
            var coreXWrapper = DIContext.Obtain<ICoreXWrapper>();

            var csd = coreXWrapper.GetCSDFromCSIB(csib);

            if (csd == null || csd.ZoneInfo == null || csd.DatumInfo == null)
              resultToReturn = $"<b>Failed to convert CSIB content to Coordinate System definition data.</b>";
            else
            {
              resultToReturn = $"<b>Coordinate System Settings (in {sw.Elapsed}) :</b><br/>";
              resultToReturn += "<b>================================================</b><br/>";
              resultToReturn += ConvertCSResult(string.Empty, csd);
            }
          }
        }
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Posts a coordinate system (CS) definition file to a TRex's data model/project.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("api/coordsystem")]
    public async Task<JsonResult> PostCoordinateSystem([FromBody] CoordinateSystemFile request)
    {
      string resultToReturn = null;

      var dcFileContentString = System.Text.Encoding.UTF8.GetString(request.CSFileContent, 0, request.CSFileContent.Length);
      var coreXWrapper = DIContext.Obtain<ICoreXWrapper>();

      var csd = coreXWrapper.GetCSDFromDCFileContent(dcFileContentString);

      if (csd == null || csd.ZoneInfo == null || csd.DatumInfo == null)
        resultToReturn = $"<b>Failed to convert DC File {request.CSFileName} content to Coordinate System definition data.</b>";
      else
      {
        var sw = new Stopwatch();
        sw.Start();

        var projectUid = request.ProjectUid ?? Guid.Empty;
        var addCoordinateSystemRequest = new AddCoordinateSystemRequest();

        var csib = coreXWrapper.GetCSIBFromDCFileContent(dcFileContentString);

        var addCoordSystemResponse = await addCoordinateSystemRequest.ExecuteAsync(new AddCoordinateSystemArgument()
        {
          ProjectID = projectUid,
          CSIB = csib
        });

        if (addCoordSystemResponse?.Succeeded ?? false)
        {
          resultToReturn = $"<b>Coordinate System Settings (in {sw.Elapsed}) :</b><br/>";
          resultToReturn += "<b>================================================</b><br/>";
          resultToReturn += ConvertCSResult(request.CSFileName, csd);
        }
      }

      return new JsonResult(resultToReturn);
    }

    private string ConvertCSResult(string fileName, CoordinateSystem coordSystem)
    {
      var azimuthDirection = coordSystem.ZoneInfo.IsSouthAzimuth ? SOUTH_STR : NORTH_STR;

      var latAxis = coordSystem.ZoneInfo.IsSouthGrid ? SOUTH_STR : NORTH_STR;
      var lonAxis = coordSystem.ZoneInfo.IsWestGrid ? WEST_STR : EAST_STR;

      // Coordinate System...
      var resultString = "<b>============= Coordinate System =============</b><br/>";
      resultString += $"<b>Name: </b> {coordSystem.SystemName}<br/>";
      
      if (fileName != string.Empty)
        resultString += $"<b>File Name: </b> {fileName}<br/>";

      resultString += $"<b>Group: </b> {coordSystem.ZoneInfo.ZoneGroupName}<br/>";
      // Ellipsoid...
      resultString += "<b>============= Coordinate System Ellipsoid =============</b><br/>";
      resultString += $"<b>Name: </b> {coordSystem.DatumInfo.EllipseName}<br/>";
      resultString += $"<b>Semi Major Axis: </b> {coordSystem.DatumInfo.EllipseA}<br/>";
      resultString += $"<b>Semi Minor Axis: </b> {0.0}<br/>";
      resultString += $"<b>Flattening: </b> {coordSystem.DatumInfo.EllipseInverseFlat}<br/>";
      resultString += $"<b>First Eccentricity: </b> {0.0}<br/>";
      resultString += $"<b>Second Eccentricity: </b> {0.0}<br/>";
      // Datum...
      resultString += "<b>============= Coordinate System Datum =============</b><br/>";
      resultString += $"<b>Name: </b> {coordSystem.DatumInfo.DatumName}<br/>";
      resultString += $"<b>Transformation Method: </b> {coordSystem.DatumInfo.DatumType}<br/>";
      resultString += $"<b>Latitude Shift Datum Grid File Name: </b> {coordSystem.DatumInfo.LatitudeShiftGridFileName}<br/>";
      resultString += $"<b>Longitude Shift Datum Grid File Name: </b> {coordSystem.DatumInfo.LongitudeShiftGridFileName}<br/>";

      if (coordSystem.DatumInfo.HeightShiftGridFileName != string.Empty)
        resultString += $"<b>Height Shift Datum Grid File Name: </b> {coordSystem.DatumInfo.LongitudeShiftGridFileName}<br/>";
      else
        resultString += "<b>Datum Grid Height Shift is not defined.</b><br/>";

      var direction = coordSystem.DatumInfo.DirectionIsLocalToWGS84 ? STR_DATUM_DIRECTION_TO_WGS84 : STR_DATUM_DIRECTION_TO_LOCAL;
      resultString += $"<b>Direction: </b> {direction}<br/>";

      resultString += $"<b>Translation X: </b> {coordSystem.DatumInfo.TranslationX}<br/>";
      resultString += $"<b>Translation Y: </b> {coordSystem.DatumInfo.TranslationY}<br/>";
      resultString += $"<b>Translation Z: </b> {coordSystem.DatumInfo.TranslationZ}<br/>";
      resultString += $"<b>Rotation X: </b> {coordSystem.DatumInfo.RotationX}<br/>";
      resultString += $"<b>Rotation Y: </b> {coordSystem.DatumInfo.RotationY}<br/>";
      resultString += $"<b>Rotation Z: </b> {coordSystem.DatumInfo.RotationZ}<br/>";
      resultString += $"<b>Scale Factor: </b> {coordSystem.DatumInfo.Scale}<br/>";
      resultString += $"<b>Parameters File Name: </b> {string.Empty}<br/>";
      // Geoid...
      resultString += "<b>============= Coordinate System Geoid =============</b><br/>";
      if (coordSystem.GeoidInfo == null)
        resultString += "<b>                 No Geoid                 </b><br/>";
      else
      {
        resultString += $"<b>Name: </b> {coordSystem.GeoidInfo.GeoidName}<br/>";
        resultString += $"<b>Method: </b> {string.Empty}<br/>";
        resultString += $"<b>File Name: </b> {coordSystem.GeoidInfo.GeoidFileName}<br/>";
      }
      // Projection
      resultString += "<b>============= Coordinate System Projection =============</b><br/>";
      resultString += $"<b>Zone Group Name: </b> {coordSystem.ZoneInfo.ZoneGroupName}<br/>";
      resultString += $"<b>Zone Name: </b> {coordSystem.ZoneInfo.ZoneName}<br/>";
      resultString += $"<b>Azimuth Direction: </b> {AZIMUTH_STR} {azimuthDirection}<br/>";
      resultString += $"<b>Positive Coordinate Direction: </b> {latAxis} {lonAxis}<br/>";
      // Others...
      resultString += "<b>============= Other Properties =============</b><br/>";
      var siteCalibration = coordSystem.ZoneInfo.HorizontalAdjustment != null || coordSystem.ZoneInfo.VerticalAdjustment != null ? "Yes" : "No";
      resultString += $"<b>Site Calibration: </b> {siteCalibration}<br/>";
      resultString += $"<b>Vertical Datum Name: </b> {string.Empty}<br/>";
      resultString += $"<b>Shift Grid Name: </b> {string.Empty}<br/>";
      resultString += $"<b>Snake Grid Name: </b> {string.Empty}<br/>";

      return resultString;
    }
  }
}
