using System;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class HeightClientLeafSubGridTests
    {
        [Fact]
        public void Test_HeightClientLeafSubGridTests_Creation()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_Assign()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_CellHasValue()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_Clear()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_DumpToLog()
        {
            Assert.Fail("Not implemented");
        }
        [Fact]
        public void Test_HeightClientLeafSubGridTests_Read()
        {
            Assert.Fail("Not implemented");
        }
        [Fact]
        public void Test_HeightClientLeafSubGridTests_Write()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_SetToZero()
        {
            ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1, SubGridTree.DefaultIndexOriginOffset);

            subgrid.SetToZeroHeight();

            Assert.Equal(subgrid.CountNonNullCells(), SubGridTree.CellsPerSubgrid);

            int NumEqualZero = 0;
            ClientHeightLeafSubGrid.ForEach((x, y) => { if (subgrid.Cells[x, y] == 0.0) NumEqualZero++; });

            Assert.Equal(NumEqualZero, SubGridTree.CellsPerSubgrid);
        }
    }
}
