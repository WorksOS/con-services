using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation sub grids
  /// </summary>
  public static class GridRotationUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("GridRotationUtilities");

    /// <summary>
    /// Computes a bitmask used to sieve out only the cells that will be used in the query context.
    /// The sieved cells are the only cells processed and returned. All other cells will be null values,
    /// even if data is present for them that matches filtering and other conditions
    /// </summary>
    /// <param name="subGridMoniker"></param>
    /// <param name="areaControlSet"></param>
    /// <param name="siteModelCellSize"></param>
    /// <param name="sieveBitmask"></param>
    /// <param name="subGridWorldOriginX"></param>
    /// <param name="subGridWorldOriginY"></param>
    /// <returns></returns>
    public static bool ComputeSieveBitmaskInteger(double subGridWorldOriginX, double subGridWorldOriginY, string subGridMoniker, 
      AreaControlSet areaControlSet, double siteModelCellSize, out SubGridTreeBitmapSubGridBits sieveBitmask)
    {
      const int kMaxStepSize = 10000;
      sieveBitmask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      /* TODO - add configuration item for VLPDPSNode_UseSkipStepComputationForWMSSubGridRequests
      if (!VLPDSvcLocations.VLPDPSNode_UseSkipStepComputationForWMSSubGridRequests)
          return false;
      */

      if (areaControlSet.PixelXWorldSize == 0 || areaControlSet.PixelYWorldSize == 0)
        return false;

      // Progress through the cells in the grid, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right)

      ///////////////// CalculateParameters;  START

      double stepsPerPixelX = areaControlSet.PixelXWorldSize / siteModelCellSize;
      double stepsPerPixelY = areaControlSet.PixelYWorldSize / siteModelCellSize;

      // Note: integers
      int stepX = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(stepsPerPixelX)));
      int stepY = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(stepsPerPixelY)));

      double stepXIncrement = stepX * siteModelCellSize;
      double stepYIncrement = stepY * siteModelCellSize;

      double stepXIncrementOverTwo = stepXIncrement / 2;
      double stepYIncrementOverTwo = stepYIncrement / 2;

      ///////////////// CalculateParameters;  END

      if (stepX < 2 && stepY < 2)
        return false;

      if (stepX >= SubGridTreeConsts.SubGridTreeDimension && stepY >= SubGridTreeConsts.SubGridTreeDimension)
        Log.LogDebug($"Skip value of {stepX}/{stepY} chosen for {subGridMoniker}");

      sieveBitmask.Clear();

      // Calculate the world coordinate location of the origin (bottom left corner) of this sub grid
      //subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

      // Skip-Iterate through the cells marking those cells that require values
      // calculate for them in the bitmask

      double temp = subGridWorldOriginY / stepYIncrement;
      double currentNorth = (Math.Truncate(temp) * stepYIncrement) - stepYIncrementOverTwo;
      int northRow = (int)Math.Floor((currentNorth - subGridWorldOriginY) / siteModelCellSize);

      while (northRow < 0)
        northRow += stepY;

      while (northRow < SubGridTreeConsts.SubGridTreeDimension)
      {
        temp = subGridWorldOriginX / stepXIncrement;

        double currentEast = (Math.Truncate(temp) * stepXIncrement) + stepXIncrementOverTwo;
        int eastCol = (int)Math.Floor((currentEast - subGridWorldOriginX) / siteModelCellSize);

        while (eastCol < 0)
          eastCol += stepX;

        while (eastCol < SubGridTreeConsts.SubGridTreeDimension)
        {
          sieveBitmask.SetBit(eastCol, northRow);
          eastCol += stepX;
        }

        northRow += stepY;
      }

      return true;
    }

    /// <param name="subGridWorldOriginY"></param>
    /// <param name="subGridMoniker"></param
    /// <param name="areaControlSet"></param>
    /// <param name="siteModelCellSize"></param>
    /// <param name="assignmentContext"></param>
    /// <param name="sieveBitmask"></param>
    /// <param name="subGridWorldOriginX"></param>
    /// <returns></returns>
    public static bool ComputeSieveBitmaskFloat(double subGridWorldOriginX, double subGridWorldOriginY, 
      AreaControlSet areaControlSet, double siteModelCellSize, FilteredValueAssignmentContext assignmentContext, out SubGridTreeBitmapSubGridBits sieveBitmask)
    {
      sieveBitmask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      if (areaControlSet.PixelXWorldSize == 0 || areaControlSet.PixelYWorldSize == 0)
        return false;

      if (areaControlSet.PixelXWorldSize < siteModelCellSize && areaControlSet.PixelYWorldSize < siteModelCellSize)
        return false;

      // Progress through the cells in the grid, starting from the southern most
      // row in the grid and progressing from the western end to the eastern end
      // (ie: bottom to top, left to right), taking into account grid offsets and
      // rotations specified in areaControlSet

      sieveBitmask.Clear();

      // Calculate the world coordinate location of the origin (bottom left corner)
      // and limits (top right corner) of this sub grid
      double subGridWorldLimitX = subGridWorldOriginX + (SubGridTreeConsts.SubGridTreeDimension * siteModelCellSize);
      double subGridWorldLimitY = subGridWorldOriginY + (SubGridTreeConsts.SubGridTreeDimension * siteModelCellSize);

      // Calculate the parameter to control skipping across a rotated grid with respect to
      // a grid projection north oriented sub grid
      InitialiseRotationAndBounds(areaControlSet,
        subGridWorldOriginX, subGridWorldOriginY, subGridWorldLimitX, subGridWorldLimitY,
        out int numRowsToScan, out int numColsToScan,
        out double stepNorthX, out double stepNorthY, out double stepEastX, out double stepEastY,
        out double firstScanPointEast, out double firstScanPointNorth);

      // Perform the walk across all probed locations determining the cells we want to
      // obtain values for and the probe locations.
      PerformScan(siteModelCellSize, assignmentContext, sieveBitmask,
        numRowsToScan, numColsToScan,
        stepNorthX, stepNorthY, stepEastX, stepEastY,
        subGridWorldOriginX, subGridWorldOriginY,
        firstScanPointEast, firstScanPointNorth);

      return true;
    }

    private static void InitialiseRotationAndBounds(AreaControlSet areaControlSet,
      double subGridMinX, double subGridMinY, double subGridMaxX, double subGridMaxY,
      out int numRowsToScan, out int numColsToScan,
      out double stepNorthX, out double stepNorthY, out double stepEastX, out double stepEastY,
      out double firstScanPointEast, out double firstScanPointNorth)
    {
      double stepX = areaControlSet.PixelXWorldSize;
      double stepY = areaControlSet.PixelYWorldSize;

      // Take into account the effect of having to have a grid probe position at
      // the 'first point' defined in areaControlSet
      // Calculate the intra-interval offset that needs to be applied to align the
      // skip-stepping to that modified grid search
      double intraGridOffsetX = areaControlSet.UserOriginX - (Math.Floor(areaControlSet.UserOriginX / stepX) * stepX);
      double intraGridOffsetY = areaControlSet.UserOriginY - (Math.Floor(areaControlSet.UserOriginY / stepY) * stepY);

      if (areaControlSet.Rotation != Consts.NullDouble && areaControlSet.Rotation != 0) // Radians, north azimuth survey angle
      {
        Fence rotatedSubGridBoundary = new Fence();

        // Create the rotated boundary by 'un-rotating' the sub grid world extents into a context
        // where the grid is itself not rotated
        GeometryHelper.RotatePointAbout(areaControlSet.Rotation, subGridMinX, subGridMinY, out double x, out double y, areaControlSet.UserOriginX, areaControlSet.UserOriginY);
        rotatedSubGridBoundary.Points.Add(new FencePoint(x, y));
        GeometryHelper.RotatePointAbout(areaControlSet.Rotation, subGridMinX, subGridMaxY, out x, out y, areaControlSet.UserOriginX, areaControlSet.UserOriginY);
        rotatedSubGridBoundary.Points.Add(new FencePoint(x, y));
        GeometryHelper.RotatePointAbout(areaControlSet.Rotation, subGridMaxX, subGridMaxY, out x, out y, areaControlSet.UserOriginX, areaControlSet.UserOriginY);
        rotatedSubGridBoundary.Points.Add(new FencePoint(x, y));
        GeometryHelper.RotatePointAbout(areaControlSet.Rotation, subGridMaxX, subGridMinY, out x, out y, areaControlSet.UserOriginX, areaControlSet.UserOriginY);
        rotatedSubGridBoundary.Points.Add(new FencePoint(x, y));

        rotatedSubGridBoundary.UpdateExtents();
        firstScanPointEast = Math.Truncate(rotatedSubGridBoundary.MinX / stepX) * stepX + intraGridOffsetX;
        firstScanPointNorth = Math.Truncate(rotatedSubGridBoundary.MinY / stepY) * stepY + intraGridOffsetY;

        numRowsToScan = (int)Math.Ceiling((rotatedSubGridBoundary.MaxY - firstScanPointNorth) / stepY) + 1;
        numColsToScan = (int)Math.Ceiling((rotatedSubGridBoundary.MaxX - firstScanPointEast) / stepX) + 1;

        // Rotate the first scan point back to the context of the grid projection north oriented
        // sub grid world extents
        GeometryHelper.RotatePointAbout(-areaControlSet.Rotation, firstScanPointEast, firstScanPointNorth, out firstScanPointEast,
          out firstScanPointNorth, areaControlSet.UserOriginX, areaControlSet.UserOriginY);

        // Perform a 'unit' rotation of the StepX and StepY quantities about the
        // origin to define step quantities that orient the vector of probe position movement
        // to the rotated probe grid
        double sinOfRotation = Math.Sin(areaControlSet.Rotation);
        double cosOfRotation = Math.Cos(areaControlSet.Rotation);

        stepNorthY = cosOfRotation * stepY;
        stepNorthX = sinOfRotation * stepX;
        stepEastX = cosOfRotation * stepX;
        stepEastY = -sinOfRotation * stepY;
      }
      else
      {
        firstScanPointEast = Math.Truncate(subGridMinX / stepX) * stepX + intraGridOffsetX;
        firstScanPointNorth = Math.Truncate(subGridMinY / stepY) * stepY + intraGridOffsetY;

        numRowsToScan = (int)Math.Ceiling((subGridMaxY - firstScanPointNorth) / stepY) + 1;
        numColsToScan = (int)Math.Ceiling((subGridMaxX - firstScanPointEast) / stepX) + 1;

        stepNorthX = 0;
        stepNorthY = stepY;
        stepEastX = stepX;
        stepEastY = 0;
      }
    }

    private static void PerformScan(double siteModelCellSize, FilteredValueAssignmentContext assignmentContext, 
      SubGridTreeBitmapSubGridBits sieveBitmask,
      int numRowsToScan, int numColsToScan,
      double stepNorthX, double stepNorthY, double stepEastX, double stepEastY,
      double subGridMinX, double subGridMinY,
      double firstScanPointEast, double firstScanPointNorth)
    {
      // Skip-Iterate through the cells marking those cells that require values
      // calculate for them in the bitmask. Also record the actual probe locations
      // that determined the cells to be processed.

      for (int I = 0; I < numRowsToScan; I++)
      {
        double currentNorth = firstScanPointNorth + I * stepNorthY;
        double currentEast = firstScanPointEast + I * stepNorthX; 

        for (int J = 0; J < numColsToScan; J++)
        {
          int eastCol = (int)Math.Floor((currentEast - subGridMinX) / siteModelCellSize);
          int northRow = (int)Math.Floor((currentNorth - subGridMinY) / siteModelCellSize);

          if (Range.InRange(eastCol, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1) &&
              Range.InRange(northRow, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1))
          {
            sieveBitmask.SetBit(eastCol, northRow);
            assignmentContext.ProbePositions[eastCol, northRow]
              .SetOffsets(currentEast - subGridMinX, currentNorth - subGridMinY);
          }

          currentEast += stepEastX;
          currentNorth += stepEastY;
        }
      }
    }
  }
}
