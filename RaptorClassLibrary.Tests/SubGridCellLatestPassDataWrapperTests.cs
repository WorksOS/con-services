using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Tests
{
    [TestClass()]
    public class Test_SubGridCellLatestPassDataWrapper
    {
        [TestMethod()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Creation()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.IsTrue(wrapper.PassDataExistanceMap != null && wrapper.PassDataExistanceMap.IsEmpty(), "Instance not created as expected");
        }

        [TestMethod()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Clear()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.IsTrue(wrapper.PassDataExistanceMap.IsEmpty(), "Existence map not empty after creation");

            wrapper.PassDataExistanceMap.SetBitValue(0, 0, true);
        }
    }
}