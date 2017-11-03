using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class HeightClientLeafSubGridTests
    {
        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_Creation()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_Assign()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_CellHasValue()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_Clear()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_DumpToLog()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_Read()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_Write()
        {
            Assert.Fail("Not implemented");
        }

        [TestMethod]
        public void Test_HeightClientLeafSubGridTests_SetToZero()
        {
            ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1, SubGridTree.DefaultIndexOriginOffset);

            subgrid.SetToZeroHeight();

            Assert.IsTrue(subgrid.CountNonNullCells() == SubGridTree.CellsPerSubgrid, "Not all cells set to non-null");

            int NumEqualZero = 0;
            ClientHeightLeafSubGrid.ForEach((x, y) => { if (subgrid.Cells[x, y] == 0.0) NumEqualZero++; });

            Assert.IsTrue(NumEqualZero == SubGridTree.CellsPerSubgrid, "Not all cells set to zero");
        }
    }
}
