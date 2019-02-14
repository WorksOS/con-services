using System;
using System.IO;
using System.Text;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class GenericLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
        [Fact(Skip = "Not Implemented")]
        public void Test_GenericLeafSubGridTests_Creation()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_GenericLeafSubGridTests_ForEach()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_GenericLeafSubGridTests_Clear()
        {
            Assert.True(false,"Not implemented");
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Read_BinaryReader()
        {
            ISubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<double>>());
            GenericLeafSubGrid<double> subgrid = new GenericLeafSubGrid<double>(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            // This is not implemented and should throw an exception. Override to implement...
            try
            {
                subgrid.Read(new BinaryReader(new MemoryStream(), Encoding.UTF8, true), new byte[10000]);
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
                subgrid.Write(new BinaryWriter(new MemoryStream()), new byte[10000]);
                Assert.True(false,"Read with BinaryWrite did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }
        }
    }
}
