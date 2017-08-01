using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VLPDDecls;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;
using VSS.Velociraptor.PDSInterface;
using ProfileCell = VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling.ProfileCell;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  public class ProfilesHelper
  {
    private const double NO_HEIGHT = 1E9;
    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;
    private const int NO_MDP = SVOICDecls.__Global.kICNullMDPValue;
    private const int NO_TEMPERATURE = SVOICDecls.__Global.kICNullMaterialTempValue;
    private const int NO_PASSCOUNT = SVOICDecls.__Global.kICNullPassCountValue;
    private const float NULL_SINGLE = DTXModelDecls.__Global.NullSingle;

    public static ProfileResult convertProductionDataProfileResult(MemoryStream ms, Guid callID)
    {
      List<StationLLPoint> points = null;

      ProfileResult profile = new ProfileResult();
      profile.callId = callID;
      profile.cells = null;
      profile.success = ms != null;

      if (profile.success)
      {
        PDSProfile pdsiProfile = new PDSProfile();

        TICProfileCellListPackager packager = new TICProfileCellListPackager();
        packager.CellList = new TICProfileCellList();
        packager.ReadFromStream(ms);

        pdsiProfile.Assign(packager.CellList);
        pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;


        if (packager.LatLongList != null) // For an alignment profile we return the lat long list to draw the profile line. slicer tool will just be a zero count
        {
          //points = packager.LatLongList.ToList().ConvertAll<Flex.StationLLPoint>(delegate(TWGS84StationPoint p) { return new Flex.StationLLPoint {lat = p.Lat * 180 / Math.PI, lng = p.Lon * 180 / Math.PI }; });
          points = packager.LatLongList.ToList().ConvertAll<StationLLPoint>
            (delegate(TWGS84StationPoint p)
            {
              return new StationLLPoint { station = p.Station, lat = p.Lat * 180 / Math.PI, lng = p.Lon * 180 / Math.PI };
            }
            );
        }

        profile.cells = new List<ProfileCell>();
        VSS.Velociraptor.PDSInterface.ProfileCell prevCell = null;
        foreach (VSS.Velociraptor.PDSInterface.ProfileCell currCell in pdsiProfile.cells)
        {
          double prevStationIntercept = prevCell == null ? 0.0 : prevCell.station + prevCell.interceptLength;
          bool gap = prevCell == null
                       ? false
                       : Math.Abs(currCell.station - prevStationIntercept) > 0.001;
          if (gap)
          {
            profile.cells.Add(new ProfileCell()
            {
              station = prevStationIntercept,
              firstPassHeight = float.NaN,
              highestPassHeight = float.NaN,
              lastPassHeight = float.NaN,
              lowestPassHeight = float.NaN,
              firstCompositeHeight = float.NaN,
              highestCompositeHeight = float.NaN,
              lastCompositeHeight = float.NaN,
              lowestCompositeHeight = float.NaN,
              designHeight = float.NaN,
              cmvPercent = float.NaN,
              cmvHeight = float.NaN,
              previousCmvPercent = float.NaN,
              mdpPercent = float.NaN,
              mdpHeight = float.NaN,
              temperature = float.NaN,
              temperatureHeight = float.NaN,
              temperatureLevel = -1,
              topLayerPassCount = -1,
              topLayerPassCountTargetRange = TargetPassCountRange.CreateTargetPassCountRange(NO_PASSCOUNT, NO_PASSCOUNT),
              passCountIndex = -1,
              topLayerThickness = float.NaN
            });
          }

          bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == NO_CCV || currCell.CCV == NO_CCV;
          bool noCCElevation = currCell.CCVElev == NULL_SINGLE || noCCVValue;
          bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == NO_MDP || currCell.MDP == NO_MDP;
          bool noMDPElevation = currCell.MDPElev == NULL_SINGLE || noMDPValue;
          bool noTemperatureValue = currCell.materialTemperature == NO_TEMPERATURE;
          bool noTemperatureElevation = currCell.materialTemperatureElev == NULL_SINGLE || noTemperatureValue;
          bool noPassCountValue = currCell.topLayerPassCount == NO_PASSCOUNT;
          //bool noPassCountTargetValue = currCell.topLayerPassCountTargetRange.Min == NO_PASSCOUNT || currCell.topLayerPassCountTargetRange.Max == NO_PASSCOUNT;

          profile.cells.Add(new ProfileCell()
          {
            station = currCell.station,

            firstPassHeight = currCell.firstPassHeight == NULL_SINGLE ? float.NaN : currCell.firstPassHeight,
            highestPassHeight = currCell.highestPassHeight == NULL_SINGLE ? float.NaN : currCell.highestPassHeight,
            lastPassHeight = currCell.lastPassHeight == NULL_SINGLE ? float.NaN : currCell.lastPassHeight,
            lowestPassHeight = currCell.lowestPassHeight == NULL_SINGLE ? float.NaN : currCell.lowestPassHeight,

            firstCompositeHeight = currCell.compositeFirstPassHeight == NULL_SINGLE ? float.NaN : currCell.compositeFirstPassHeight,
            highestCompositeHeight = currCell.compositeHighestPassHeight == NULL_SINGLE ? float.NaN : currCell.compositeHighestPassHeight,
            lastCompositeHeight = currCell.compositeLastPassHeight == NULL_SINGLE ? float.NaN : currCell.compositeLastPassHeight,
            lowestCompositeHeight = currCell.compositeLowestPassHeight == NULL_SINGLE ? float.NaN : currCell.compositeLowestPassHeight,
            designHeight = currCell.designHeight == NULL_SINGLE ? float.NaN : currCell.designHeight,

            cmvPercent = noCCVValue
              ? float.NaN : (float)currCell.CCV / (float)currCell.TargetCCV * 100.0F,
            cmvHeight = noCCElevation ? float.NaN : currCell.CCVElev,
            previousCmvPercent = noCCVValue
              ? float.NaN : (float)currCell.PrevCCV / (float)currCell.PrevTargetCCV * 100.0F,   

            mdpPercent = noMDPValue
              ? float.NaN : (float)currCell.MDP / (float)currCell.TargetMDP * 100.0F,
            mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev,

            temperature = noTemperatureValue ? float.NaN : currCell.materialTemperature / 10.0F,// As temperature is reported in 10th...
            temperatureHeight = noTemperatureElevation ? float.NaN : currCell.materialTemperatureElev,
            temperatureLevel = noTemperatureValue ? -1 :
                (currCell.materialTemperature < currCell.materialTemperatureWarnMin ? 2 :
                (currCell.materialTemperature > currCell.materialTemperatureWarnMax ? 0 : 1)),

            topLayerPassCount = noPassCountValue ? -1 : currCell.topLayerPassCount,
            topLayerPassCountTargetRange = TargetPassCountRange.CreateTargetPassCountRange(currCell.topLayerPassCountTargetRange.Min, currCell.topLayerPassCountTargetRange.Max),

            passCountIndex = noPassCountValue ? -1 :
                (currCell.topLayerPassCount < currCell.topLayerPassCountTargetRange.Min ? 2 :
                (currCell.topLayerPassCount > currCell.topLayerPassCountTargetRange.Max ? 0 : 1)),

            topLayerThickness = currCell.topLayerThickness == NULL_SINGLE ? float.NaN : currCell.topLayerThickness
          });

          prevCell = currCell;
        }
        //Add a last point at the intercept length of the last cell so profiles are drawn correctly
        if (prevCell != null)
        {
          ProfileCell lastCell = profile.cells[profile.cells.Count - 1];
          profile.cells.Add(new ProfileCell()
          {
            station = prevCell.station + prevCell.interceptLength,
            firstPassHeight = lastCell.firstPassHeight,
            highestPassHeight = lastCell.highestPassHeight,
            lastPassHeight = lastCell.lastPassHeight,
            lowestPassHeight = lastCell.lowestPassHeight,
            firstCompositeHeight = lastCell.firstCompositeHeight,
            highestCompositeHeight = lastCell.highestCompositeHeight,
            lastCompositeHeight = lastCell.lastCompositeHeight,
            lowestCompositeHeight = lastCell.lowestCompositeHeight,
            designHeight = lastCell.designHeight,
            cmvPercent = lastCell.cmvPercent,
            cmvHeight = lastCell.cmvHeight,
            mdpPercent = lastCell.mdpPercent,
            mdpHeight = lastCell.mdpHeight,
            temperature = lastCell.temperature,
            temperatureHeight = lastCell.temperatureHeight,
            temperatureLevel = lastCell.temperatureLevel,
            topLayerPassCount = lastCell.topLayerPassCount,
            topLayerPassCountTargetRange = lastCell.topLayerPassCountTargetRange,
            passCountIndex = lastCell.passCountIndex,
            topLayerThickness = lastCell.topLayerThickness,
            previousCmvPercent = lastCell.previousCmvPercent
          });
        }
        ms.Close();

        profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;
      }

      //Check for no production data at all - Raptor may return cells but all heights will be NaN
      bool gotData = profile.cells != null && profile.cells.Count > 0;
      var heights = gotData ? (from c in profile.cells where !float.IsNaN(c.firstPassHeight) select c).ToList() : null;
      if (heights != null && heights.Count > 0)
      {
        profile.minStation = profile.cells.Min(c => c.station);
        profile.maxStation = profile.cells.Max(c => c.station);
        profile.minHeight = heights.Min(h => h.minHeight);
        profile.maxHeight = heights.Max(h => h.maxHeight);
        profile.alignmentPoints = points;
      }
      else
      {
        profile.minStation = double.NaN;
        profile.maxStation = double.NaN;
        profile.minHeight = double.NaN;
        profile.maxHeight = double.NaN;
        profile.alignmentPoints = points;
      }

      return profile;
    }

    public static void convertProfileEndPositions(ProfileGridPoints gridPoints, ProfileLLPoints lLPoints,
                                                 out VLPDDecls.TWGS84Point startPt, out VLPDDecls.TWGS84Point endPt,
                                                 out bool positionsAreGrid)
    {
      if (gridPoints != null)
      {
        positionsAreGrid = true;
        startPt = new VLPDDecls.TWGS84Point() { Lat = gridPoints.y1, Lon = gridPoints.x1 };
        endPt = new VLPDDecls.TWGS84Point() { Lat = gridPoints.y2, Lon = gridPoints.x2 };
      }
      else
        if (lLPoints != null)
        {
          positionsAreGrid = false;
          startPt = new VLPDDecls.TWGS84Point() { Lat = lLPoints.lat1, Lon = lLPoints.lon1 };
          endPt = new VLPDDecls.TWGS84Point() { Lat = lLPoints.lat2, Lon = lLPoints.lon2 };
        }
        else
        {

          startPt = new VLPDDecls.TWGS84Point();
          endPt = new VLPDDecls.TWGS84Point();
          positionsAreGrid = false;

          // TODO throw an exception
        }
    }
  }
}