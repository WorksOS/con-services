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
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Tests
{
    [TestClass]
    public class MutabilityConverterTests
    {
        [TestMethod]
        public void Test_MutabilityConverterTests_ConvertSubgridDirectoryTest()
        {
            // Create a subgrid directory with a single segment and some cells. Create a stream fron it then use the
            // mutability converter to convert it to the immutable form. Read this back into an immutable representation
            // and compare the mutable and immutable versions for consistency.

            // Create a leaf to contain the mutable directory
            ServerSubGridTreeLeaf mutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTree.SubGridTreeLevels);
            mutableLeaf.Directory.GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false);

            // Load the mutable stream of information
            mutableLeaf.Directory.AllocateGlobalLatestCells();
            mutableLeaf.Directory.CreateDefaultSegment();

            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic).PassData[x, y].Height = 12345.6789F;
            });

            SubGridCellLatestPassDataWrapper_NonStatic mutableCells = (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic);

            MemoryStream outStream = new MemoryStream();
            mutableLeaf.SaveDirectoryToStream(outStream);

            MemoryStream inStream = null;

            MutabilityConverter.ConvertToImmutable(FileSystemStreamType.SubGridDirectory, outStream, out inStream);

            ServerSubGridTreeLeaf immutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTree.SubGridTreeLevels);
            immutableLeaf.Directory.GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true);

            inStream.Position = 0;
            immutableLeaf.LoadDirectoryFromStream(inStream);

            SubGridCellLatestPassDataWrapper_StaticCompressed immutableCells = (immutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_StaticCompressed);

            // Check height of the cells match to tolerance given the compressed lossiness.
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                double mutableValue = mutableCells.PassData[x, y].Height;
                double immutableValue = immutableCells.ReadHeight((int)x, (int)y);

                double diff = immutableValue - mutableValue;

                Assert.IsTrue(Math.Abs(diff) < 0.00001, "Cell at ({0}, {1}) has unexpected value: {2} vs {3}, diff = {4}", 
                             x, y, immutableCells.ReadHeight((int)x, (int)y), mutableCells.PassData[x, y].Height, diff);
            });
        }

        [TestMethod]
        public void Test_MutabilityConverterTests_ConvertSubgridSegmentTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Test_MutabilityConverterTests_ConvertEventListTest()
        {
            Assert.Fail();
        }
    }
}