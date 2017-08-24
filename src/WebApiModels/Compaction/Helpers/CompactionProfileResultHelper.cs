﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class CompactionProfileResultHelper : ICompactionProfileResultHelper
  {
    /// <summary>
    /// Find the cut-fill elevations for the cells from the cut-fill design elevations
    /// </summary>
    /// <param name="slicerProfileResult">The production data profile result with the cells</param>
    /// <param name="slicerDesignResult">The design profile result with the vertices</param>
    public void FindCutFillElevations(CompactionProfileResult<CompactionProfileCell> slicerProfileResult, CompactionProfileResult<CompactionProfileVertex> slicerDesignResult)
    {
      var cells = slicerProfileResult.points;
      if (cells != null && cells.Count > 0)
      {
        int startIndx = -1;
        var vertices = slicerDesignResult.points;
        foreach (var cell in cells)
        {
          startIndx = FindVertexIndex(vertices, cell.station, startIndx);
          if (startIndx != -1)
          {
            //Check for no design elevation
            if (float.IsNaN(vertices[startIndx].elevation) || float.IsNaN(vertices[startIndx + 1].elevation))
            {
              //If the cell station matches (within 3mm) either vertex station
              //then we can use that vertex elevation directly
              const double THREE_MM = 0.003;
              if (Math.Abs(vertices[startIndx].station - cell.station) <= THREE_MM)
              {
                cell.cutFillHeight = vertices[startIndx].elevation;
              }
              else if (Math.Abs(vertices[startIndx+1].station - cell.station) <= THREE_MM)
              {
                cell.cutFillHeight = vertices[startIndx+1].elevation;
              }
            }
            else
            {
              //Calculate elevation by interpolation
              var proportion = (cell.station - vertices[startIndx].station) /
                               (vertices[startIndx + 1].station - vertices[startIndx].station);
              cell.cutFillHeight = (float) (vertices[startIndx].elevation +
                                            proportion *
                                            (vertices[startIndx + 1].elevation - vertices[startIndx].elevation));
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
    /// Convert from one profile representation to another
    /// </summary>
    /// <param name="slicerProfileResult">The profile result to convert from</param>
    /// <returns>The new profile result representation</returns>
    public CompactionProfileResult<CompactionProfileData> ConvertProfileResult(CompactionProfileResult<CompactionProfileCell> slicerProfileResult)
    {
      //shouldn't ever happen but for safety check arg
      if (slicerProfileResult == null || slicerProfileResult.points == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Unexpected missing profile result"));
      }

      var profile = new CompactionProfileResult<CompactionProfileData>
      {
        gridDistanceBetweenProfilePoints = slicerProfileResult.gridDistanceBetweenProfilePoints,
        designFileUid = slicerProfileResult.designFileUid,
        points = new List<CompactionProfileData>
        {
          new CompactionProfileData
          {
            type = "firstPass",
            data = (from p in slicerProfileResult.points
                    select new CompactionDataPoint
                    {
                      type = "firstPass",
                      cellType = p.cellType,
                      x = p.station,
                      y = p.firstPassHeight,
                      value = p.firstPassHeight                     
                    }).ToList()
          },
          new CompactionProfileData
          {
            type = "highestPass",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "highestPass",
                cellType = p.cellType,
                x = p.station,
                y = p.highestPassHeight,
                value = p.highestPassHeight
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "lastPass",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "lastPass",
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.lastPassHeight
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "lowestPass",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "lowestPass",
                cellType = p.cellType,
                x = p.station,
                y = p.lowestPassHeight,
                value = p.lowestPassHeight
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "lastComposite",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "lastComposite",
                cellType = p.cellType,
                x = p.station,
                y = p.lastCompositeHeight,
                value = p.lastCompositeHeight
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "cmvSummary",
            data = (from p in slicerProfileResult.points
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
          new CompactionProfileData
          {
            type = "cmvDetail",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "cmvDetail",
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmv
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "cmvPercentChange",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "cmvPercentChange",
                cellType = p.cellType,
                x = p.station,
                y = p.cmvHeight,
                value = p.cmvPercentChange
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "mdpSummary",
            data = (from p in slicerProfileResult.points
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
          new CompactionProfileData
          {
            type = "temperatureSummary",
            data = (from p in slicerProfileResult.points
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
          new CompactionProfileData
          {
            type = "speedSummary",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "speedSummary",
                cellType = p.cellType,
                x = p.station,
                y = p.speedHeight,
                value = p.minSpeed,
                value2 = p.maxSpeed,
                valueType = p.speedIndex
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "passCountSummary",
            data = (from p in slicerProfileResult.points
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
          new CompactionProfileData
          {
            type = "passCountDetail",
            data = (from p in slicerProfileResult.points
              select new CompactionDataPoint
              {
                type = "passCountDetail",
                cellType = p.cellType,
                x = p.station,
                y = p.lastPassHeight,
                value = p.topLayerPassCount
              }).ToList()
          },
          new CompactionProfileData
          {
            type = "cutFill",
            data = (from p in slicerProfileResult.points
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
  }
}
