using System;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Tests.netcore.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests
{
        public class HeightClientLeafSubGridTests : IClassFixture<DILoggingFixture>
    {
        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_Creation()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_Assign()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_CellHasValue()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_Clear()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_DumpToLog()
        {
            Assert.True(false,"Not implemented");
        }
        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_Read()
        {
            Assert.True(false,"Not implemented");
        }
        [Fact(Skip = "Not Implemented")]
        public void Test_HeightClientLeafSubGridTests_Write()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact]
        public void Test_HeightClientLeafSubGridTests_SetToZero()
        {
            ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1, SubGridTree.DefaultIndexOriginOffset);

            subgrid.SetToZeroHeight();

            Assert.Equal((uint)subgrid.CountNonNullCells(), SubGridTree.CellsPerSubgrid);

            int NumEqualZero = 0;
            ClientHeightLeafSubGrid.ForEach((x, y) => { if (subgrid.Cells[x, y] == 0.0) NumEqualZero++; });

            Assert.Equal((uint)NumEqualZero, SubGridTree.CellsPerSubgrid);
        }
    }
}
