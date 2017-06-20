using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Tests
{
    [TestClass()]
    public class SubGridCellSegmentPassesDataWrapper_NonStaticTests
    {
        private CellPass TestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 100,
                CCA = 101,
                CCV = 102,
                Frequency = 103,
                gpsMode = Raptor.Types.GPSMode.Fixed,
                halfPass = false,
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

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Test()
        {
            SubGridCellSegmentPassesDataWrapper_NonStatic item = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            Assert.IsNotNull(item, "Failed to create instance");

            ISubGridCellSegmentPassesDataWrapper itemInterface = new SubGridCellSegmentPassesDataWrapper_NonStatic();
            Assert.IsNotNull(itemInterface, "Failed to create instance interface");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassCount_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            Assert.IsTrue(item.PassCount(1, 1) == 0, "New segment cell @ (1, 1) does not report 0 passcount");

            item.AddPass(1, 1, new CellPass());
            Assert.IsTrue(item.PassCount(1, 1) == 1, "Incorrect pass count, expected 1, got {0}", item.PassCount(1, 1));
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AllocatePasses_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            item.AllocatePasses(1, 1, 10);

            Assert.IsTrue(item.PassCount(1, 1) == 10, "Incorrect pass count, expected 10, got {0}", item.PassCount(1, 1));
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AddPass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            item.AddPass(1, 1, TestCellPass());
            Assert.IsTrue(item.PassCount(1, 1) == 1, "Incorrect pass count, expected 1, got {0}", item.PassCount(1, 1));

            Assert.IsTrue(item.ExtractCellPass(1, 1, 0).Equals(TestCellPass()), "Cell added is not as expected when retrieved");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ReplacePass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.IsTrue(item.PassCount(1, 1) == 1, "Incorrect pass count, expected 1, got {0}", item.PassCount(1, 1));
            Assert.IsTrue(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");

            pass.CCV = 1000; // Change the cell pass a little

            item.ReplacePass(1, 1, 0, pass);

            Assert.IsTrue(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ExtractCellPass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.IsTrue(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
        }

        [TestMethod()]
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

            Assert.IsTrue(item.PassCount(1, 1) == 3, "Passes not added to cell");

            int index = 0;
            bool exactMatch;

            exactMatch = item.LocateTime(1, 1, new DateTime(1999, 12, 31, 0, 0, 0), out index);
            Assert.IsTrue(index == -1, "Search for pass earlier than first found a cell pass, index = {0}", index);

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 1, 0, 0, 0), out index);
            Assert.IsTrue(exactMatch && index > -1 && item.Pass(1, 1, (uint)index).Equals(pass1), "Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 0)");

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 1, 0, 0, 1), out index);
            Assert.IsTrue(exactMatch == false && item.Pass(1, 1, (uint)index).Equals(pass1), "Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 1), index = {0}", index);

            exactMatch = item.LocateTime(1, 1, new DateTime(2000, 1, 2, 10, 0, 0), out index);
            Assert.IsTrue(!exactMatch && index > -1 && item.Pass(1, 1, (uint)index).Equals(pass2), "Failed to locate pass at DateTime(2001, 1, 2, 10, 0, 0), index = {0}", index);

            exactMatch = item.LocateTime(1, 1, new DateTime(2001, 1, 1, 0, 0, 0), out index);
            Assert.IsTrue(!exactMatch && index > -1 && item.Pass(1, 1, (uint)index).Equals(pass3), "Failed to locate pass at DateTime(2001, 1, 1, 0, 0, 0), index = {0}", index);
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Read_Test()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ReadTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ReadTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Write_Test()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_WriteTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_WriteTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassHeight_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.IsTrue(item.PassHeight(1, 1, 0).Equals(pass.Height), "Cell pass height not same as value added");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassTime_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.IsTrue(item.PassTime(1, 1, 0).Equals(pass.Time), "Cell pass time not same as value added");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Integrate_Test()
        {
            Assert.Fail();
        }

        [TestMethod()]
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

            Assert.IsTrue(item.PassCount(1, 1) == 3, "Passes not added to cell");

            Cell_NonStatic cell = item.Cell(1, 1);

            Assert.IsTrue(cell.Passes.Zip(passes, (a, b) => a.Equals(b)).All(x => x == true), "Extracted cell does not contain the same cell passes added to it");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Pass_Test()
        {
            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.IsTrue(item.Pass(1, 1, 0).Equals(pass), "Cell pass not same as value added");
        }

        [TestMethod()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_SetState_Test()
        {
            // Create the main 2D array of cell pass arrays
            CellPass[,][] cellPasses = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension][];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) => cellPasses[x, y] = new CellPass[1] { TestCellPass() });

            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_NonStatic();

            // Feed the cell passes to the segment
            item.SetState(cellPasses);

            // Check the passes all match
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                Assert.IsTrue(cellPasses[x, y][0].Equals(item.Pass(x, y, 0)), "Pass in cell {0}:{1} does not match", x, y);
            });
        }
   }
}