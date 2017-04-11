using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.Geometry.Tests
{
    [TestClass()]
    public class FenceTests
    {
        private Fence makeSimpleRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            return fence;
        }

        [TestMethod()]
        public void Test_FenceTests_Fence()
        {
            Fence fence = new Fence();

            Assert.IsFalse(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.IsFalse(fence.HasVertices, "New fence has vertices");
            Assert.IsTrue(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");

            Fence fence2 = new Fence(0, 0, 1, 1);
            Assert.IsTrue(fence2.NumVertices == 4 && fence2.Area() == 1.0);
        }

        [TestMethod()]
        public void Test_FenceTests_IncludesPoint()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.IsTrue(fence.IncludesPoint(50, 50), "Point (50, 50) not included");
            Assert.IsFalse(fence.IncludesPoint(150, 150), "Point (150, 150) is included");
            Assert.IsFalse(fence.IncludesPoint(-1, 0), "Point (-1, 0) is included");
        }

        [TestMethod()]
        public void Test_FenceTests_IncludesLine()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.IsTrue(fence.IncludesLine(50, 50, 75, 75), "Line (50, 50, 75, 75) not included");
            Assert.IsTrue(fence.IncludesLine(50, 50, 175, 75), "Line (50, 50, 175, 75) not included");
            Assert.IsTrue(fence.IncludesLine(-100, -100, 200, 200), "Line (-100, -100, 200, 200) not included");
            Assert.IsFalse(fence.IncludesLine(-100, -100, 0, 200), "Line (-100, -100, 0, 200) included");
        }

        [TestMethod()]
        public void Test_FenceTests_IntersectsExtent()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.IsTrue(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX, fence.MinY, fence.MaxX, fence.MaxY)), 
                          "Extents do not overlap");
            Assert.IsTrue(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 50, fence.MinY - 50, fence.MaxX - 50, fence.MaxY - 50)),
                          "Extents do not overlap");
            Assert.IsTrue(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 100, fence.MinY - 100, fence.MaxX - 100, fence.MaxY - 100)),
                          "Extents do not overlap");
            Assert.IsTrue(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 100, fence.MinY + 100, fence.MaxX + 100, fence.MaxY + 100)),
                          "Extents do not overlap");

            Assert.IsFalse(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 101, fence.MinY - 101, fence.MaxX - 101, fence.MaxY - 101)),
                           "Extents not overlap");
            Assert.IsFalse(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 101, fence.MinY + 101, fence.MaxX + 101, fence.MaxY + 101)),
                           "Extents not overlap");
        }

        [TestMethod()]
        public void Test_FenceTests_Initialise()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Initialise();

            Assert.IsFalse(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.IsFalse(fence.HasVertices, "New fence has vertices");
            Assert.IsTrue(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");
        }

        [TestMethod()]
        public void Test_FenceTests_Clear()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Clear();

            Assert.IsFalse(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.IsFalse(fence.HasVertices, "New fence has vertices");
            Assert.IsTrue(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");        
        }

        [TestMethod()]
        public void Test_FenceTests_IsSquare()
        {
            Fence fence = makeSimpleRectangleFence();
            Assert.IsTrue(fence.IsSquare, "Rectangle is not square");

            fence.Points[0].SetXY(-100, 100); // Not square any more
            fence.UpdateExtents();

            Assert.IsFalse(fence.IsSquare, "Rectangle is square");
        }

        [TestMethod()]
        public void Test_FenceTests_GetExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            double minx, miny, maxx, maxy;

            fence.GetExtents(out minx, out miny, out maxx, out maxy);

            Assert.IsTrue(minx == 0 && miny == 0 && maxx == 100 && maxy == 100,
                "Extracted extents are incorrect");
        }

        [TestMethod()]
        public void Test_FenceTests_SetExtents()
        {
            Fence fence = new Fence();
            fence.SetExtents(0, 0, 100, 100);

            Assert.IsTrue(fence.NumVertices == 4, "Incorrect number of vertices");

            Assert.IsTrue(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.IsTrue(fence.IsRectangle == true, "Not a rectangle");
        }

        [TestMethod()]
        public void Test_FenceTests_HasVertices()
        {
            Fence fence = new Fence();

            Assert.IsFalse(fence.HasVertices, "HasVertices incorrect");

            fence.SetExtents(0, 0, 100, 100);

            Assert.IsTrue(fence.HasVertices && fence.NumVertices == 4, "HasVertices incorrect");
        }

        [TestMethod()]
        public void Test_FenceTests_NumVertices()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.IsTrue(fence.NumVertices == 4, "Incorrect number of vertices");
        }

        [TestMethod()]
        public void Test_FenceTests_Area()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.IsTrue(fence.Area() == 10000, "Fence area is incorrect: {0}", fence.Area());
        }

        [TestMethod()]
        public void Test_FenceTests_UpdateExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            fence.UpdateExtents();

            Assert.IsTrue(fence.MinX == 0 && fence.MinY == 0 && fence.MaxX == 100 && fence.MaxY == 100,
                "Extents are incorrect");
        }

        [TestMethod()]
        public void Test_FenceTests_Assign()
        {
            Fence fenceSource = makeSimpleRectangleFence();
            Fence fence = new Fence();

            fence.Assign(fenceSource);

            Assert.IsTrue(fence.NumVertices == 4, "Incorrect number of vertices");

            Assert.IsTrue(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");
        }

        [TestMethod()]
        public void Test_FenceTests_SetRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.IsTrue(fence.NumVertices == 4, "Incorrect number of vertices");

            Assert.IsTrue(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.IsTrue(fence.IsRectangle == true, "Not a rectangle");
        }

        [TestMethod()]
        public void Test_FenceTests_IsNull()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.IsFalse(fence.IsNull(), "Fence is null when it is not");

            fence[0].X = Consts.NullDouble;

            Assert.IsTrue(fence.IsNull(), "Fence is not null when it is");
        }
    }
}
