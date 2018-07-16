using System;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex;
using Xunit;

namespace VSS.TRex.Tests
{
        public class SubGridTreeUtilitiesTests
    {
        [Fact]
        public void Test_SubGridTreeUtilities_GetOTGLeafSubGridCellIndex()
        {
            // GetOTGLeafSubGridCellIndex is a subgrid cell key relative operation only, and depends only on the values of the 
            // Cell X & Y location to compute the subgrid relative X and y cell indices that location would have in any subgrid.

            byte SubGridCellX, SubGridCellY;
            SubGridUtilities.GetOTGLeafSubGridCellIndex(0, 0, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");

            SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTree.SubGridTreeDimensionMinus1, SubGridTree.SubGridTreeDimensionMinus1, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == (SubGridTree.SubGridTreeDimension - 1) && SubGridCellY == (SubGridTree.SubGridTreeDimension - 1), "Subgrid cell indices incorrect");

            SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");
        }
    }
}
