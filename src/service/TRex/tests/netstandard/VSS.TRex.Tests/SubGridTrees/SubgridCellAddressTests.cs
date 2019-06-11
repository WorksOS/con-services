using FluentAssertions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubgridCellAddressTests : IClassFixture<DILoggingFixture>
    {
        [Fact]
        public void Test_SubgridCellAddress_Creation()
        {
            SubGridCellAddress ca = new SubGridCellAddress();

            // Check the simple style of creation is OK
            Assert.True(ca.X == 0 && ca.Y == 0 && ca.ProdDataRequested == false && ca.SurveyedSurfaceDataRequested == false,
              "Default simple cell address creation did not set properties as expected");

            ca = new SubGridCellAddress(1, 1);

            // Check the simple style of creation is OK
            Assert.True(ca.X == 1 && ca.Y == 1 && ca.ProdDataRequested == false && ca.SurveyedSurfaceDataRequested == false,
                "Simple cell address creation did not set properties as expected");

            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1, true, true);

            // Check the simple style of creation is OK
            Assert.True(ca2.X == 1 && ca2.Y == 1 && ca2.ProdDataRequested == true && ca2.SurveyedSurfaceDataRequested == true,
                "Complete cell address creation did not set properties as expected");
        }

        [Fact]
        public void Test_SubgridCellAddress_Set()
        {
          SubGridCellAddress ca = new SubGridCellAddress();

          ca.Set(1, 2, true, false);

          // Check the simple style of creation is OK
          Assert.True(ca.X == 1 && ca.Y == 2 && ca.ProdDataRequested == true && ca.SurveyedSurfaceDataRequested == false,
            "Setting simple cell address not set properties as expected");
        }

    [Fact]
        public void Test_SubgridCellAddress_FlagManagement()
        {
            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1, true, true);

            // Check setting/clearing the bit flag based properties
            ca2.ProdDataRequested = false;
            Assert.False(ca2.ProdDataRequested);
            ca2.ProdDataRequested = true;
            Assert.True(ca2.ProdDataRequested);

            ca2.SurveyedSurfaceDataRequested = false;
            Assert.False(ca2.SurveyedSurfaceDataRequested);
            ca2.SurveyedSurfaceDataRequested = true;
            Assert.True(ca2.SurveyedSurfaceDataRequested);
        }

        [Fact]
        public void Test_SubgridCellAddress_ToString()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            Assert.Equal("1:1", ca.ToString());
        }

        [Fact]
        public void Test_SubgridCellAddress_Equality()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);
            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1);
            SubGridCellAddress ca3 = new SubGridCellAddress(3, 3);

            Assert.True(ca.Equals(ca2), "Equality check between identical addresses failed");
            Assert.False(ca.Equals(ca3), "Inequality between different addresses failed");
        }

        [Fact]
        public void Test_SubgridCellAddress_ToNormalisedInt64()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            long correct = (1L << 32) | 1;
            Assert.Equal(ca.ToNormalisedInt64, correct);
        }

        [Fact]
        public void Test_SubgridCellAddress_ToNormalisedSubgridOriginInt64()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            long correct = 0;
            Assert.Equal(ca.ToNormalisedSubgridOriginInt64, correct);

            SubGridCellAddress ca2 = new SubGridCellAddress(1 << 5, 1 << 5);

            long correct2 = (1L << 25) | 1;
            Assert.Equal(ca2.ToNormalisedSubgridOriginInt64, correct2);
        }

        [Fact]
        public void Test_SubgridCellAddress_ToSkipInterleavedDescriptor()
        {
            SubGridCellAddress ca;
            long correct;

            ca = new SubGridCellAddress(1, 1);
            correct = 1;
            Assert.Equal(ca.ToSkipInterleavedDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1);
            correct = (1 << 5) | 1;
            Assert.Equal(ca.ToSkipInterleavedDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1 << 5);
            correct = 1 << 5;
            Assert.Equal(ca.ToSkipInterleavedDescriptor, correct);
        }

        [Fact]
        public void Test_SubgridCellAddress_ToSkipInterleavedSubgridOriginDescriptor()
        {
            SubGridCellAddress ca;
            long correct;

            ca = new SubGridCellAddress(1, 1);
            correct = 0;
            Assert.Equal(ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1);
            correct = 0;
            Assert.Equal(ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1, 1 << 5);
            correct = 1;
            Assert.Equal(ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1 << 5);
            correct = 1;
            Assert.Equal(ca.ToSkipInterleavedSubgridOriginDescriptor, correct);
        }

        [Theory]
        [InlineData(1000, 1000, 1023)]
        [InlineData(10_000, 10_000, 792)]
        [InlineData(100_000, 100_000, 693)]
        [InlineData(1_000_000, 1_000_000, 594)]
        [InlineData(10_000_000, 10_000_000, 660)]
        [InlineData(100_000_000, 100_000_000, 264)]
        [InlineData(1_000_000_000, 1_000_000_000, 528)]
        public void Test_SubgridCellAddress_ToSpatialPartitionDescriptor(int cellX, int cellY, int expectedPartition)
        {
          // These tests assume the default number of 1024 partitions from the configuration

          // Test the instance method
          SubGridCellAddress ca = new SubGridCellAddress(cellX, cellY);
          ca.ToSpatialPartitionDescriptor().Should().Be(expectedPartition);

          // Test the static method
          SubGridCellAddress.ToSpatialPartitionDescriptor(cellX, cellY).Should().Be(expectedPartition);
        }
    }
}
