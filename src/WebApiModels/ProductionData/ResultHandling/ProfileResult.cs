using System;
using System.Collections.Generic;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling
{
  /// <summary>
  /// Base class containing common information relevant to linear and alignment based profile calculations
  /// </summary>
  public abstract class BaseProfile : ContractExecutionResult
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    /// 
    public Guid callId;

    /// <summary>
    /// Was the profile calculation successful?
    /// </summary>
    /// 
    public bool success;

    /// <summary>
    /// The minimum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double minStation;

    /// <summary>
    /// The maximum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double maxStation;

    /// <summary>
    /// The minimum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double minHeight;

    /// <summary>
    /// The maximum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double maxHeight;

    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    /// 
    public double gridDistanceBetweenProfilePoints;
  }

  /// <summary>
  /// The collection of information for a single cell in the computed profile (which may contain many cells) as defined by the collection of cell passes that comprise it.
  /// Values are revelant to the cell as a whole. In the case of cell attributes, event or target information they represent the latest known values
  /// for those items as at the time of the last contributory cell pass in the cell that contained a known-value for the attribute in question.
  /// Composite elevations are elevations that are calculated from a combination of elevation information from production data sourced from TAG files produced
  /// by machine control systems, the elevation information obtained from dated topological surveys (surveyed surfaces).
  /// </summary>
  /// 
  public class ProfileCell
  {
    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell.
    /// </summary>
    /// 
    public double station;

    /// <summary>
    /// Elevation of first cell pass in the profile cell.
    /// </summary>
    /// 
    public float firstPassHeight;

    /// <summary>
    /// Highest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float highestPassHeight;

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lastPassHeight;

    /// <summary>
    /// Lowest elevation recoreded for all cell passes involved in computation of this profile cell.
    /// </summary>
    /// 
    public float lowestPassHeight;

    /// <summary>
    /// First (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float firstCompositeHeight;

    /// <summary>
    /// Highest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float highestCompositeHeight;

    /// <summary>
    /// Last (in time) composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lastCompositeHeight;

    /// <summary>
    /// Lowest composite elevation recorded in the cell
    /// </summary>
    /// 
    public float lowestCompositeHeight;

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    /// 
    public float designHeight;

    /// <summary>
    ///  CMV value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the CMV value was recorded.
    /// </summary>
    /// 
    public float cmvPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the CMV value.
    /// </summary>
    /// 
    public float cmvHeight;


    /// <summary>
    /// The previous valid CMV value
    /// </summary>
    public float previousCmvPercent;

    /// <summary>
    /// MDP value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the MDP value was recorded.
    /// </summary>
    /// 
    public float mdpPercent;

    /// <summary>
    /// Elevation of the cell pass that contributed the MDP value.
    /// </summary>
    /// 
    public float mdpHeight;

    /// <summary>
    /// Temperature value. Value expressed in Celcius.
    /// </summary>
    /// 
    public float temperature;

    /// <summary>
    /// Elevation of the cell pass that contributed the temperature value.
    /// </summary>
    /// 
    public float temperatureHeight;

    /// <summary>
    /// Unknown.
    /// </summary>
    /// 
    public int temperatureLevel;

    /// <summary>
    /// Number of passes contained in the top most layer analysed from the cell passes
    /// </summary>
    /// 
    public int topLayerPassCount;

    /// <summary>
    /// Pass count target application at the time the last cell pass that contributed to the top most layer was recorded
    /// </summary>
    /// 
    public TargetPassCountRange topLayerPassCountTargetRange;

    /// <summary>
    /// Unknown
    /// </summary>
    /// 
    public int passCountIndex;

    /// <summary>
    /// The thickness of the top most layer analysed from the cell passes. Value is expressed in meters.
    /// </summary>
    /// 
    public float topLayerThickness;

    /// <summary>
    /// Determine the minimum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float minHeight
    {
      get
      {
        return lowestPassHeight < lowestCompositeHeight ? lowestPassHeight : lowestCompositeHeight;
      }
    }

    /// <summary>
    /// Determine the maximum elevation of the measured production data elevation or composite elevation
    /// </summary>
    /// 
    public float maxHeight
    {
      get
      {
        return highestPassHeight > highestCompositeHeight ? highestPassHeight : highestCompositeHeight;
      }
    }

      public static ProfileCell HelpSample { get { return new ProfileCell
      {
                                                                  cmvHeight = 10,
                                                                  cmvPercent = 43,
                                                                  station = 11,
                                                                  topLayerPassCount = 15,
                                                                  topLayerPassCountTargetRange = TargetPassCountRange.HelpSample,
                                                                  firstPassHeight = 4,
                                                                  lastCompositeHeight = 6,
                                                                  lastPassHeight = 1,
                                                                  designHeight = 6,
                                                                  firstCompositeHeight = 54,
                                                                  highestCompositeHeight = 123,
                                                                  highestPassHeight = 33,
                                                                  lowestCompositeHeight = 54,
                                                                  lowestPassHeight = 1,
                                                                  mdpHeight = 7,
                                                                  mdpPercent = 33,
                                                                  passCountIndex = 6,
                                                                  temperature = 120,
                                                                  temperatureHeight = 4,
                                                                  temperatureLevel = 5,
                                                                  topLayerThickness = 100
                                                          };}  }

      /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation containing station and last pass elevation information from the profile cell</returns>
    /// 
    public override string ToString()
    {
      return String.Format("Station:{0}, LastPassElevation:{1}", station, lastPassHeight);
    }
  }

  /// <summary>
  /// A point that combines WGS85 latitude and longitude with a station value. This is used as a part of the alignment based profile response 
  /// representation to define the geometry along which the profile computation as made.
  /// </summary>
  /// 
  public class StationLLPoint
  {
    /// <summary>
    /// Station of point. Value is expressed in meters.
    /// </summary>
    /// 
    public double station;

    /// <summary>
    /// Latitude of point. Value is expressed in radians.
    /// </summary>
    /// 
    public double lat;

    /// <summary>
    /// Latitude of point. Value is expressed in radians.
    /// </summary>
    /// 
    public double lng;

      public static StationLLPoint HelpSample { get {return new StationLLPoint {lat = 1.23234, lng = 1.3543, station = 1.1};}  }
  }

  /// <summary>
  /// The representation of a profile computed as a straight line between two points in the cartesian grid coordinate system of the project or
  /// by following a section of an alignment centerline.
  /// </summary>
  /// 
  public class ProfileResult : BaseProfile
  {
    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    /// 
    public List<ProfileCell> cells;

    /// <summary>
    /// A geometrical representation of the profile which defines the actual portion of the line or alignment used for the profile.
    /// </summary>
    /// 
    public List<StationLLPoint> alignmentPoints;

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    public ProfileResult()
    {
      // ...
    }

    /// <summary>
    /// Creates a sample instance of ProfileResult class to be displayed in the Help documentation.
    /// </summary>
    /// 
    public static ProfileResult HelpSample
    {
      get
      {
        return new ProfileResult
        {
          callId = new Guid(),
          success = true,
          minStation = 0,
          maxStation = 100,
          minHeight = 0,
          maxHeight = 212,
          gridDistanceBetweenProfilePoints = 12,
          cells = new List<ProfileCell> { ProfileCell.HelpSample, ProfileCell.HelpSample },
          alignmentPoints = new List<StationLLPoint> { StationLLPoint.HelpSample, StationLLPoint.HelpSample}
        };
      }
    }

  }


}