using System;
using VSS.TRex.Designs;
using VSS.TRex.Tests.netcore.TestFixtures;
using Xunit;

namespace VSS.TRex.DesignProfiling.Tests
{
  public class TTMDesignTests : IClassFixture<DILoggingFixture>
  {
        private static TTMDesign design;

        private void LoadTheDesign()
        {
          lock (this)
          {
            if (design == null)
            {
              design = new TTMDesign(SubGridTree.DefaultCellSize);
              design.LoadFromFile(@"C:\Temp\Bug36372.ttm");
            }
          }
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
                TTMDesign localDesign = new TTMDesign(SubGridTree.DefaultCellSize);

                Assert.NotNull(localDesign);
            }
            catch (Exception)
            {
                Assert.False(true);
            }
        }

        [Fact()]
        public void GetExtentsTest()
        {
            LoadTheDesign();

            design.GetExtents(out double x1, out double y1, out double x2, out double y2);

            Assert.NotEqual(x1, Common.Consts.NullReal);
            Assert.NotEqual(y1, Common.Consts.NullReal);
            Assert.NotEqual(x2, Common.Consts.NullReal);
            Assert.NotEqual(y2, Common.Consts.NullReal);
        }

        [Fact()]
        public void GetHeightRangeTest()
        {
            LoadTheDesign();

            design.GetHeightRange(out double z1, out double z2);

            Assert.NotEqual(z1, Common.Consts.NullReal);
            Assert.NotEqual(z2, Common.Consts.NullReal);
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
            LoadTheDesign();

            object Hint = null;

            bool result = design.InterpolateHeight(ref Hint, probeX, probeY, 0, out double Z);

            Assert.True(result, "Height interpolation returned false");

            Assert.True(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
        }

        [Theory]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTest(double probeX, double probeY)
        {
            LoadTheDesign();
            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            bool result = design.InterpolateHeights(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);

            Assert.True(result, "Heights interpolation returned false");
        }

        [Theory(Skip = "Performance Test")]
        [InlineData(247500.0, 193350.0)]
        public void InterpolateHeightsTestPerf(double probeX, double probeY)
        {
            LoadTheDesign();

            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            
            for (int i = 0; i < 10000; i++)
                design.InterpolateHeights(Patch, probeX, probeY, SubGridTree.DefaultCellSize, 0);

            Assert.False(true, "Perf Test");
        }

        [Fact]
        public void LoadFromFileTest()
        {
            LoadTheDesign();

            Assert.True(design.Data.Triangles.Count > 0, "No triangles present in loaded TTM file.");
            Assert.True(design.Data.Vertices.Count > 0, "No vertices present in loaded TTM file.");
        }

        [Fact]
        public void SubgridOverlayIndexTest()
        {
            LoadTheDesign();

            Assert.NotNull(design.SubgridOverlayIndex());
        }
    }
}
