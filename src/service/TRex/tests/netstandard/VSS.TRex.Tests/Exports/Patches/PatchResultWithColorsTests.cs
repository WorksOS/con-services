using System;
using FluentAssertions;
using VSS.TRex.Common.Records;
using VSS.TRex.Exports.Patches;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  public class PatchResultWithColorsTests
  {
    [Fact]
    public void Creation()
    {
      var result = new PatchResultWithColors();
      result.Should().NotBeNull();
    }

    [Fact]
    public void PatchResult_ConstructResultData()
    {
      var result = new PatchResultWithColors
      {
        TotalNumberOfPagesToCoverFilteredData = 1,
        CellSize = SubGridTreeConsts.DefaultCellSize,
        PatchNumber = 1,
        Patch = new[]
        {
          new SubgridDataPatchRecord_ElevationAndColor
          {
            Data = new PatchColorsRecord[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension]
          }
        }
      };

      var bytes = result.ConstructResultData();
      bytes.Should().NotBeNull();
    }
  }
}
