using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Common.Records;
using VSS.TRex.Common.Types;
using VSS.TRex.Exports.Patches;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  public class PatchResultTests
  {
    [Fact]
    public void Creation()
    {
      var result = new PatchResult();
      result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, true)]
    [InlineData(4, 1, true)]
    [InlineData(1, 2, true)]
    [InlineData(2, 2, true)]
    [InlineData(4, 2, true)]
    [InlineData(1, 4, true)]
    [InlineData(2, 4, true)]
    [InlineData(4, 4, true)]
    [InlineData(1, 8, false)]
    [InlineData(8, 1, false)]
    public void PatchResult_ConstructResultData_ElevationOffsets(byte elevOffetSize, byte timeOffsetSize, bool valid)
    {
      var result = new PatchResult
      {
        TotalNumberOfPagesToCoverFilteredData = 1,
        CellSize = SubGridTreeConsts.DefaultCellSize,
        PatchNumber = 1,
        Patch = new []
        {
          new SubgridDataPatchRecord_ElevationAndTime
          { 
            ElevationOffsetSize = elevOffetSize, 
            TimeOffsetSize = timeOffsetSize,
            Data = new PatchOffsetsRecord[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension]
          }
        }       
      };

      if (valid)
      {
        var bytes = result.ConstructResultData();
        bytes.Should().NotBeNull();
      }
      else
      {
        Action act = () => result.ConstructResultData();
        act.Should().Throw<ArgumentException>().WithMessage("*Unknown bytes size*");
      }
    }
  }
}
