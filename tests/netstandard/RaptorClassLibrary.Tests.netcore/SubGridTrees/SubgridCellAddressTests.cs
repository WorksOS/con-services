using System;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests
{
        public class SubgridCellAddressTests
    {
        [Fact]
        public void Test_SubgridCellAddress_Creation()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            // Check the simple style of creation is OK
            Assert.True(ca.X == 1 && ca.Y == 1 && ca.ProdDataRequested == false && ca.SurveyedSurfaceDataRequested == false,
                "Simple cell address creation did not set properties as expected");

            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1, true, true);

            // Check the simple style of creation is OK
            Assert.True(ca2.X == 1 && ca2.Y == 1 && ca2.ProdDataRequested == true && ca2.SurveyedSurfaceDataRequested == true,
                "Complete cell address creation did not set properties as expected");
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

            long correct = ((long)1 << 32) | 1;
            Assert.Equal(ca.ToNormalisedInt64, correct);
        }

        [Fact]
        public void Test_SubgridCellAddress_ToNormalisedSubgridOriginInt64()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            long correct = 0;
            Assert.Equal(ca.ToNormalisedSubgridOriginInt64, correct);

            SubGridCellAddress ca2 = new SubGridCellAddress(1 << 5, 1 << 5);

            long correct2 = ((long)1 << 25) | 1;
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
    }
}
