using VSS.Velociraptor.DesignProfiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor;
using VSS.Velociraptor.Designs.TTM;
using VSS.VisionLink.Raptor.Designs;
using Xunit;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.Velociraptor.DesignProfiling.Tests
{
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

        [Fact(Skip = "not implemented")]
        public void CreateAccessContextTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void ComputeFilterPatchTest()
        {
        }

        [Fact()]
        public void TTMDesignTest()
        {
            try
            {
                TTMDesign design = new TTMDesign(SubGridTree.DefaultCellSize);

                Assert.NotNull(design);
            }
            catch (Exception E)
            {
                Assert.False(true);
            }
        }

        [Fact()]
        public void GetExtentsTest()
        {

            design.GetExtents(out double x1, out double y1, out double x2, out double y2);

            Assert.NotEqual(x1, Consts.NullReal);
            Assert.NotEqual(y1, Consts.NullReal);
            Assert.NotEqual(x2, Consts.NullReal);
            Assert.NotEqual(y2, Consts.NullReal);
        }

        [Fact()]
        public void GetHeightRangeTest()
        {
//            TTMDesign design = LoadTheDesign();

            design.GetHeightRange(out double z1, out double z2);

            Assert.NotEqual(z1, Consts.NullReal);
            Assert.NotEqual(z2, Consts.NullReal);
            Assert.True(z2 >= z1, "Z2 is below Z1");
        }

        [Fact(Skip = "not implemented")]
        public void HasElevationDataForSubGridPatchTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void HasElevationDataForSubGridPatchTest1()
        {
        }

        [Fact(Skip = "not implemented")]
        public void HasFiltrationDataForSubGridPatchTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void HasFiltrationDataForSubGridPatchTest1()
        {
        }

        [Theory]
        [InlineData(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest(double probeX, double probeY, double expectedZ)
        {
            object Hint = null;

            bool result = design.InterpolateHeight(ref Hint, probeX, probeY, 0, out double Z);

            Assert.True(result, "Height interpolation returned false");

            Assert.True(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [Theory]
        [InlineData(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest1(double probeX, double probeY, double expectedZ)
        {
            object Hint = null;

            TriangleQuadTree.Tsearch_state_rec SearchState = TriangleQuadTree.Tsearch_state_rec.Init();

            bool result = design.InterpolateHeight1(ref SearchState, ref Hint, probeX, probeY, 0, out double Z);

            Assert.True(result, "Height interpolation returned false");

            Assert.True(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [Theory()]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightTestPerf1(double probeX, double probeY)
        {
            object Hint = null;

            TriangleQuadTree.Tsearch_state_rec SearchState = TriangleQuadTree.Tsearch_state_rec.Init();
            for (int i = 0; i < 1000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight1(ref SearchState, ref Hint, probeX, probeY, 0, out double Z);
                Assert.True(result);
            }

            Assert.False(true, "Perf Test");
        }

        [Theory()]
        [InlineData(247500.0, 193350.0, 29.875899875665258)]
        public void InterpolateHeightTest2(double probeX, double probeY, double expectedZ)
        {
            object Hint = null;
            bool result = design.InterpolateHeight2(ref Hint, probeX, probeY, 0, out double Z);

            Assert.True(result, "Height interpolation returned false");

            Assert.True(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [Theory()]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightTest2Perf(double probeX, double probeY)
        {
            object Hint = null;
            for (int i = 0; i < 10000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight2(ref Hint, probeX, probeY, 0, out double Z);
            }

            Assert.False(true,"Perf Test");
        }

        [Theory()]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightTest3Perf(double probeX, double probeY)
        {
            object Hint = null;
            for (int i = 0; i < 10000000; i++)
            {
                Hint = null;
                bool result = design.InterpolateHeight3(ref Hint, probeX, probeY, 0, out double Z);
            }

            Assert.False(true, "Perf Test");
        }

        [Theory()]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTest(Double probeX, Double probeY)
        {
            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            bool result = design.InterpolateHeights(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);

            Assert.True(result, "Heights interpolation returned false");
        }

        [Theory(Skip = "Performance Test")]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTestPerf(Double probeX, Double probeY)
        {

            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            
            for (int i = 0; i < 10000; i++)
            {
                bool result = design.InterpolateHeights(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);
            }

            Assert.False(true, "Perf Test");
        }

        [Theory(Skip = "Performance Test")]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTest2Perf(Double probeX, Double probeY)
        {

            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            for (int i = 0; i < 10000; i++)
            {
                bool result = design.InterpolateHeights2(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);
            }

            Assert.False(true, "Perf Test");
        }

        [Theory(Skip = "Performance Test")]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTest3Perf(Double probeX, Double probeY)
        {

            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            for (int i = 0; i < 10000; i++)
            {
                bool result = design.InterpolateHeights3(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);
            }

            Assert.False(true, "Perf Test");
        }

        [Fact()]
        public void LoadFromFileTest()
        {
            Assert.True(design.Data.Triangles.Count > 0, "No triangles present in loaded TTM file.");
            Assert.True(design.Data.Vertices.Count > 0, "No vertices present in loaded TTM file.");
        }

        [Fact()]
        public void SubgridOverlayIndexTest()
        {
            Assert.NotNull(design.SubgridOverlayIndex());
        }
    }
}