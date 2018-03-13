using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Velociraptor.DesignProfiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor;
using VSS.Velociraptor.Designs.TTM;
using VSS.VisionLink.Raptor.Designs;

namespace VSS.Velociraptor.DesignProfiling.Tests
{
    [TestClass()]
    public class TTMDesignTests
    {
        private static TTMDesign design = LoadTheDesign();

        private static TTMDesign LoadTheDesign()
        {
            if (design == null)
            {
                design = new TTMDesign(SubGridTree.DefaultCellSize);
                design.LoadFromFile(@"C:\Temp\Bug36372.ttm");
            }

            return design;
        }

        [Ignore()]
        [TestMethod()]
        public void CreateAccessContextTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void ComputeFilterPatchTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TTMDesignTest()
        {
            try
            {
                TTMDesign design = new TTMDesign(SubGridTree.DefaultCellSize);

                Assert.IsTrue(design != null, "Creation of TTM design class instance failed");
            }
            catch (Exception E)
            {
                Assert.Fail($"Exception {E} occurred");
            }
        }

        [TestMethod()]
        public void GetExtentsTest()
        {

            design.GetExtents(out double x1, out double y1, out double x2, out double y2);

            Assert.IsTrue(x1 != Consts.NullReal, "X1 is null");
            Assert.IsTrue(y1 != Consts.NullReal, "Y1 is null");
            Assert.IsTrue(x2 != Consts.NullReal, "X2 is null");
            Assert.IsTrue(y2 != Consts.NullReal, "Y2 is null");
        }

        [TestMethod()]
        public void GetHeightRangeTest()
        {
//            TTMDesign design = LoadTheDesign();

            design.GetHeightRange(out double z1, out double z2);

            Assert.IsTrue(z1 != Consts.NullReal, "Z1 is null");
            Assert.IsTrue(z2 != Consts.NullReal, "Z2 is null");
            Assert.IsTrue(z2 >= z1, "Z2 is below Z1");
        }

        [Ignore()]
        [TestMethod()]
        public void HasElevationDataForSubGridPatchTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void HasElevationDataForSubGridPatchTest1()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void HasFiltrationDataForSubGridPatchTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void HasFiltrationDataForSubGridPatchTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [DataRow(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest(double probeX, double probeY, double expectedZ)
        {
//            TTMDesign design = LoadTheDesign();

            object Hint = null;

            TriangleQuadTree.Tsearch_state_rec SearchState = TriangleQuadTree.Tsearch_state_rec.Init();

            bool result = design.InterpolateHeight(ref SearchState, ref Hint, probeX, probeY, 0, out double Z);

            Assert.IsTrue(result, "Height interpolation returned false");

            Assert.IsTrue(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [TestMethod()]
        [DataRow(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTestPerf(double probeX, double probeY, double expectedZ)
        {
            //            TTMDesign design = LoadTheDesign();
            object Hint = null;

            TriangleQuadTree.Tsearch_state_rec SearchState = TriangleQuadTree.Tsearch_state_rec.Init();
            for (int i = 0; i < 1000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight(ref SearchState, ref Hint, probeX, probeY, 0, out double Z);
                Assert.IsTrue(result);
            }

            Assert.Fail("Perf Test");
        }

        [TestMethod()]
        [DataRow(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest2(double probeX, double probeY, double expectedZ)
        {
            //            TTMDesign design = LoadTheDesign();
            object Hint = null;
            bool result = design.InterpolateHeight2(ref Hint, probeX, probeY, 0, out double Z);

            Assert.IsTrue(result, "Height interpolation returned false");

            Assert.IsTrue(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [TestMethod()]
        [DataRow(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest2Perf(double probeX, double probeY, double expectedZ)
        {
            object Hint = null;
            for (int i = 0; i < 10000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight2(ref Hint, probeX, probeY, 0, out double Z);
//                Assert.IsTrue(result);
            }

            Assert.Fail("Perf Test");
        }

        [TestMethod()]
        [DataRow(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest3Perf(double probeX, double probeY, double expectedZ)
        {
            object Hint = null;
            for (int i = 0; i < 10000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight3(ref Hint, probeX, probeY, 0, out double Z);
//                Assert.IsTrue(result);
            }

            Assert.Fail("Perf Test");
        }

        [TestMethod()]
        public void InterpolateHeightsTest()
        {
//            TTMDesign design = LoadTheDesign();
            design.QuadTreeSpatialIndex.dump_tree(" C# ");


            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            bool result = design.InterpolateHeights(Patch, 247500.0, 193350.0, SubGridTree.DefaultCellSize, 0);

            Assert.IsTrue(result, "Heights interpolation returned false");

//            Assert.IsTrue(Z != Consts.NullReal, "Interpolated heighth value is null");
        }

        [TestMethod()]
        public void LoadFromFileTest()
        {
            Assert.IsTrue(design.Data.Triangles.Count > 0, "No triangles present in loaded TTM file.");
            Assert.IsTrue(design.Data.Vertices.Count > 0, "No vertices present in loaded TTM file.");
        }

        [TestMethod()]
        public void SubgridOverlayIndexTest()
        {
            Assert.IsTrue(design.SubgridOverlayIndex() != null, "SubgridOverlayIndex is null");
        }
    }
}