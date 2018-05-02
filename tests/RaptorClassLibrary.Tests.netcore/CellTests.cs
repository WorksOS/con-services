using System;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Cells;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class CellTests
    {
        /// <summary>
        /// Ensure the IsEmpty mechanism reports the cell empty of cell passes
        /// </summary>
        [Fact]
        public void Test_Cell_EmptyCellPassesOnCreation()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            Assert.Equal((uint)0, c.PassCount);
            Assert.True(c.IsEmpty, "Cell does not report itself as being empty of cell passes");
        }

        /// <summary>
        /// Ensure the passcount allocation mechanism creates the appropriate number of entries
        /// </summary>
        [Fact]
        public void Test_Cell_CellPassAllocation()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            c.AllocatePasses(10);

            Assert.Equal((uint)10, c.PassCount);
        }

        /// <summary>
        /// Ensure the topmostheight mechanism creates the appropriate number of entries when added in an ordered manner
        /// </summary>
        [Fact]
        public void Test_Cell_AddCellPassOrdered()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            c.AddPass(CellPassTests.ATestCellPass());

            Assert.Equal((uint)1, c.PassCount);

            CellPass cp = CellPassTests.ATestCellPass();
            Assert.True(c.Passes[0].Equals(cp), "Added cell pass not the same content as it was constructed with");

            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.Equal((uint)2, c.PassCount);

            CellPass cp2 = CellPassTests.ATestCellPass2();
            Assert.True(c.Passes[1].Equals(cp2), "Added cell pass not the same content as it was constructed with");
        }

        /// <summary>
        /// Ensure the topmostheight mechanism creates the appropriate number of entries when added in an unordered manner
        /// </summary>
        [Fact]
        public void Test_Cell_AddCellPassUnOrdered()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.Equal((uint)1, c.PassCount);

            CellPass cp2 = CellPassTests.ATestCellPass2();
            Assert.True(c.Passes[0].Equals(cp2), "Added cell pass not the same content as it was constructed with");

            c.AddPass(CellPassTests.ATestCellPass());

            Assert.Equal((uint)2, c.PassCount);

            CellPass cp1 = CellPassTests.ATestCellPass();
            Assert.True(c.Passes[0].Equals(cp1), "Added cell pass not the same content as it was constructed with");
        }

        /// <summary>
        /// Ensure the topmostheight mechanism selects the correct 'top most' height in terms of 
        /// the height from the most recently recorded cell pass in time
        /// </summary>
        [Fact]
        public void Test_Cell_TopMostHeight()
        {
            Cell_NonStatic c1 = new Cell_NonStatic(0);

            // Add two cell passes in ascending order
            c1.AddPass(CellPassTests.ATestCellPass());
            c1.AddPass(CellPassTests.ATestCellPass2());

            Assert.Equal((uint)2, c1.PassCount);
            Assert.Equal(50, c1.TopMostHeight);

            // Add two cell passes in ascending order
            Cell_NonStatic c2 = new Cell_NonStatic(0);

            c2.AddPass(CellPassTests.ATestCellPass2());
            c2.AddPass(CellPassTests.ATestCellPass());

            Assert.Equal((uint)2, c2.PassCount);
            Assert.Equal(50, c2.TopMostHeight);
        }

        /// <summary>
        /// Check that cell passes are correctly located based on time
        /// </summary>
        [Fact]
        public void Test_Cell_LocateTime()
        {
            // Create a cell with two cell passes with different times
            Cell_NonStatic c = new Cell_NonStatic(0);
            CellPass cp1 = CellPassTests.ATestCellPass();
            CellPass cp2 = CellPassTests.ATestCellPass2();

            c.AddPass(cp1);
            c.AddPass(cp2);

            // Check cp1 and cp2 are present in that order
            Assert.True(c.Passes[0].Equals(cp1) && c.Passes[1].Equals(cp2), "Two passes added are not in expected order");

            // Locate both with the time present in each of the contributing cell passes to ensure exact time matches
            // at the boundaries are preserved

            c.LocateTime(cp1.Time, out int cellPassIndex);
            Assert.Equal(0, cellPassIndex);
            Assert.True(c.Passes[cellPassIndex].Equals(cp1), "Select (first) cell pass does not match pass added to cell");

            c.LocateTime(cp2.Time, out cellPassIndex);
            Assert.Equal(1, cellPassIndex);
            Assert.True(c.Passes[cellPassIndex].Equals(cp2), "Select (second) cell pass does not match pass added to cell");

            // Locate the cell passes with modified times to cause no exact match to text the 
            // returned insertion position is correct

            c.LocateTime(cp1.Time.AddMinutes(-1), out cellPassIndex);
            Assert.Equal(0, cellPassIndex);

            c.LocateTime(cp1.Time.AddMinutes(1), out cellPassIndex);
            Assert.Equal(1, cellPassIndex);

            c.LocateTime(cp2.Time.AddMinutes(1), out cellPassIndex);
            Assert.Equal(2, cellPassIndex);
        }

        /// <summary>
        /// Test replacing of one pass with anther
        /// </summary>
        [Fact]
        public void Test_Cell_ReplaceCellPass()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            // Add a cell pass then replace it with a second cell pass
            c.AddPass(CellPassTests.ATestCellPass());
            c.ReplacePass(0, CellPassTests.ATestCellPass2());

            Assert.True(c.Passes[0].Equals(CellPassTests.ATestCellPass2()), "Replaced pass is not the expected pass comapred with");
        }

        /// <summary>
        /// Test removal of a specific pass from the list of cell passes
        /// </summary>
        [Fact]
        public void Test_Cell_RemoveCellPass()
        {
            Cell_NonStatic c = new Cell_NonStatic(0);

            // Add a cell pass then replace it with a second cell pass
            c.AddPass(CellPassTests.ATestCellPass());
            c.AddPass(CellPassTests.ATestCellPass2());

            Assert.Equal((uint)2, c.PassCount);

            c.RemovePass(0);
            Assert.Equal((uint)1, c.PassCount);

            // Check the remaining pass is the one we think it is
            Assert.True(c.Passes[0].Equals(CellPassTests.ATestCellPass2()), "Remaining pass after removal is not the expected pass");
        }


        /// <summary>
        /// Test integration of the cell passes from one cell into the cell passes of another
        /// </summary>
        [Fact]
        public void Test_Cell_IntegrateCells_SingleCellPasses_NoModified()
        {
            // Create cells with a single (different) cell pass in each
            CellPass cp1 = CellPassTests.ATestCellPass();
            CellPass cp2 = CellPassTests.ATestCellPass2();

            Cell_NonStatic c1 = new Cell_NonStatic(0);
            c1.AddPass(cp1);

            Cell_NonStatic c2 = new Cell_NonStatic(0);
            c2.AddPass(cp2);

            // Test integration of later cell pass to list with earlier cell pass resulting in 1 added and 0 modified
            c1.Integrate(c2.Passes, 0, c2.PassCount - 1, out int addedCount, out int modifiedCount);

            Assert.Equal(1, addedCount);
            Assert.Equal(0, modifiedCount);

            Assert.True(c2.PassCount == 1 && c2.Passes[0].Equals(cp2), "Integration unexpectedly modified source cell");
            Assert.True(c1.PassCount == 2 && c1.Passes[0].Equals(cp1) && c1.Passes[1].Equals(cp2),
                          "Result of integration two cells with single passes does not contain two passes of the expected content");
        }

        /// <summary>
        /// Test integration of the cell passes from one cell into the cell passes of another
        /// </summary>
        [Fact]
        public void Test_Cell_IntegrateCells_SingleCellPasses_Modified()
        {
            // Create cells with the same single cell pass in each
            CellPass cp1 = CellPassTests.ATestCellPass();

            Cell_NonStatic c1 = new Cell_NonStatic(0);
            c1.AddPass(cp1);

            Cell_NonStatic c2 = new Cell_NonStatic(0);
            c2.AddPass(cp1);

            // Test integration of the identical cell passes resulting in a single cell pass, 0 added and 0 modified (as identical cell passes are not a modification)
            c1.Integrate(c2.Passes, 0, c2.PassCount - 1, out int addedCount, out int modifiedCount);

            Assert.Equal(0, addedCount);
            Assert.Equal(0, modifiedCount);

            Assert.True(c2.PassCount == 1 && c2.Passes[0].Equals(cp1), "Integration unexpectedly modified source cell");
            Assert.True(c1.PassCount == 1 && c1.Passes[0].Equals(cp1),
                          "Result of integration two cells with the same single passes does not contain a single pass of the expected content");

            // Modify the cell pass in cell 2 to have a different machime ID, but same remaining state to determing if the modification count is set

            //c2.Passes[0].MachineID = 10000;
            c2.Passes[0].InternalSiteModelMachineIndex = 10000;

            // Test integration of the identical cell passes resulting in a single cell pass, 0 added and 1 modified (as identical cell passes are not a modification)
            c1.Integrate(c2.Passes, 0, c2.PassCount - 1, out addedCount, out modifiedCount);

            Assert.Equal(0, addedCount);
            Assert.Equal(1, modifiedCount);

            Assert.Equal((uint)1, c1.PassCount);
        }

    }

}