using System;
using System.Linq;
using System.Text;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.SubGridTrees.Server.Tests
{
        public class SubGridCellSegmentPassesDataWrapper_NonStaticTests
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
                gpsMode = GPSMode.Fixed,
                HalfPass = false,
                Height = 104,
                // MachineID = 105,
                InternalSiteModelMachineIndex = 105,
                GPSModeStore = 106,
                MachineSpeed = 106,
                MaterialTemperature = 107,
                MDP = 108,
                PassType = PassType.Track,
                RadioLatency = 109,
                RMV = 110,
                Time = new DateTime(2000, 1, 2, 3, 4, 5)
            };
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Test()
        {
            SubGridCellSegmentPassesDataWrapper_NonStatic item = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            Assert.NotNull(item);

            ISubGridCellSegmentPassesDataWrapper itemInterface = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            Assert.NotNull(itemInterface);
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassCount_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            Assert.Equal((uint)0, item.PassCount(1, 1));

            item.AddPass(1, 1, new CellPass());
            Assert.Equal((uint)1, item.PassCount(1, 1));
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AllocatePasses_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            item.AllocatePasses(1, 1, 10);

            Assert.Equal((uint)10, item.PassCount(1, 1));
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AddPass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            item.AddPass(1, 1, TestCellPass());
            Assert.Equal((uint)1, item.PassCount(1, 1));

            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(TestCellPass()), "Cell added is not as expected when retrieved");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ReplacePass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.Equal((uint)1, item.PassCount(1, 1));
            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");

            pass.CCV = 1000; // Change the cell pass a little

            item.ReplacePass(1, 1, 0, pass);

            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ExtractCellPass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_LocateTime_Test()
        {
            CellPass pass1 = TestCellPass();
            pass1.Time = new DateTime(2000, 1, 1, 0, 0, 0);

            CellPass pass2 = TestCellPass();
            pass2.Time = new DateTime(2000, 1, 2, 0, 0, 0);

            CellPass pass3 = TestCellPass();
            pass3.Time = new DateTime(2000, 1, 3, 0, 0, 0);

            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            item.AddPass(1, 1, pass1);
            item.AddPass(1, 1, pass2);
            item.AddPass(1, 1, pass3);

            Assert.Equal((uint)3, item.PassCount(1, 1));

            bool exactMatch = item.LocateTime(1, 1, new DateTime(1999, 12, 31, 0, 0, 0), out int index);
            Assert.False(exactMatch, "Exact match found!!!");
            Assert.Equal(0, index);

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 1, 0, 0, 0), out index);
            Assert.True(exactMatch && index > -1 && item.Pass(1, 1, (uint)index).Equals(pass1), $"Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 0), located pass is {item.Pass(1, 1, (uint)index)}");

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 1, 0, 0, 1), out index);
            Assert.True(exactMatch == false && item.Pass(1, 1, (uint)index - 1).Equals(pass1), $"Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 1), index = {index}");

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 2, 10, 0, 0), out index);
            Assert.True(!exactMatch && index > -1 && item.Pass(1, 1, (uint)index - 1).Equals(pass2), $"Failed to locate pass at DateTime(2001, 1, 2, 10, 0, 0), index = {index}");

            exactMatch = item.LocateTime(1, 1, new DateTime(2001, 1, 1, 0, 0, 0), out index);
            Assert.True(!exactMatch && index > -1 && item.Pass(1, 1, (uint)index - 1).Equals(pass3), $"Failed to locate pass at DateTime(2001, 1, 1, 0, 0, 0), index = {index}");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_WriteRead_Test()
        {
            // Create the main 2D array of cell pass arrays
            CellPass[,][] cellPasses = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension][];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) => cellPasses[x, y] = new [] { TestCellPass() });

            ISubGridCellSegmentPassesDataWrapper item1 = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            MemoryStream ms = new MemoryStream();

            // Write to the stream...
            BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true);
            item1.SetState(cellPasses);
            item1.Write(writer);

            // Create a new segment and read it back again
            ISubGridCellSegmentPassesDataWrapper item2 = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            ms.Position = 0;
            BinaryReader reader = new BinaryReader(ms, Encoding.UTF8, true);
            item2.Read(reader);

            SubGridUtilities.SubGridDimensionalIterator((col, row) =>
            {
                Assert.True(item1.ExtractCellPasses(col, row)
                                 .Zip(item2.ExtractCellPasses(col, row), (a, b) => a.Equals(b))
                                 .All(x => x), 
                            "Read segment does not contain the same list of cell passes written into it");
            });
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassHeight_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.PassHeight(1, 1, 0).Equals(pass.Height), "Cell pass height not same as value added");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassTime_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.PassTime(1, 1, 0).Equals(pass.Time), "Cell pass time not same as value added");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Integrate_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            CellPass pass2 = TestCellPass();
            pass2.Time = pass2.Time.AddSeconds(60);

            Cell_NonStatic integrateFrom = new Cell_NonStatic();
            integrateFrom.AddPass(pass2);

            item.Integrate(1, 1, integrateFrom.Passes, 0, 0, out int addedCount, out int modifiedCount);

            Assert.Equal((uint)2, item.PassCount(1, 1));
            Assert.Equal(1, addedCount);
            Assert.Equal(0, modifiedCount);
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Cell_Test()
        {
            CellPass pass1 = TestCellPass();
            pass1.Time = new DateTime(2000, 1, 1, 0, 0, 0);

            CellPass pass2 = TestCellPass();
            pass2.Time = new DateTime(2000, 1, 2, 0, 0, 0);

            CellPass pass3 = TestCellPass();
            pass3.Time = new DateTime(2000, 1, 3, 0, 0, 0);

            CellPass[] passes = new CellPass[] { pass1, pass2, pass3 };

            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            item.AddPass(1, 1, pass1);
            item.AddPass(1, 1, pass2);
            item.AddPass(1, 1, pass3);

            Assert.Equal((uint)3, item.PassCount(1, 1));

            Cell_NonStatic cell = new Cell_NonStatic() { Passes = item.ExtractCellPasses(1, 1) };

            Assert.True(cell.Passes.Zip(passes, (a, b) => a.Equals(b)).All(x => x), 
                        "Extracted cell does not contain the same cell passes added to it");
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Pass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.Pass(1, 1, 0).Equals(pass), "Cell pass not same as value added");
        }

        /// <summary>
        /// Tests that the method to take a set of cell passes for the segment can set all those call passes into the 
        /// internal representation
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_SetState_Test()
        {
            // Create the main 2D array of cell pass arrays
            CellPass[,][] cellPasses = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension][];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) => cellPasses[x, y] = new [] { TestCellPass() });

            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            // Feed the cell passes to the segment
            item.SetState(cellPasses);

            // Check the passes all match
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                Assert.True(cellPasses[x, y][0].Equals(item.Pass(x, y, 0)), $"Pass in cell {x}:{y} does not match");
            });
        }

        /// <summary>
        /// Tests that the machine ID set for an uncompressed non static cell pass wrapper is null (as expected)
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_NoMachineIDSet_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            // Uncompressed static cell pass wrappers do not provide machine ID sets
            Assert.Null(item.GetMachineIDSet());
        }
    }
}
