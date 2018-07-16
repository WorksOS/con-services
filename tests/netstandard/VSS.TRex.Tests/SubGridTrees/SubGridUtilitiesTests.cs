using System;
using System.IO;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;
using System.Text;
using Xunit;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridUtilitiesTests
    {
        [Fact]
        public void Test_SubGridDimensionalIterator()
        {
            // Ensure the iterator covers all the cells in a subgrid
            int counter = 0;

            SubGridUtilities.SubGridDimensionalIterator((x, y) => counter++);
            Assert.Equal(SubGridTree.SubGridTreeCellsPerSubgrid, counter);
        }
    }
}
