using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class SubGridTreeUtilitiesTests
    {
        [TestMethod]
        public void Test_SubGridTreeUtilities_GetOTGLeafSubGridCellIndex()
        {
            // GetOTGLeafSubGridCellIndex is a subgrid cell key relative operation only, and depends only on the values of the 
            // Cell X & Y location to compute the subgrid relative X and y cell indices that location would have in any subgrid.

            byte SubGridCellX, SubGridCellY;
            SubGridUtilities.GetOTGLeafSubGridCellIndex(0, 0, out SubGridCellX, out SubGridCellY);
            Assert.IsTrue(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");

            SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTree.SubGridTreeDimension - 1, SubGridTree.SubGridTreeDimension - 1, out SubGridCellX, out SubGridCellY);
            Assert.IsTrue(SubGridCellX == (SubGridTree.SubGridTreeDimension - 1) && SubGridCellY == (SubGridTree.SubGridTreeDimension - 1), "Subgrid cell indices incorrect");

            SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension, out SubGridCellX, out SubGridCellY);
            Assert.IsTrue(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");
        }
    }
}
