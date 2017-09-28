using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
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
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger"></param>
    public CompactionProfileResultHelper(ILoggerFactory logger)
    {
      this.logger = logger;
      log = logger.CreateLogger<CompactionProfileResultHelper>();
    }

    /// <summary>
    /// Find the cut-fill elevations for the cells from the cut-fill design elevations
    /// </summary>
    /// <param name="slicerProfileResult">The production data profile result with the cells</param>
    /// <param name="slicerDesignResult">The design profile result with the vertices</param>
    public void FindCutFillElevations(CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult,
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult)
    {
      log.LogDebug("FindCutFillElevations: ");

      var vertices = slicerDesignResult.results;
      var cells = (from r in slicerProfileResult.results
        where r.type == "cutFill"
        select r).Single().data;
      if (cells != null && cells.Count > 0 && vertices != null && vertices.Count > 0)
      {
        int startIndx = -1;
        foreach (var cell in cells)
        {
          startIndx = FindVertexIndex(vertices, cell.x, startIndx);
          if (startIndx != -1)
          {
            //Check for no design elevation
            if (float.IsNaN(vertices[startIndx].elevation) || float.IsNaN(vertices[startIndx + 1].elevation))
            {
              //If the cell station matches (within 3mm) either vertex station
              //then we can use that vertex elevation directly
              const double THREE_MM = 0.003;
              if (Math.Abs(vertices[startIndx].station - cell.x) <= THREE_MM)
              {
                cell.y2 = vertices[startIndx].elevation;
              }
              else if (Math.Abs(vertices[startIndx + 1].station - cell.x) <= THREE_MM)
              {
                cell.y2 = vertices[startIndx + 1].elevation;
              }
            }
            else
            {
              //Calculate elevation by interpolation
              var proportion = (cell.x - vertices[startIndx].station) /
                               (vertices[startIndx + 1].station - vertices[startIndx].station);
              cell.y2 = (float) (vertices[startIndx].elevation +
                                proportion * (vertices[startIndx + 1].elevation - vertices[startIndx].elevation));
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
      var distance = profileWithDistance != null ? profileWithDistance.gridDistanceBetweenProfilePoints : 0;

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
            if (result.data[result.data.Count - 1].station < profile.gridDistanceBetweenProfilePoints)
            {
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
      if (slicerProfileResult == null || slicerProfileResult.results == null)
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
            type = "firstPass",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "firstPass",
                cellType = p.cellType,
                x = p.station,
                y = p.firstPassHeight,
                value = p.firstPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "highestPass",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "highestPass",
                cellType = p.cellType,
                x = p.station,
                y = p.highestPassHeight,
                value = p.highestPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "lastPass",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "lastPass",
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.lastPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "lowestPass",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "lowestPass",
                cellType = p.cellType,
                x = p.station,
                y = p.lowestPassHeight,
                value = p.lowestPassHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "lastComposite",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "lastComposite",
                cellType = p.cellType,
                x = p.station,
                y = p.lastCompositeHeight,
                value = p.lastCompositeHeight
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "cmvSummary",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "cmvSummary",
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmvPercent,
                valueType = p.cmvIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "cmvDetail",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "cmvDetail",
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmv
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "cmvPercentChange",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "cmvPercentChange",
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmvPercentChange
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "mdpSummary",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "mdpSummary",
                cellType = p.cellType,
                x = p.station,
                y = p.mdpHeight,
                value = p.mdpPercent,
                valueType = p.mdpIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "temperatureSummary",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "temperatureSummary",
                cellType = p.cellType,
                x = p.station,
                y = p.temperatureHeight,
                value = p.temperature,
                valueType = p.temperatureIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "speedSummary",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "speedSummary",
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
            type = "passCountSummary",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "passCountSummary",
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.topLayerPassCount,
                valueType = p.passCountIndex
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "passCountDetail",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "passCountDetail",
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.topLayerPassCount
              }).ToList()
          },
          new CompactionProfileDataResult
          {
            type = "cutFill",
            data = (from p in slicerProfileResult.results
              select new CompactionDataPoint
              {
                type = "cutFill",
                cellType = p.cellType,
                x = p.station,
                y = p.lastCompositeHeight,
                value = p.cutFill,
                y2 = p.cutFillHeight
              }).ToList()
          }
        }
      };
      return profile;
    }


    /// <summary>
    /// The profiles for various types (CMV, temperature, pass count etc.) may have several points
    /// in sequence which have no data which are effectively a single gap. Remove these repeated
    /// points and just keep the start of the gap and the next data point.
    /// </summary>
    /// <param name="result">The profile result to remove the repeated gaps from</param>
    public void RemoveRepeatedNoData(CompactionProfileResult<CompactionProfileDataResult> result)
    {
      log.LogDebug("RemoveRepeatedNoData: Production data profile");

      foreach (var profileResult in result.results)
      {
        //Identify all the gaps.
        //All data with NaN elevation or value is effectively a gap
        foreach (var point in profileResult.data)
        {
          bool noValue = point.type.StartsWith("passCount") ? point.value == -1 : float.IsNaN(point.value);
          if (float.IsNaN(point.y) || noValue)
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
        if (newList.Count == 2 && newList[0].cellType == ProfileCellType.Gap && newList[1].cellType == ProfileCellType.Gap)
        {
          newList.RemoveRange(0, 2);
        }
        profileResult.data = newList;
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
          var points = new List<CompactionDataPoint>();

          points.Add(result.data[0]);
          for (int i = 1; i < result.data.Count - 2; i++)
          {
            points.Add(result.data[i]);
            if (result.data[i].cellType != ProfileCellType.Gap)
            {
              var midPoint = new CompactionDataPoint(result.data[i]);
              midPoint.cellType = ProfileCellType.MidPoint;
              midPoint.x = result.data[i].x +
                           (result.data[i + 1].x - result.data[i].x) / 2;
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
    public void InterpolateEdges(CompactionProfileResult<CompactionProfileDataResult> profileResult)
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

            int startIndx, endIndx;
            FindMidPoints(i, result.data, out startIndx, out endIndx);
            log.LogDebug($"Edge {i}: Midpoints: {startIndx}, {endIndx} for type {result.type}");
            if (startIndx >= 0 && endIndx <= result.data.Count - 1)
            {
              InterpolateElevation(result.data[i], result.data[startIndx], result.data[endIndx]);
            }
            //Special case: If all NaN to the LHS try and find 2 mid points to the RHS and extrapolate.
            //This can happen if profile line starts in a gap.
            else if (endIndx < result.data.Count - 1)
            {
              startIndx = endIndx;
              int startIndx2, endIndx2;
              FindMidPoints(endIndx + 1, result.data, out startIndx2, out endIndx2);
              log.LogDebug($"Special Case Start Gap {i}: Midpoints: {startIndx}, {endIndx2} for type {result.type}");
              if (endIndx2 <= result.data.Count - 1)
              {
                InterpolateElevation(result.data[i], result.data[startIndx], result.data[endIndx2]);
              }
            }
            //Special case: If all NaN to the RHS try and find 2 mid points to the LHS and extrapolate.
            //This can happen if profile line ends in a gap.
            else if (startIndx > 0)
            {
              endIndx = startIndx;
              int startIndx2, endIndx2;
              FindMidPoints(startIndx - 1, result.data, out startIndx2, out endIndx2);
              log.LogDebug($"Special Case End Gap {i}: Midpoints: {startIndx}, {endIndx2} for type {result.type}");
              if (startIndx2 >= 0)
              {
                InterpolateElevation(result.data[i], result.data[startIndx2], result.data[endIndx]);
              }
            }

          }
        }
        log.LogDebug($"After interpolation for {result.type}");
      }
    }

    /// <summary>
    /// Finds the mid points each side of an edge to use for interpolation
    /// </summary>
    /// <param name="indx">The index of the edge point</param>
    /// <param name="points">The list of points</param>
    /// <param name="startIndx">The index of the mid point before the edge</param>
    /// <param name="endIndx">The index of the mid point after the edge</param>
    private void FindMidPoints(int indx, List<CompactionDataPoint> points, out int startIndx, out int endIndx)
    {
      startIndx = indx;
      bool found = false;
      while (startIndx >= 0 && !found)
      {
        found = MidPointCellHasHeightValue(points[startIndx]);
        if (!found) startIndx--;
      }
      endIndx = indx;
      found = false;
      while (endIndx < points.Count && !found)
      {
        found = MidPointCellHasHeightValue(points[endIndx]);
        if (!found) endIndx++;
      }
    }

    /// <summary>
    /// Determine if the current point is a midpoint and has an elevation.
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <returns>True if the cell has a non-NaN elevation value for the specified height type</returns>
    private bool MidPointCellHasHeightValue(CompactionDataPoint point)
    {
      return point.cellType == ProfileCellType.MidPoint ? !float.IsNaN(point.y) : false;
    }

    /// <summary>
    /// Interpolate elevation for the specified point on the line segment from startPoint to endPoint
    /// </summary>
    /// <param name="point">The point to interpolate</param>
    /// <param name="startPoint">The start of the line segment</param>
    /// <param name="endPoint">The end of the line segment</param>
    private void InterpolateElevation(CompactionDataPoint point, CompactionDataPoint startPoint, CompactionDataPoint endPoint)
    {
      var proportion = (point.x - startPoint.x) / (endPoint.x - startPoint.x);
      point.y = InterpolateElevation(proportion, startPoint.y, endPoint.y);
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
