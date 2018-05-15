using System;
using System.IO;
//using VSS.VisionLink.Raptor.SubGridTrees.Server;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;
using System.Text;
using Xunit;

namespace VSS.TRex.SubGridTrees.Utilities.Tests
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
