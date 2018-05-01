using System;
using System.IO;
//using VSS.VisionLink.Raptor.SubGridTrees.Server;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using System.Text;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Compression;
using System.Diagnostics;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Tests
{
        public class MutabilityConverterTests
    {
        /// <summary>
        /// A handy test cell pass for the unit tests below to use
        /// </summary>
        /// <returns></returns>
        private CellPass TestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 100,
                CCA = 101,
                CCV = 102,
                Frequency = 103,
                gpsMode = Raptor.Types.GPSMode.Fixed,
                HalfPass = false,
                Height = 104,
                MachineID = 105,
                GPSModeStore = 106,
                MachineSpeed = 106,
                MaterialTemperature = 107,
                MDP = 108,
                passType = Raptor.Types.PassType.Track,
                RadioLatency = 109,
                RMV = 110,
                SiteModelMachineIndex = 111,
                Time = new DateTime(2000, 1, 2, 3, 4, 5)
            };
        }

        [Fact]
        public void Test_MutabilityConverterTests_ConvertSubgridDirectoryTest()
        {
            // Create a subgrid directory with a single segment and some cells. Create a stream fron it then use the
            // mutability converter to convert it to the immutable form. Read this back into an immutable representation
            // and compare the mutable and immutable versions for consistency.

            // Create a leaf to contain the mutable directory
            IServerLeafSubGrid mutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTree.SubGridTreeLevels);
            mutableLeaf.Directory.GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false);

            // Load the mutable stream of information
            mutableLeaf.Directory.CreateDefaultSegment();

            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic).PassData[x, y].Height = 1234.5678F;
            });

            // Take a copy of the mutable cells for later reference
            SubGridCellLatestPassDataWrapper_NonStatic mutableCells = (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic);

            MemoryStream outStream = new MemoryStream();
            mutableLeaf.SaveDirectoryToStream(outStream);

            MemoryStream inStream = null;

            MutabilityConverter.ConvertToImmutable(FileSystemStreamType.SubGridDirectory, outStream, out inStream);

            IServerLeafSubGrid immutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTree.SubGridTreeLevels);
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

                Assert.True(Math.Abs(diff) <= 0.001,
                    $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
            });
        }

        [Fact]
        public void Test_MutabilityConverterTests_ConvertSubgridSegmentTest()
        {
            // Create a segment with some cell passes. Create a stream fron it then use the
            // mutability converter to convert it to the immutable form. Read this back into an immutable representation
            // and compare the mutable and immutable versions for consistency.

            // Create a mutable segment to contain the passes
            SubGridCellPassesDataSegment mutableSegment = new SubGridCellPassesDataSegment
                (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false),
                 SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false));

            // Load the mutable stream of information
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                (mutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic).PassData[x, y].Height = 1234.5678F;

                // Add 5 passes to each cell
                for (int i = 0; i < 5; i++)
                {
                    CellPass cellPass = TestCellPass();

                    // Adjust the height/time so there is a range of values
                    cellPass.Time = cellPass.Time.AddMinutes(i);
                    cellPass.Height += i;

                    mutableSegment.PassesData.AddPass(x, y, cellPass);
                }
            });

            // Take a copy of the mutable cells and cell passes for later reference
            SubGridCellLatestPassDataWrapper_NonStatic mutableLatest = (mutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic);
            CellPass[,][] mutablePasses = mutableSegment.PassesData.GetState();

            MemoryStream outStream = new MemoryStream();
            using (var writer = new BinaryWriter(outStream, Encoding.UTF8, true))
            {
                mutableSegment.Write(writer);
            }

            MemoryStream inStream = null;

            // Convert the mutable data into the immutable form and reload it into an immutable segment
            MutabilityConverter.ConvertToImmutable(FileSystemStreamType.SubGridSegment, outStream, out inStream);

            SubGridCellPassesDataSegment immutableSegment = new SubGridCellPassesDataSegment
                (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true),
                 SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(false, true));

            inStream.Position = 0;

            using (var reader = new BinaryReader(inStream, Encoding.UTF32, true))
            {
                immutableSegment.Read(reader, true, true);
            }

            SubGridCellLatestPassDataWrapper_StaticCompressed immutableLatest = (immutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed);
            ISubGridCellSegmentPassesDataWrapper immutablePasses = immutableSegment.PassesData;

            // Check height of the latest cells match to tolerance given the compressed lossiness.
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                double mutableValue = mutableLatest.PassData[x, y].Height;
                double immutableValue = immutableLatest.ReadHeight((int)x, (int)y);

                double diff = immutableValue - mutableValue;

                Assert.True(Math.Abs(diff) <= 0.001, $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
            });

            // Check the heights specially to account for tolerance differences
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                for (uint i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
                {
                    double mutableValue = mutableSegment.PassesData.PassHeight(x, y, i);
                    double immutableValue = immutableSegment.PassesData.PassHeight(x, y, i);

                    double diff = immutableValue - mutableValue;

                    Assert.True(Math.Abs(diff) <= 0.001, $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
                }
            });

            // Check the times specially to account for tolerance differences
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                for (uint i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
                {

                    DateTime mutableValue = mutableSegment.PassesData.PassTime(x, y, i);
                    DateTime immutableValue = immutableSegment.PassesData.PassTime(x, y, i);

                    TimeSpan diff = mutableValue - immutableValue;

                    Assert.True(diff.Duration() <= TimeSpan.FromSeconds(1), $"Cell time at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
                }
            });

            // Check that the cell passes in the cell pass stacks for the segment match to tolerance given the compressed lossiness
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                for (int i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
                {
                    CellPass cellPass = immutableSegment.PassesData.ExtractCellPass(x, y, i);

                    // Force the height and time in the immutable record to be the same as the immutable record
                    // as they have been independently checked above. Also set the machine ID to be the same as the mutable
                    // machine ID as the immutable representation does not include it in the Ignite POC
                    cellPass.Time = mutablePasses[x, y][i].Time;
                    cellPass.Height = mutablePasses[x, y][i].Height;
                    cellPass.MachineID = mutablePasses[x, y][i].MachineID;

                    CellPass mutableCellPass = mutablePasses[x, y][i];
                    Assert.True(mutableCellPass.Equals(cellPass), $"Cell passes not equal at Cell[{x}, {y}], cell pass index {i}");
                }
            });
        }

        [Fact(Skip = "not implemented")]
        public void Test_MutabilityConverterTests_ConvertEventListTest()
        {
        }
    }
}