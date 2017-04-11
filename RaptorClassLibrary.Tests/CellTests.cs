using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class CellTests
    {
        /// <summary>
        /// Ensure the IsEmpty mechanism reports the cell empty of cell passes
        /// </summary>
        [TestMethod]
        public void Test_Cell_EmptyCellPassesOnCreation()
        {
            Cell c = new Cell(0);

            Assert.IsTrue(c.PassCount == 0, "Count of passes in cell not 0 as expected");
            Assert.IsTrue(c.IsEmpty, "Cell does not report itself as being empty of cell passes");
        }

        /// <summary>
        /// Ensure the passcount allocation mechanism creates the appropriate number of entries
        /// </summary>
        [TestMethod]
        public void Test_Cell_CellPassAllocation()
        {
            Cell c = new Cell(0);

            c.AllocatePasses(10);

            Assert.IsTrue(c.PassCount == 10, "Count of passes in cell not 10 as expected.");
        }

        /// <summary>
        /// Ensure the topmostheight mechanism creates the appropriate number of entries when added in an ordered manner
        /// </summary>
        [TestMethod]
        public void Test_Cell_AddCellPassOrdered()
        {
            Cell c = new Cell(0);

            c.AddPass(CellPassTests.ATestCellPass());

            Assert.IsTrue(c.PassCount == 1, "Count of passes in cell not 1 as expected.");

            CellPass cp = CellPassTests.ATestCellPass();
            Assert.IsTrue(c.Passes[0].Equals(cp), "Added cell pass not the same content as it was constructed with");

            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.IsTrue(c.PassCount == 2, "Count of passes in cell not 2 as expected.");

            CellPass cp2 = CellPassTests.ATestCellPass2();
            Assert.IsTrue(c.Passes[1].Equals(cp2), "Added cell pass not the same content as it was constructed with");
        }

        /// <summary>
        /// Ensure the topmostheight mechanism creates the appropriate number of entries when added in an unordered manner
        /// </summary>
        [TestMethod]
        public void Test_Cell_AddCellPassUnOrdered()
        {
            Cell c = new Cell(0);

            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.IsTrue(c.PassCount == 1, "Count of passes in cell not 1 as expected.");

            CellPass cp2 = CellPassTests.ATestCellPass2();
            Assert.IsTrue(c.Passes[0].Equals(cp2), "Added cell pass not the same content as it was constructed with");

            c.AddPass(CellPassTests.ATestCellPass());

            Assert.IsTrue(c.PassCount == 2, "Count of passes in cell not 2 as expected.");

            CellPass cp1 = CellPassTests.ATestCellPass();
            Assert.IsTrue(c.Passes[0].Equals(cp1), "Added cell pass not the same content as it was constructed with");
        }

        /// <summary>
        /// Ensure the topmostheight mechanism selects the correct 'top most' height in terms of 
        /// the height from the most recently recorded cell pass in time
        /// </summary>
        [TestMethod]
        public void Test_Cell_TopMostHeight()
        {
            Cell c1 = new Cell(0);

            // Add two cell passes in ascending order
            c1.AddPass(CellPassTests.ATestCellPass());
            c1.AddPass(CellPassTests.ATestCellPass2());

            Assert.IsTrue(c1.PassCount == 2, "Count of passes in cell not 1 as expected.");
            Assert.IsTrue(c1.TopMostHeight == 50, "Top most height not 50 as expected from cell pass 2");

            // Add two cell passes in ascending order
            Cell c2 = new Cell(0);

            c2.AddPass(CellPassTests.ATestCellPass2());
            c2.AddPass(CellPassTests.ATestCellPass());

            Assert.IsTrue(c2.PassCount == 2, "Count of passes in cell not 1 as expected.");
            Assert.IsTrue(c2.TopMostHeight == 50, "Top most height not 50 as expected from cell pass 2");
        }

        /// <summary>
        /// Check that cell passes are correctly located based on time
        /// </summary>
        [TestMethod]
        public void Test_Cell_LocateTime()
        {
            // Create a cell with two cell passes with different times
            Cell c = new Cell(0);
            CellPass cp1 = CellPassTests.ATestCellPass();
            CellPass cp2 = CellPassTests.ATestCellPass2();

            c.AddPass(cp1);
            c.AddPass(cp2);

            // Check cp1 and cp2 are present in that order
            Assert.IsTrue(c.Passes[0].Equals(cp1) && c.Passes[1].Equals(cp2), "Two passes added are not in expected order");

            // Locate both with the time present in each of the contributing cell passes to ensure exact time matches
            // at the boundaries are preserved
            int cellPassIndex = 0;

            c.LocateTime(cp1.Time, out cellPassIndex);
            Assert.IsTrue(cellPassIndex == 0, "Located cell pass (first) has incorrect index");
            Assert.IsTrue(c.Passes[cellPassIndex].Equals(cp1), "Select (first) cell pass does not match pass added to cell");

            c.LocateTime(cp2.Time, out cellPassIndex);
            Assert.IsTrue(cellPassIndex == 1, "Located cell pass (second) has incorrect index");
            Assert.IsTrue(c.Passes[cellPassIndex].Equals(cp2), "Select (second) cell pass does not match pass added to cell");

            // Locate the cell passes with modified times to cause no exact match to text the 
            // returned insertion position is correct

            c.LocateTime(cp1.Time.AddMinutes(-1), out cellPassIndex);
            Assert.IsTrue(cellPassIndex == 0, "Located insertion position for cell pass (expected position 0) is incorrect");

            c.LocateTime(cp1.Time.AddMinutes(1), out cellPassIndex);
            Assert.IsTrue(cellPassIndex == 1, "Located insertion position for cell pass (expected position 1) is incorrect");

            c.LocateTime(cp2.Time.AddMinutes(1), out cellPassIndex);
            Assert.IsTrue(cellPassIndex == 2, "Located insertion position for cell pass (expected position 2) is incorrect");
        }

        /// <summary>
        /// Test replacing of one pass with anther
        /// </summary>
        [TestMethod]
        public void Test_Cell_ReplaceCellPass()
        {
            Cell c = new Cell(0);

            // Add a cell pass then replace it with a second cell pass
            c.AddPass(CellPassTests.ATestCellPass());
            c.ReplacePass(0, CellPassTests.ATestCellPass2());

            Assert.IsTrue(c.Passes[0].Equals(CellPassTests.ATestCellPass2()), "Replaced pass is not the expected pass comapred with");
        }

        /// <summary>
        /// Test removal of a specific pass from the list of cell passes
        /// </summary>
        [TestMethod]
        public void Test_Cell_RemoveCellPass()
        {
            Cell c = new Cell(0);

            // Add a cell pass then replace it with a second cell pass
            c.AddPass(CellPassTests.ATestCellPass());
            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.IsTrue(c.PassCount == 2, "Cell pass list does not contain expected count of passes (2)");

            c.RemovePass(0);
            Assert.IsTrue(c.PassCount == 1, "Cell pass list does not contain expected count of passes (1) after removal");

            // Check the remaining pass is the one we think it is
            Assert.IsTrue(c.Passes[0].Equals(CellPassTests.ATestCellPass2()), "Remaining pass after removal is not the expected pass");
        }


        /// <summary>
        /// Test integration of the cell passes from one cell into the cell passes of another
        /// </summary>
        [TestMethod]
        public void Test_Cell_IntegrateCells_SingleCellPasses_NoModified()
        {
            // Create cells with a single (different) cell pass in each
            CellPass cp1 = CellPassTests.ATestCellPass();
            CellPass cp2 = CellPassTests.ATestCellPass2();

            Cell c1 = new Cell(0);
            c1.AddPass(cp1);

            Cell c2 = new Cell(0);
            c2.AddPass(cp2);

            int addedCount = 0;
            int modifiedCount = 0;

            // Test integration of later cell pass to list with earlier cell pass resulting in 1 added and 0 modified
            c1.Integrate(c2, 0, c2.PassCount - 1, out addedCount, out modifiedCount);

            Assert.IsTrue(addedCount == 1, "Count of added cell pases is not 1, but {0}", addedCount);
            Assert.IsTrue(modifiedCount == 0, "Count of modified cell pases is not 0, but {0}", modifiedCount);

            Assert.IsTrue(c2.PassCount == 1 && c2.Passes[0].Equals(cp2), "Integration unexpectedly modified source cell");
            Assert.IsTrue(c1.PassCount == 2 && c1.Passes[0].Equals(cp1) && c1.Passes[1].Equals(cp2),
                          "Result of integration two cells with single passes does not contain two passes of the expected content");
        }

        /// <summary>
        /// Test integration of the cell passes from one cell into the cell passes of another
        /// </summary>
        [TestMethod]
        public void Test_Cell_IntegrateCells_SingleCellPasses_Modified()
        {
            // Create cells with the same single cell pass in each
            CellPass cp1 = CellPassTests.ATestCellPass();

            Cell c1 = new Cell(0);
            c1.AddPass(cp1);

            Cell c2 = new Cell(0);
            c2.AddPass(cp1);

            int addedCount = 0;
            int modifiedCount = 0;

            // Test integration of the identical cell passes resulting in a single cell pass, 0 added and 0 modified (as identical cell passes are not a modification)
            c1.Integrate(c2, 0, c2.PassCount - 1, out addedCount, out modifiedCount);

            Assert.IsTrue(addedCount == 0, "Count of added cell pases is not 0, but {0}", addedCount);
            Assert.IsTrue(modifiedCount == 0, "Count of modified cell pases is not 0, but {0}", modifiedCount);

            Assert.IsTrue(c2.PassCount == 1 && c2.Passes[0].Equals(cp1), "Integration unexpectedly modified source cell");
            Assert.IsTrue(c1.PassCount == 1 && c1.Passes[0].Equals(cp1),
                          "Result of integration two cells with the same single passes does not contain a single pass of the expected content");

            // Modify the cell pass in cell 2 to have a different machime ID, but same remaining state to determing if the modification count is set

            c2.Passes[0].MachineID = 100000;

            // Test integration of the identical cell passes resulting in a single cell pass, 0 added and 1 modified (as identical cell passes are not a modification)
            c1.Integrate(c2, 0, c2.PassCount - 1, out addedCount, out modifiedCount);

            Assert.IsTrue(addedCount == 0, "Count of added cell pases is not 0, but {0}", addedCount);
            Assert.IsTrue(modifiedCount == 1, "Count of modified cell pases is not 1, but {0}", modifiedCount);

            Assert.IsTrue(c1.PassCount == 1, "Result of integration two cells with the same single passes does not contain a single pass");
        }

    }

}