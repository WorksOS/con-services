using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class SubgridCellAddressTests
    {
        [TestMethod]
        public void Test_SubgridCellAddress_Creation()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            // Check the simple style of creation is OK
            Assert.IsTrue(ca.X == 1 && ca.Y == 1 && ca.ProdDataRequested == false && ca.SurveyedSurfaceDataRequested == false,
                "Simple cell address creation did not set properties as expected");

            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1, true, true);

            // Check the simple style of creation is OK
            Assert.IsTrue(ca2.X == 1 && ca2.Y == 1 && ca2.ProdDataRequested == true && ca2.SurveyedSurfaceDataRequested == true,
                "Complete cell address creation did not set properties as expected");
        }

        [TestMethod]
        public void Test_SubgridCellAddress_FlagManagement()
        {
            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1, true, true);

            // Check setting/clearing the bit flag based properties
            ca2.ProdDataRequested = false;
            Assert.IsTrue(ca2.ProdDataRequested == false, "ProdDataRequested did not clear as expected");
            ca2.ProdDataRequested = true;
            Assert.IsTrue(ca2.ProdDataRequested == true, "ProdDataRequested did not set as expected");

            ca2.SurveyedSurfaceDataRequested = false;
            Assert.IsTrue(ca2.SurveyedSurfaceDataRequested == false, "SurveyedSurfaceDataRequested did not clear as expected");
            ca2.SurveyedSurfaceDataRequested = true;
            Assert.IsTrue(ca2.SurveyedSurfaceDataRequested == true, "SurveyedSurfaceDataRequested did not set as expected");
        }

        [TestMethod]
        public void Test_SubgridCellAddress_ToString()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            Assert.IsTrue(ca.ToString() == "1:1", "ToString returned unexpected result: {0}", ca.ToString());
        }

        [TestMethod]
        public void Test_SubgridCellAddress_Equality()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);
            SubGridCellAddress ca2 = new SubGridCellAddress(1, 1);
            SubGridCellAddress ca3 = new SubGridCellAddress(3, 3);

            Assert.IsTrue(ca.Equals(ca2), "Equality check between identical addresses failed");
            Assert.IsFalse(ca.Equals(ca3), "Inequality between different addresses failed");
        }

        [TestMethod]
        public void Test_SubgridCellAddress_ToNormalisedInt64()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            long correct = ((long)1 << 32) | 1;
            Assert.IsTrue(ca.ToNormalisedInt64 == correct, "Normalised descriptor for 1:1 incorrect (is {0} instead of {1})", ca.ToNormalisedInt64, correct);
        }

        [TestMethod]
        public void Test_SubgridCellAddress_ToNormalisedSubgridOriginInt64()
        {
            SubGridCellAddress ca = new SubGridCellAddress(1, 1);

            long correct = 0;
            Assert.IsTrue(ca.ToNormalisedSubgridOriginInt64 == correct, "Normalised descriptor for 1:1 incorrect (is {0} instead of {1})", ca.ToNormalisedSubgridOriginInt64, correct);

            SubGridCellAddress ca2 = new SubGridCellAddress(1 << 5, 1 << 5);

            long correct2 = ((long)1 << 25) | 1;
            Assert.IsTrue(ca2.ToNormalisedSubgridOriginInt64 == correct2, "Normalised descriptor for 1<<5:1<<5 incorrect (is {0} instead of {1})", ca2.ToNormalisedSubgridOriginInt64, correct2);
        }

        [TestMethod]
        public void Test_SubgridCellAddress_ToSkipInterleavedDescriptor()
        {
            SubGridCellAddress ca = null;
            long correct;

            ca = new SubGridCellAddress(1, 1);
            correct = 1;
            Assert.IsTrue(ca.ToSkipInterleavedDescriptor == correct, "SkipInterleaved descriptor for 1:1 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1);
            correct = (1 << 5) | 1;
            Assert.IsTrue(ca.ToSkipInterleavedDescriptor == correct, "SkipInterleaved descriptor for 1<<5:1 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1 << 5);
            correct = 1 << 5;
            Assert.IsTrue(ca.ToSkipInterleavedDescriptor == correct, "SkipInterleaved descriptor for 1<<5:1<<5 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedDescriptor, correct);
        }

        [TestMethod]
        public void Test_SubgridCellAddress_ToSkipInterleavedSubgridOriginDescriptor()
        {
            SubGridCellAddress ca = null;
            long correct;

            ca = new SubGridCellAddress(1, 1);
            correct = 0;
            Assert.IsTrue(ca.ToSkipInterleavedSubgridOriginDescriptor == correct, "ToSkipInterleavedSubgridOriginDescriptor descriptor for 1:1 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1);
            correct = 0;
            Assert.IsTrue(ca.ToSkipInterleavedSubgridOriginDescriptor == correct, "ToSkipInterleavedSubgridOriginDescriptor descriptor for 1<<5:1 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1, 1 << 5);
            correct = 1;
            Assert.IsTrue(ca.ToSkipInterleavedSubgridOriginDescriptor == correct, "ToSkipInterleavedSubgridOriginDescriptor descriptor for 1:1<<5 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedSubgridOriginDescriptor, correct);

            ca = new SubGridCellAddress(1 << 5, 1 << 5);
            correct = 1;
            Assert.IsTrue(ca.ToSkipInterleavedSubgridOriginDescriptor == correct, "ToSkipInterleavedSubgridOriginDescriptor descriptor for 1<<5:1<<5 incorrect (is {0} instead of {1})", ca.ToSkipInterleavedSubgridOriginDescriptor, correct);
        }   
    }
}
