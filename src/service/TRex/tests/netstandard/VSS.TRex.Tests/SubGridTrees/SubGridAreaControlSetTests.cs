using System;
using System.Security.Cryptography.X509Certificates;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridAreaControlSetTests
  {
    #region ComputeSieveBitmaskInteger

    [Fact]
    public void AreaControlSet_Integer_PixelWorldSizeMustBeGreaterThan0()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = 0;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = 0;
      double subGridWorldOriginY = 0;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits _);
      Assert.False(sieveFilterInUse, "sieve filter should not have been generated for zero pixelWorldSize");
    }

    [Fact]
    public void AreaControlSet_Integer_Unhappy_StepMustBeAtLeast2TimesCellSize()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      // pixelWorldSizeOrInterval/siteModelCellsize will result in stepSize = 1
      double pixelWorldSizeOrIntervalXY = siteModelCellsize; 
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = 0;
      double subGridWorldOriginY = 0;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits _);
      Assert.False(sieveFilterInUse, "sieve filter should not have been generated where pixel size <2x siteModelCellSize");
    }

    [Fact]
    public void AreaControlSet_Integer_WorldOriginPriorToCenterCell00()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      // for ComputeSieveInteger(): (differs to ComputeSieveFloat())
      //    subGridWorldOriginX is treated as the center of a cell
      //    i.e. 0,0 is seen as to the left of the grid; cellsize/2 is seen at on cell 0,0
      double pixelWorldSizeOrIntervalXY = SubGridTreeConsts.DefaultCellSize * 2;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = 0;
      double subGridWorldOriginY = 0;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 1,1
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      var rowXActual = sieveBitmask.RowToString(0);
      var rowXExpected = " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 0 is invalid");

      rowXActual = sieveBitmask.RowToString(2);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 2 is invalid");

      rowXActual = sieveBitmask.RowToString(1);
      rowXExpected = " 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 1 is invalid");

      rowXActual = sieveBitmask.RowToString(3);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 3 is invalid");
    }

    [Fact]
    public void AreaControlSet_Integer_WorldOriginCenterCell00()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      var rowXActual = sieveBitmask.RowToString(0);
      var rowXExpected = " 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 0 is invalid");

      rowXActual = sieveBitmask.RowToString(2);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 2 is invalid");

      rowXActual = sieveBitmask.RowToString(1);
      rowXExpected = " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 1 is invalid");

      rowXActual = sieveBitmask.RowToString(3);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 3 is invalid");
    }

    [Fact]
    public void AreaControlSet_Integer_Unhappy_UserOrigin_IgnoredForInteger()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = siteModelCellsize * 30;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");
    }

    [Fact]
    public void AreaControlSet_Integer_Unhappy_Rotation_IgnoredForInteger()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = 0;
      var rotation = 90;
      var areaControlSet = new AreaControlSet(true, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");
    }
    #endregion ComputeSieveBitmaskInteger

    #region ComputeSieveBitmaskFloat
    [Fact]
    public void AreaControlSet_Float_PixelWorldSizeMustBeGreaterThan0()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = 0;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = 0;
      double subGridWorldOriginY = 0;
      var subGridMoniker = "theSubGridMoniker";
      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits _);
      Assert.False(sieveFilterInUse, "sieve filter should not have been generated for zero pixelWorldSize");
    }

    [Fact]
    public void AreaControlSet_Float_Unhappy_StepMustBeAtLeastCellSize()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      // pixelWorldSizeOrInterval/siteModelCellsize will result in stepSize = 1
      double pixelWorldSizeOrIntervalXY = siteModelCellsize /2;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = 0;
      double subGridWorldOriginY = 0;
      var subGridMoniker = "theSubGridMoniker";
      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits _);
      Assert.False(sieveFilterInUse, "sieve filter should not have been generated where pixelWorldSize < siteModelCellSize");
    }

    [Fact]
    public void AreaControlSet_Float_WorldOriginPriorToCenterCell00()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;
      
      // for ComputeSieveFloat(): (differs to ComputeSieveInteger())
      //    subGridWorldOriginX is becomes the center of a cell
      //    i.e. 0,0 is seen as center of cell 1,1; cellsize/2 is ALSO seen at on cell 0,0
      double pixelWorldSizeOrIntervalXY = SubGridTreeConsts.DefaultCellSize * 2;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2; 
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";

      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 1,1
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      var rowXActual = sieveBitmask.RowToString(0);
      var rowXExpected = " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 0 is invalid");

      rowXActual = sieveBitmask.RowToString(2);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 2 is invalid");

      rowXActual = sieveBitmask.RowToString(1);
      rowXExpected = " 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 1 is invalid");

      rowXActual = sieveBitmask.RowToString(3);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 3 is invalid");
    }

    [Fact]
    public void AreaControlSet_Float_WorldOriginCenterCell00()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = 0;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      var rowXActual = sieveBitmask.RowToString(0);
      var rowXExpected = " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 0 is invalid");

      rowXActual = sieveBitmask.RowToString(2);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 2 is invalid");

      rowXActual = sieveBitmask.RowToString(1);
      rowXExpected = " 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 1 is invalid");

      rowXActual = sieveBitmask.RowToString(3);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 3 is invalid");

      Assert.True(Math.Abs(assignmentContext.ProbePositions[1, 1].XOffset - 0.51) < 0.001, "real world X offset for 1,1 is invalid");
      Assert.True(Math.Abs(assignmentContext.ProbePositions[1, 1].YOffset - 0.51) < 0.001, "real world Y offset for 1,1 is invalid");
    }

    [Fact]
    public void AreaControlSet_Float_UserOrigin()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = 1000; // i.e. easting/northing // siteModelCellsize * 30;
      var rotation = Consts.NullDouble;
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      var rowXActual = sieveBitmask.RowToString(0);
      var rowXExpected = " 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0 1 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 0 is invalid");

      rowXActual = sieveBitmask.RowToString(2);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 2 is invalid");

      rowXActual = sieveBitmask.RowToString(1);
      rowXExpected = " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
      Assert.True(rowXExpected == rowXActual, "bitSet for row 1 is invalid");

      rowXActual = sieveBitmask.RowToString(3);
      Assert.True(rowXExpected == rowXActual, "bitSet for row 3 is invalid");

      CheckProbePositions(assignmentContext.ProbePositions, true, 0.23, 0.68, 0.23, 0.68);
    }


    [Fact]
    public void AreaControlSet_Float_Rotation()
    {
      double siteModelCellsize = SubGridTreeConsts.DefaultCellSize;

      double pixelWorldSizeOrIntervalXY = siteModelCellsize * 2;
      double userOriginXY = 0;
      var rotation = 1.5708; // 90deg
      var areaControlSet = new AreaControlSet(false, pixelWorldSizeOrIntervalXY, pixelWorldSizeOrIntervalXY, userOriginXY, userOriginXY, rotation);

      double subGridWorldOriginX = siteModelCellsize / 2;
      double subGridWorldOriginY = siteModelCellsize / 2;
      var subGridMoniker = "theSubGridMoniker";
      var assignmentContext = new FilteredValueAssignmentContext();
      var sieveFilterInUse = GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, subGridMoniker,
        areaControlSet, siteModelCellsize, assignmentContext, out SubGridTreeBitmapSubGridBits sieveBitmask);

      Assert.True(sieveFilterInUse, "sieve filter should have been generated");
      // 256/1024 set, first set at 0,0
      Assert.True(sieveBitmask.CountBits() == 256, $"Incorrect count of bits set. Expected 256 but got {sieveBitmask.CountBits()}");

      CheckProbePositions(assignmentContext.ProbePositions, false, 0.51, 0.68, 0.51, 0.68);
    }

    private void CheckProbePositions(FilteredValueAssignmentContext.ProbePoint[,] probePositions, bool areOddsZero, double xBase, double xIncrement, double yBase, double yIncrement)
    {

      for (int x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
      {
        for (int y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
        {
          Math.DivRem(x, 2, out int xRem);
          Math.DivRem(y, 2, out int yRem);

          double expectedXOffset;
          double expectedYOffset;

          if ((areOddsZero && (xRem > 0 || yRem > 0)) 
              || (!areOddsZero && (xRem == 0 || yRem == 0)))
          {
            expectedXOffset = 0;
            expectedYOffset = 0;
          }
          else
          {
            expectedXOffset = x == 0 ? xBase : xBase + (xIncrement * (int)(x / 2));
            expectedYOffset = y == 0 ? yBase : yBase + (yIncrement * (int)(y / 2));
          }

          Assert.True(Math.Abs(probePositions[x, y].XOffset - expectedXOffset) < 0.001, $"real world X offset for row:{x},{y} is invalid. Expected: {expectedXOffset} but got {probePositions[x, y].XOffset}");
          Assert.True(Math.Abs(probePositions[x, y].YOffset - expectedYOffset) < 0.001, $"real world Y offset for row:{x},{y} is invalid. Expected: {expectedYOffset} but got {probePositions[x, y].YOffset}");
        }
      }
    }

    #endregion ComputeSieveBitmaskFloat

  }
}
