using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataCCAEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataCMVEntryRecord_Creation()
    {
      SubGridCellPassDataCCAEntryRecord rec = new SubGridCellPassDataCCAEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredCCA == CellPassConsts.NullCCA);
      Assert.True(rec.TargetCCA == CellPassConsts.NullCCATarget);
      Assert.True(rec.PreviousMeasuredCCA == CellPassConsts.NullCCA);
      Assert.True(rec.PreviousTargetCCA == CellPassConsts.NullCCATarget);
    }
  }
}
