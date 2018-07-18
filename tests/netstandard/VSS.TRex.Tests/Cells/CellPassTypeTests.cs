using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Cells;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Cells
{
    public class CellPassTypeTests
    {
        [Fact]
        public void Test_CellPassTypeSet_Creation()
        {
            PassTypeSet PassTypes = PassTypeSet.None;
            Assert.True(PassTypes == PassTypeSet.None);

            PassTypes |= PassTypeSet.Front;
            Assert.True(PassTypes == PassTypeSet.Front);

            PassTypes |= PassTypeSet.Rear;
            Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear));

            PassTypes |= PassTypeSet.Track;
            Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear | PassTypeSet.Track));

            PassTypes |= PassTypeSet.Wheel;
            Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear | PassTypeSet.Track | PassTypeSet.Wheel));
        }

        [Fact]
        public void Test_CellPassTypeSet_Comparison()
        {
            PassTypeSet PassTypes = PassTypeSet.None;

            PassTypes = PassTypeSet.Front;
            Assert.True(CellPass.PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Front));

            PassTypes = PassTypeSet.Rear;
            Assert.True(CellPass.PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Rear));

            PassTypes = PassTypeSet.Track;
            Assert.True(CellPass.PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Track));

            PassTypes = PassTypeSet.Wheel;
            Assert.True(CellPass.PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Wheel));
        }
    }
}
