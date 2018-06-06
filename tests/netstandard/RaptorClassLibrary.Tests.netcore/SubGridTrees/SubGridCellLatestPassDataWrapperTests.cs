using VSS.TRex.SubGridTrees.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.TRex.SubGridTrees.Server.Tests
{
        public class Test_SubGridCellLatestPassDataWrapper
    {
        [Fact()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Creation()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.True(wrapper.PassDataExistanceMap != null && wrapper.PassDataExistanceMap.IsEmpty(), "Instance not created as expected");
        }

        [Fact()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Clear()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.True(wrapper.PassDataExistanceMap.IsEmpty(), "Existence map not empty after creation");

            wrapper.PassDataExistanceMap.SetBitValue(0, 0, true);
        }
    }
}