using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.VisionLink.Raptor.SubGridTrees.Server;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using System.Text;

namespace VSS.VisionLink.Raptor.SubGridTrees.Utilities.Tests
{
    [TestClass]
    public class SubGridUtilitiesTests
    {
        [TestMethod]
        public void Test_SubGridDimensionalIterator()
        {
            // Ensure the iterator covers all the cells in a subgrid
            int counter = 0;

            SubGridUtilities.SubGridDimensionalIterator((x, y) => counter++);
            Assert.AreEqual(SubGridTree.SubGridTreeCellsPerSubgrid, counter, "SubGridDimensionalIterator not covering all cells");
        }
    }
}
