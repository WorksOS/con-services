using System;
using System.IO;
using System.Text;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
    public class GenericLeafSubGridTests : IClassFixture<DILoggingFixture>
    {
        [Fact]
        public void Test_GenericLeafSubGridTests_Creation()
        {
            var generic = new GenericLeafSubGrid<bool>();
            generic.Should().NotBeNull();
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Creation_WithItems()
        {
          var items = new bool[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
          var generic = new GenericLeafSubGrid<bool>(items);

          generic.Should().NotBeNull();
          generic.Items.Should().BeEquivalentTo(items);
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Creation_WithinTree()
        {
          var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());
          var generic = new GenericLeafSubGrid<bool>(tree, null, SubGridTreeConsts.SubGridTreeLevels);

          generic.Should().NotBeNull();
          generic.Owner.Should().Be(tree);
          generic.Parent.Should().Be(null);
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_ForEach_ActionFunctor()
        {
          var generic = new GenericLeafSubGrid<bool>(null, null, SubGridTreeConsts.SubGridTreeLevels);

          // All bools in generic are false..

          // Test index based ForEach
          uint count = 0;
          generic.ForEach((x, y) =>
          {
            if (!generic.Items[x, y])
              count++;
          });

          count.Should().Be(SubGridTreeConsts.CellsPerSubGrid);
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_ForEach_FunctionDelegate()
        {
          var generic = new GenericLeafSubGrid<bool>(null, null, SubGridTreeConsts.SubGridTreeLevels);

          // All bools in generic are false..
          // Iterate until completion...
          uint count = 0;
          generic.ForEach(b =>
          {
            if (!b) count++;
            return true;
          });

          count.Should().Be(SubGridTreeConsts.CellsPerSubGrid);

          // Terminate iteration after first element
          count = 0;
          generic.ForEach(b =>
          {
            if (!b) count++;
            return false;
          });

          count.Should().Be(1);
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Clear()
        {
          var generic = new GenericLeafSubGrid<bool>(null, null, SubGridTreeConsts.SubGridTreeLevels);

          generic.ForEach((x, y) => generic.Items[x, y] = true);
          generic.Clear();
          generic.ForEach((x, y) => generic.Items[x, y].Should().BeFalse());
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Read_BinaryReader()
        {
            ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<double>>());
            GenericLeafSubGrid<double> subgrid = new GenericLeafSubGrid<double>(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // This is not implemented and should throw an exception. Override to implement...
            try
            {
                subgrid.Read(new BinaryReader(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION), Encoding.UTF8, true), new byte[10000]);
                Assert.True(false,"Read with BinaryReader did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Write_BinaryWriter()
        {
            ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<double>>());
            GenericLeafSubGrid<double> subgrid = new GenericLeafSubGrid<double>(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // This is not implemented and should throw an exception. Override to implement...
            try
            {
                subgrid.Write(new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION)), new byte[10000]);
                Assert.True(false,"Read with BinaryWrite did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_CellHasValue()
        {
          // Note: By definition, base generic cell has value behaviour is to assume the value exists
          var generic = new GenericLeafSubGrid<bool>();
          SubGridUtilities.SubGridDimensionalIterator((x, y) => generic.CellHasValue((byte)x, (byte)y).Should().Be(true));
        }
    }
}
