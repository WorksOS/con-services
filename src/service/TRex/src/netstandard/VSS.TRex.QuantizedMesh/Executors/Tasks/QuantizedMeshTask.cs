using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.QuantizedMesh.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.QuantizedMesh.Executors.Tasks
{
  /// <summary>
  /// A Task specialized towards rendering quantized mesh tiles
  /// </summary>
  public class QuantizedMeshTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<QuantizedMeshTask>();

    /// <summary>
    /// The action (via a delegate) this task will perform on each of the sub grids transferred to it
    /// </summary>
    public QuantizedMeshTask()
    {
    }

    public GriddedElevDataRow[,] GriddedElevDataArray;
    public double GridIntervalX;
    public double GridIntervalY;
    public int GridSize;
    public double TileMinX;
    public double TileMinY;
    public double TileMaxX;
    public double TileMaxY;
    public float MinElevation = float.PositiveInfinity;
    public float MaxElevation = float.NegativeInfinity;
    public float LowestElevation = 0;
    private int TileRangeMinX;
    private int TileRangeMinY;
    private int TileRangeMaxX;
    private int TileRangeMaxY;

    /// <summary>
    /// Populate result grid from processed subgrid
    /// </summary>
    /// <param name="subGrid"></param>
    private void ExtractRequiredValues(ClientHeightLeafSubGrid subGrid)
    {

      var worldExtents = subGrid.WorldExtents();
      var subGridWorldOriginY = worldExtents.MinY;
      var subGridWorldOriginX = worldExtents.MinX;
      var topX = worldExtents.MaxX; 
      var topY = worldExtents.MaxY;
      float elev;

      // Work out the x/y range across our grid we will lookup values
      double rangeMinX = (subGridWorldOriginX - TileMinX) / GridIntervalX;
      var posX = (int)(Math.Truncate(rangeMinX));
      TileRangeMinX = (posX == (int)rangeMinX) ? (int)rangeMinX : posX + 1;
      if (TileRangeMinX < 0)
        TileRangeMinX = 0;

      double rangeMinY = (subGridWorldOriginY - TileMinY) / GridIntervalY;
      var posY = (int)(Math.Truncate(rangeMinY));
      TileRangeMinY = (posY == (int)rangeMinY) ? (int)rangeMinY : posY + 1;
      if (TileRangeMinY < 0)
        TileRangeMinY = 0;

      double rangeMaxX = (topX - TileMinX) / GridIntervalX;
      TileRangeMaxX = (int)(Math.Truncate(rangeMaxX));
      if (TileRangeMaxX > GridSize - 1)
        TileRangeMaxX = GridSize - 1;

      double rangeMaxY = (topY - TileMinY) / GridIntervalY;
      TileRangeMaxY = (int)(Math.Truncate(rangeMaxY));
      if (TileRangeMaxY > GridSize - 1)
        TileRangeMaxY = GridSize - 1;

      // Iterate over our grid and extract cell heights from subgrid
      for (int y = TileRangeMinY; y <= TileRangeMaxY; y++)
        for (int x = TileRangeMinX; x <= TileRangeMaxX; x++)
        {
          // based on grid position lookup cell value
          // use grid's easting northing value to find subgrid cell we are interested in
          SubGridTree.CalculateIndexOfCellContainingPosition(GriddedElevDataArray[x, y].Easting, GriddedElevDataArray[x, y].Northing,
          subGrid.CellSize,
          subGrid.IndexOriginOffset,
          out int CellX, out int CellY);
          subGrid.GetOTGLeafSubGridCellIndex(CellX, CellY, out var subGridX, out var subGridY);
          // if we have a valid height add it to our data grid
          if (subGrid.Cells[subGridX, subGridY] != CellPassConsts.NullHeight)
          {
            elev = subGrid.Cells[subGridX, subGridY];
            GriddedElevDataArray[x, y].Elevation = elev;
            if (elev < MinElevation)
              MinElevation = elev;
            if (elev > MaxElevation)
              MaxElevation = elev;
          }
        }
    }


    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      bool result = false;

      if (base.TransferResponse(response))
      {
        if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
        {
          Log.LogWarning("No sub grid responses returned");
        }
        else
        {
          foreach (var subGrid in subGridResponses)
          {
            if (subGrid is ClientHeightLeafSubGrid leafSubGrid)
              ExtractRequiredValues(leafSubGrid);
          }
          result = true;
        }
      }

      return result;
    }
  }

}
