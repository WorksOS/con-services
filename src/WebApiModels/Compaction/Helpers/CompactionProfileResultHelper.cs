using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class CompactionProfileResultHelper : ICompactionProfileResultHelper
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionProfileResultHelper(ILoggerFactory logger)
    {
      log = logger.CreateLogger<CompactionProfileResultHelper>();
    }

    /// <summary>
    /// Find the cut-fill elevations for the cells from the cut-fill design elevations
    /// </summary>
    /// <param name="slicerProfileResult">The production data profile result with the cells</param>
    /// <param name="slicerDesignResult">The design profile result with the vertices</param>
    /// <param name="type">The type of profile to do, either cut-fill or summary volumes</param>
    /// <param name="calcType">The type of summary volumes calculation</param>
    public void FindCutFillElevations(CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult,
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult, string type, VolumeCalcType calcType)
    {
      log.LogDebug($"FindCutFillElevations: {type}");

      if (type != CompactionDataPoint.CUT_FILL && type != CompactionDataPoint.SUMMARY_VOLUMES)
        return;

      if (calcType == VolumeCalcType.GroundToGround)
        return;

      var vertices = slicerDesignResult.results;
      var cells = (from r in slicerProfileResult.results
                   where r.type == type
                   select r).Single().data;
      if (cells != null && cells.Count > 0 && vertices != null && vertices.Count > 0)
      {
        int startIndx = -1;
        foreach (var cell in cells)
        {
          startIndx = FindVertexIndex(vertices, cell.x, startIndx);
          if (startIndx != -1)
          {
            float newy = float.NaN;
            //Check for no design elevation
            if (float.IsNaN(vertices[startIndx].elevation) || float.IsNaN(vertices[startIndx + 1].elevation))
            {
              //If the cell station matches (within 3mm) either vertex station
              //then we can use that vertex elevation directly
              const double THREE_MM = 0.003;
              if (Math.Abs(vertices[startIndx].station - cell.x) <= THREE_MM)
              {
                newy = vertices[startIndx].elevation;
              }
              else if (Math.Abs(vertices[startIndx + 1].station - cell.x) <= THREE_MM)
              {
                newy = vertices[startIndx + 1].elevation;
              }
            }
            else
            {
              //Calculate elevation by interpolation
              var proportion = (cell.x - vertices[startIndx].station) /
                               (vertices[startIndx + 1].station - vertices[startIndx].station);
              newy = (float)(vertices[startIndx].elevation +
                                proportion * (vertices[startIndx + 1].elevation - vertices[startIndx].elevation));
            }
            if (calcType == VolumeCalcType.DesignToGround)
              cell.y = newy;
            else
            {
              cell.y2 = newy;
            }
          }
        }
      }
    }

    /// <summary>
    /// Find the first vertex index for the pair of design profile vertices that the production data cell station lies between.
    /// </summary>
    /// <param name="vertices">The design profile vertices</param>
    /// <param name="cellStation">The cell station</param>
    /// <param name="startIndx">The index from which to start the search</param>
    /// <returns>The first vertex of the pair bracketing the cell, -1 if none</returns>
    private int FindVertexIndex(List<CompactionProfileVertex> vertices, double cellStation, int startIndx)
    {
      //Quick check that the cell station is within design limits
      if (vertices != null && vertices.Count > 0)
      {
        if (vertices[0].station <= cellStation && cellStation <= vertices[vertices.Count - 1].station)
        {
          //Now find the vertices that bracket the cell station
          if (startIndx == -1)
          {
            startIndx = 0;
          }
          for (int i = startIndx; i < vertices.Count - 1; i++)
          {
            if (vertices[i].station <= cellStation && cellStation <= vertices[i + 1].station)
            {
              return i;
            }
          }
        }
      }
      return -1;
    }

    /// <summary>
    /// Convert from one design profile representation to another
    /// </summary>
    /// <param name="slicerProfileResults">The profile result to convert from</param>
    /// <returns>The new profile result representation</returns>
    public CompactionProfileResult<CompactionDesignProfileResult> ConvertProfileResult(
      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults)
    {
      log.LogDebug("ConvertProfileResult: Design profiles");

      //shouldn't ever happen but for safety check arg
      if (slicerProfileResults == null || slicerProfileResults.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Unexpected missing design profile results"));
      }

      //all gridDistanceBetweenProfilePoints are the same if the profile slices the design surface
      var profileWithDistance = slicerProfileResults.Values.Where(v => v.gridDistanceBetweenProfilePoints > 0)
        .FirstOrDefault();
      var distance = profileWithDistance?.gridDistanceBetweenProfilePoints ?? 0;

      var profile = new CompactionProfileResult<CompactionDesignProfileResult>
      {
        gridDistanceBetweenProfilePoints = distance,
        results = (from spr in slicerProfileResults
                   select new CompactionDesignProfileResult
                   {
                     designFileUid = spr.Key,
                     data = spr.Value.results
                   }).ToList()
      };

      return profile;
    }

    /// <summary>
    /// Adds slicer end points to the profile results if not already present
    /// </summary>
    /// <param name="profile">The profile result to check</param>
    public void AddSlicerEndPoints(CompactionProfileResult<CompactionDesignProfileResult> profile)
    {
      //Raptor returns only the vertices on the design surface.
      //Add slicer end points with NaN elevation if not present for a profile.
      if (profile.gridDistanceBetweenProfilePoints > 0)
      {
        foreach (var result in profile.results)
        {
          if (result.data.Count > 0)
          {
            if (result.data[0].station > 0)
            {
              result.data.Insert(0, new CompactionProfileVertex { cellType = ProfileCellType.Gap, station = 0, elevation = float.NaN });
            }
            const double ONE_MM = 0.001;
            if (Math.Abs(result.data[result.data.Count - 1].station - profile.gridDistanceBetweenProfilePoints) > ONE_MM)
            {
              //The start of the gap between the last point and the slicer end point is the end of the actual data.
              result.data[result.data.Count - 1].cellType = ProfileCellType.Gap;
              //Now add the slicer end point.
              result.data.Add(new CompactionProfileVertex { cellType = ProfileCellType.Gap, station = profile.gridDistanceBetweenProfilePoints, elevation = float.NaN });
            }
          }
        }
      }
    }

    /// <summary>
    /// Convert from one production data profile representation to another. The source is a list with each item containing the
    /// data for every profile type. The destination is a list of lists, one list for each profile type containing its own data.
    /// </summary>
    /// <param name="slicerProfileResult">The profile result to convert from</param>
    /// <returns>The new profile result representation</returns>
    public CompactionProfileResult<CompactionProfileDataResult> RearrangeProfileResult(
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult)
    {
      log.LogDebug("ConvertProfileResult: Production data profile");

      //shouldn't ever happen but for safety check arg
      if (slicerProfileResult?.results == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Unexpected missing profile result"));
      }

      var profile = new CompactionProfileResult<CompactionProfileDataResult>
      {
        gridDistanceBetweenProfilePoints = slicerProfileResult.gridDistanceBetweenProfilePoints,
        results = new List<CompactionProfileDataResult>
        {
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.FIRST_PASS,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.FIRST_PASS,
                cellType = p.cellType,
                x = p.station,
                y = p.firstPassHeight,
                value = p.firstPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.HIGHEST_PASS,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.HIGHEST_PASS,
                cellType = p.cellType,
                x = p.station,
                y = p.highestPassHeight,
                value = p.highestPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.LAST_PASS,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.LAST_PASS,
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.lastPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.LOWEST_PASS,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.LOWEST_PASS,
                cellType = p.cellType,
                x = p.station,
                y = p.lowestPassHeight,
                value = p.lowestPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.LAST_COMPOSITE,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.LAST_COMPOSITE,
                cellType = p.cellType,
                x = p.station,
                y = p.lastCompositeHeight,
                value = p.lastCompositeHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.CMV_SUMMARY,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.CMV_SUMMARY,
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmvPercent,
                valueType = p.cmvIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.CMV_DETAIL,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.CMV_DETAIL,
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmv
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.CMV_PERCENT_CHANGE,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.CMV_PERCENT_CHANGE,
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmvPercentChange
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.MDP_SUMMARY,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.MDP_SUMMARY,
                cellType = p.cellType,
                x = p.station,
                y = p.mdpHeight,
                value = p.mdpPercent,
                valueType = p.mdpIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.TEMPERATURE_SUMMARY,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.TEMPERATURE_SUMMARY,
                cellType = p.cellType,
                x = p.station,
                y = p.temperatureHeight,
                value = p.temperature,
                valueType = p.temperatureIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.SPEED_SUMMARY,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.SPEED_SUMMARY,
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.minSpeed,
                value2 = p.maxSpeed,
                valueType = p.speedIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.PASS_COUNT_SUMMARY,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.PASS_COUNT_SUMMARY,
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.topLayerPassCount,
                valueType = p.passCountIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.PASS_COUNT_DETAIL,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.PASS_COUNT_DETAIL,
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.topLayerPassCount
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = CompactionDataPoint.CUT_FILL,
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = CompactionDataPoint.CUT_FILL,
                cellType = p.cellType,
                x = p.station,
                y = p.lastCompositeHeight,
                value = p.cutFill,
                y2 = float.NaN, //will be set later using the cut-fill design
              }).ToList()
          }
        }
      };
      return profile;
    }

    /// <summary>
    /// Convert from one summary volumes profile representation to another. 
    /// </summary>
    /// <param name="slicerProfileResult">The profile result to convert from</param>
    /// <param name="calcType">The type of summary volumes profile</param>
    /// <returns>The new profile result representation</returns>
    public CompactionProfileDataResult RearrangeProfileResult(
      CompactionProfileResult<CompactionSummaryVolumesProfileCell> slicerProfileResult, VolumeCalcType? calcType)
    {
      log.LogDebug("ConvertProfileResult: Summary volumes profile");

      if (slicerProfileResult == null)
        return null;

      return new CompactionProfileDataResult
      {
        type = CompactionDataPoint.SUMMARY_VOLUMES,
        data = (from p in slicerProfileResult.results
                select new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = p.cellType,
                  x = p.station,
                  //y or y2 will be set later using the summary volumes design
                  y = calcType == VolumeCalcType.DesignToGround ? float.NaN : p.lastPassHeight1,
                  value = -p.cutFill,
                  y2 = calcType == VolumeCalcType.GroundToDesign ? float.NaN : p.lastPassHeight2
                }).ToList()
      };
    }

    /// <summary>
    /// The profiles for various types (CMV, temperature, pass count etc.) may have several points
    /// in sequence which have no data which are effectively a single gap. Remove these repeated
    /// points and just keep the start of the gap and the next data point.
    /// </summary>
    /// <param name="result">The profile result to remove the repeated gaps from</param>
    /// <param name="calcType">The type of summary volumes calculation</param>
    public void RemoveRepeatedNoData(CompactionProfileResult<CompactionProfileDataResult> result, VolumeCalcType? calcType)
    {
      log.LogDebug("RemoveRepeatedNoData: Production data profile");

      bool isDesignToGround = calcType.HasValue && calcType == VolumeCalcType.DesignToGround;

      foreach (var profileResult in result.results)
      {
        if (profileResult.data.Count > 0)
        {
          //Identify all the gaps. All data with NaN elevation or value is effectively a gap.
          //The exception is a summary volumes profile that is design to ground where y is NaN as it will be set later using the design. In this case use y2.
          foreach (var point in profileResult.data)
          {
            bool noValue = point.type.StartsWith("passCount") ? point.value == -1 : float.IsNaN(point.value);
            bool noY = point.type == CompactionDataPoint.SUMMARY_VOLUMES && isDesignToGround
              ? point.y2.HasValue && float.IsNaN(point.y2.Value)
              : float.IsNaN(point.y);
            if (noY || noValue)
              point.cellType = ProfileCellType.Gap;
          }

          //Now remove repeated gaps.
          //Always keep first and last points as they are the slicer end points
          CompactionDataPoint prevData = profileResult.data[0];
          bool haveGap = prevData.cellType == ProfileCellType.Gap;
          List<CompactionDataPoint> newList = new List<CompactionDataPoint> { prevData };
          for (int i = 1; i < profileResult.data.Count - 1; i++)
          {
            if (profileResult.data[i].cellType == ProfileCellType.Gap)
            {
              if (!haveGap)
              {
                //This is the start of a gap - keep it
                haveGap = true;
                newList.Add(profileResult.data[i]);
              }
              //else ignore it - repeated gap
            }
            else
            {
              //A data point - keep it
              haveGap = false;
              newList.Add(profileResult.data[i]);
            }
          }
          newList.Add(profileResult.data[profileResult.data.Count - 1]);
          //If the only 2 points are the slicer end points and they have no data then
          //remove them and return an empty list to indicate no profile data at all.
          if (newList.Count == 2 && newList[0].cellType == ProfileCellType.Gap &&
              newList[1].cellType == ProfileCellType.Gap)
          {
            newList.RemoveRange(0, 2);
          }
          profileResult.data = newList;
        }
      }
    }

    /// <summary>
    /// Add mid points between the cell edge intersections. This is because the profile line is plotted using these points.
    /// The cell edges are retained as this is where the color changes on the graph or chart.
    /// </summary>
    /// <param name="profileResult">The profile results from Raptor, one list of cell points for each profile type</param>
    /// <returns>The complete list of interspersed edges and  mid points for each profile type.</returns>
    public void AddMidPoints(CompactionProfileResult<CompactionProfileDataResult> profileResult)
    {
      log.LogDebug("Adding midpoints");
      foreach (var result in profileResult.results)
      {
        log.LogDebug($"Adding midpoints for {result.type}");

        if (result.data.Count >= 4)
        {
          //No mid point for first and last segments since only partial across the cell.
          //We have already added them as mid points.
          var points = new List<CompactionDataPoint> { result.data[0] };

          for (int i = 1; i < result.data.Count - 2; i++)
          {
            points.Add(result.data[i]);
            if (result.data[i].cellType != ProfileCellType.Gap)
            {
              var midPoint = new CompactionDataPoint(result.data[i])
              {
                cellType = ProfileCellType.MidPoint,
                x = result.data[i].x +
                    (result.data[i + 1].x - result.data[i].x) / 2
              };
              points.Add(midPoint);
            }
          }
          points.Add(result.data[result.data.Count - 2]);
          points.Add(result.data[result.data.Count - 1]);
          result.data = points;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"After adding midpoints for {result.type}: {result.data.Count}");
        foreach (var point in result.data)
        {
          sb.Append($",{point.cellType}");
        }
        log.LogDebug(sb.ToString());
      }
    }

    /// <summary>
    /// Since the profile line will be drawn between line segment mid points we need to interpolate the cell edge points to lie on these line segments.
    /// </summary>
    /// <param name="profileResult">The profile containing the list of line segment points, both edges and mid points, for each profile type.</param>
    /// <param name="calcType">The type of summary volumes calculation</param>
    public void InterpolateEdges(CompactionProfileResult<CompactionProfileDataResult> profileResult, VolumeCalcType? calcType)
    {
      log.LogDebug("Interpolating edges");
      foreach (var result in profileResult.results)
      {
        log.LogDebug($"Interpolating edges for {result.type}");

        if (result.data.Count >= 3)
        {
          //First and last points are not gaps or edges. They're always the start and end of the profile line.
          for (int i = 1; i < result.data.Count - 1; i++)
          {
            if (result.data[i].cellType == ProfileCellType.MidPoint)
              continue;

            //No y to interpolate for summary volumes design-ground
            if (result.type != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround)
            {
              InterpolatePoint(i, result.data, result.type, false);
            }

            //y2 to interpolate for only summary volumes ground-ground and design-ground
            if (result.type == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign)
            {
              InterpolatePoint(i, result.data, result.type, true);
            }

          }
        }
        log.LogDebug($"After interpolation for {result.type}");
      }
    }

    /// <summary>
    /// Interpolate the elevation(s) for the given point
    /// </summary>
    /// <param name="i">The index of the point to interpolate</param>
    /// <param name="data">The list of points</param>
    /// <param name="type">The profile type</param>
    /// <param name="useY2">True if interpolating the second elevation</param>
    private void InterpolatePoint(int i, List<CompactionDataPoint> data, string type, bool useY2)
    {
      FindMidPoints(i, data, out int startIndx, out int endIndx, useY2);
      log.LogDebug($"Edge {i}: Midpoints: {startIndx}, {endIndx} for type {type}");
      if (startIndx >= 0 && endIndx <= data.Count - 1)
      {
        InterpolateElevation(data[i], data[startIndx], data[endIndx], useY2);
      }
      //Special case: If all NaN to the LHS try and find 2 mid points to the RHS and extrapolate.
      //This can happen if profile line starts in a gap.
      else if (endIndx < data.Count - 1)
      {
        startIndx = endIndx;
        FindMidPoints(endIndx + 1, data, out _, out int endIndx2, useY2);
        log.LogDebug($"Special Case Start Gap {i}: Midpoints: {startIndx}, {endIndx2} for type {type}");
        if (endIndx2 <= data.Count - 1)
        {
          InterpolateElevation(data[i], data[startIndx], data[endIndx2], useY2);
        }
      }
      //Special case: If all NaN to the RHS try and find 2 mid points to the LHS and extrapolate.
      //This can happen if profile line ends in a gap.
      else if (startIndx > 0)
      {
        endIndx = startIndx;
        FindMidPoints(startIndx - 1, data, out int startIndx2, out int endIndx2, useY2);
        log.LogDebug($"Special Case End Gap {i}: Midpoints: {startIndx}, {endIndx2} for type {type}");
        if (startIndx2 >= 0)
        {
          InterpolateElevation(data[i], data[startIndx2], data[endIndx], useY2);
        }
      }
    }

    /// <summary>
    /// Finds the mid points each side of an edge to use for interpolation
    /// </summary>
    /// <param name="indx">The index of the edge point</param>
    /// <param name="points">The list of points</param>
    /// <param name="startIndx">The index of the mid point before the edge</param>
    /// <param name="endIndx">The index of the mid point after the edge</param>
    /// <param name="useY2">True if checking the second elevation</param>
    private void FindMidPoints(int indx, List<CompactionDataPoint> points, out int startIndx, out int endIndx, bool useY2)
    {
      startIndx = indx;
      bool found = false;
      while (startIndx >= 0 && !found)
      {
        found = MidPointCellHasHeightValue(points[startIndx], useY2);
        if (!found) startIndx--;
      }
      endIndx = indx;
      found = false;
      while (endIndx < points.Count && !found)
      {
        found = MidPointCellHasHeightValue(points[endIndx], useY2);
        if (!found) endIndx++;
      }
    }

    /// <summary>
    /// Determine if the current point is a midpoint and has an elevation.
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <param name="useY2">True if checking the second elevation</param>
    /// <returns>True if the cell has a non-NaN elevation value for the specified height type</returns>
    private bool MidPointCellHasHeightValue(CompactionDataPoint point, bool useY2)
    {
      if (point.cellType == ProfileCellType.MidPoint)
      {
        if (useY2)
        {
          if (point.y2.HasValue)
            return !float.IsNaN(point.y2.Value);
        }
        else
        {
          return !float.IsNaN(point.y);
        }
      }
      return false;
    }

    /// <summary>
    /// Interpolate elevation for the specified point on the line segment from startPoint to endPoint
    /// </summary>
    /// <param name="point">The point to interpolate</param>
    /// <param name="startPoint">The start of the line segment</param>
    /// <param name="endPoint">The end of the line segment</param>
    /// <param name="useY2">True if interpolating the second elevation</param>
    private void InterpolateElevation(CompactionDataPoint point, CompactionDataPoint startPoint, CompactionDataPoint endPoint, bool useY2)
    {
      var proportion = (point.x - startPoint.x) / (endPoint.x - startPoint.x);
      if (useY2)
      {
        point.y2 = InterpolateElevation(proportion, startPoint.y2 ?? float.NaN, endPoint.y2 ?? float.NaN);
      }
      else
      {
        point.y = InterpolateElevation(proportion, startPoint.y, endPoint.y);
      }
      log.LogDebug($"Interpolated station {point.x} of cell type {point.cellType} for type {point.type}");
    }

    /// <summary>
    /// Interpolate an elevation
    /// </summary>
    /// <param name="proportion">The proportion of the elevation to use</param>
    /// <param name="startElevation">The elevation at the start of the line segment to be used for interpolation</param>
    /// <param name="endElevation">The elevation at the end of the line segment to be used for interpolation</param>
    /// <returns></returns>
    private float InterpolateElevation(double proportion, float startElevation, float endElevation)
    {
      //Check for no elevation data before trying to interpolate
      return float.IsNaN(startElevation) || float.IsNaN(endElevation) ? float.NaN :
        startElevation + (float)proportion * (endElevation - startElevation);
    }


  }
}
