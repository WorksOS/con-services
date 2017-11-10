using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Velociraptor.DesignProfiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor;
using VSS.Velociraptor.Designs.TTM;

namespace VSS.Velociraptor.DesignProfiling.Tests
{
    [TestClass()]
    public class TTMDesignTests
    {
        private static TTMDesign _design = null;

        private static TTMDesign LoadTheDesign()
        {
            if (_design == null)
            {
                _design = new TTMDesign(SubGridTree.DefaultCellSize);
                _design.LoadFromFile(@"C:\Temp\Bug36372.ttm");
            }

            return _design;
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
            TTMDesign design = LoadTheDesign();

            design.GetExtents(out double x1, out double y1, out double x2, out double y2);

            Assert.IsTrue(x1 != Consts.NullReal, "X1 is null");
            Assert.IsTrue(y1 != Consts.NullReal, "Y1 is null");
            Assert.IsTrue(x2 != Consts.NullReal, "X2 is null");
            Assert.IsTrue(y2 != Consts.NullReal, "Y2 is null");
        }

        [TestMethod()]
        public void GetHeightRangeTest()
        {
            TTMDesign design = LoadTheDesign();

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
        public void InterpolateHeightTest()
        {
            TTMDesign design = LoadTheDesign();

            object Hint = null;
            bool result = design.InterpolateHeight(null, ref Hint, 247500.0, 193350.0, 0, out double Z);

            Assert.IsTrue(result, "Height interpolation returned false");
            Assert.IsTrue(Z != Consts.NullReal, "Interpolated heighth value is null");
        }

        [TestMethod()]
        public void InterpolateHeightsTest()
        {
            TTMDesign design = LoadTheDesign();

            float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            bool result = design.InterpolateHeights(null, Patch, 247500.0, 193350.0, SubGridTree.DefaultCellSize, 0);

            Assert.IsTrue(result, "Heights interpolation returned false");
//            Assert.IsTrue(Z != Consts.NullReal, "Interpolated heighth value is null");
        }

        [TestMethod()]
        public void LoadFromFileTest()
        {
            TTMDesign design = LoadTheDesign();

            Assert.IsTrue(design.Data.Triangles.Count > 0, "No triangles present in loaded TTM file.");
            Assert.IsTrue(design.Data.Vertices.Count > 0, "No vertices present in loaded TTM file.");
        }

        [TestMethod()]
        public void SubgridOverlayIndexTest()
        {
            TTMDesign design = LoadTheDesign();

            Assert.IsTrue(design.SubgridOverlayIndex() != null, "SubgridOverlayIndex is null");
        }
    }
}