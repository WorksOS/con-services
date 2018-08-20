using Xunit;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTreeUtilitiesTests
    {
        [Fact]
        public void Test_SubGridTreeUtilities_GetOTGLeafSubGridCellIndex()
        {
            // GetOTGLeafSubGridCellIndex is a subgrid cell key relative operation only, and depends only on the values of the 
            // Cell X & Y location to compute the subgrid relative X and y cell indices that location would have in any subgrid.

            byte SubGridCellX, SubGridCellY;
            VSS.TRex.SubGridTrees.Server.Utilities.SubGridUtilities.GetOTGLeafSubGridCellIndex(0, 0, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");

            VSS.TRex.SubGridTrees.Server.Utilities.SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == (SubGridTreeConsts.SubGridTreeDimension - 1) && SubGridCellY == (SubGridTreeConsts.SubGridTreeDimension - 1), "Subgrid cell indices incorrect");

            VSS.TRex.SubGridTrees.Server.Utilities.SubGridUtilities.GetOTGLeafSubGridCellIndex(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");
        }
    }
}
