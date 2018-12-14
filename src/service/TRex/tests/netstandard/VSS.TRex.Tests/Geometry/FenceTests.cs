using VSS.TRex.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.TRex.Common;
using Xunit;

namespace VSS.TRex.Tests.Geometry
{
        public class FenceTests
    {
        private Fence makeSimpleRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            return fence;
        }

        [Fact()]
        public void Test_FenceTests_Fence()
        {
            Fence fence = new Fence();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");

            Fence fence2 = new Fence(0, 0, 1, 1);
            Assert.True(fence2.NumVertices == 4 && fence2.Area() == 1.0);
        }

        [Fact()]
        public void Test_FenceTests_IncludesPoint()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IncludesPoint(50, 50), "Point (50, 50) not included");
            Assert.False(fence.IncludesPoint(150, 150), "Point (150, 150) is included");
            Assert.False(fence.IncludesPoint(-1, 0), "Point (-1, 0) is included");
        }

        [Fact()]
        public void Test_FenceTests_IncludesLine()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IncludesLine(50, 50, 75, 75), "Line (50, 50, 75, 75) not included");
            Assert.True(fence.IncludesLine(50, 50, 175, 75), "Line (50, 50, 175, 75) not included");
            Assert.True(fence.IncludesLine(-100, -100, 200, 200), "Line (-100, -100, 200, 200) not included");
            Assert.False(fence.IncludesLine(-100, -100, 0, 200), "Line (-100, -100, 0, 200) included");
        }

        [Fact()]
        public void Test_FenceTests_IntersectsExtent()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX, fence.MinY, fence.MaxX, fence.MaxY)), 
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 50, fence.MinY - 50, fence.MaxX - 50, fence.MaxY - 50)),
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 100, fence.MinY - 100, fence.MaxX - 100, fence.MaxY - 100)),
                          "Extents do not overlap");
            Assert.True(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 100, fence.MinY + 100, fence.MaxX + 100, fence.MaxY + 100)),
                          "Extents do not overlap");

            Assert.False(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX - 101, fence.MinY - 101, fence.MaxX - 101, fence.MaxY - 101)),
                           "Extents not overlap");
            Assert.False(fence.IntersectsExtent(new BoundingWorldExtent3D(fence.MinX + 101, fence.MinY + 101, fence.MaxX + 101, fence.MaxY + 101)),
                           "Extents not overlap");
        }

        [Fact()]
        public void Test_FenceTests_Initialise()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Initialise();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");
        }

        [Fact()]
        public void Test_FenceTests_Clear()
        {
            Fence fence = makeSimpleRectangleFence();
            fence.Clear();

            Assert.False(fence.IsRectangle, "IsRectangle not initialised to false");
            Assert.False(fence.HasVertices, "New fence has vertices");
            Assert.True(fence.MinX > fence.MaxX && fence.MinY > fence.MaxY,
                "Fence extents are not reveresed");        
        }

        [Fact()]
        public void Test_FenceTests_IsSquare()
        {
            Fence fence = makeSimpleRectangleFence();
            Assert.True(fence.IsSquare, "Rectangle is not square");

            fence.Points[0].SetXY(-100, 100); // Not square any more
            fence.UpdateExtents();

            Assert.False(fence.IsSquare, "Rectangle is square");
        }

        [Fact()]
        public void Test_FenceTests_GetExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            fence.GetExtents(out double minx, out double miny, out double maxx, out double maxy);

            Assert.True(minx == 0 && miny == 0 && maxx == 100 && maxy == 100,
                "Extracted extents are incorrect");
        }

        [Fact()]
        public void Test_FenceTests_SetExtents()
        {
            Fence fence = new Fence();
            fence.SetExtents(0, 0, 100, 100);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.True(fence.IsRectangle);
        }

        [Fact()]
        public void Test_FenceTests_HasVertices()
        {
            Fence fence = new Fence();

            Assert.False(fence.HasVertices, "HasVertices incorrect");

            fence.SetExtents(0, 0, 100, 100);

            Assert.True(fence.HasVertices && fence.NumVertices == 4, "HasVertices incorrect");
        }

        [Fact()]
        public void Test_FenceTests_NumVertices()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.Equal(4, fence.NumVertices);
        }

        [Fact()]
        public void Test_FenceTests_Area()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.Equal(10000, fence.Area());
        }

        [Fact()]
        public void Test_FenceTests_UpdateExtents()
        {
            Fence fence = makeSimpleRectangleFence();

            Assert.True(fence.MinX == 0 && fence.MinY == 0 && fence.MaxX == 100 && fence.MaxY == 100,
                "Extents are incorrect");
        }

        [Fact()]
        public void Test_FenceTests_Assign()
        {
            Fence fenceSource = makeSimpleRectangleFence();
            Fence fence = new Fence();

            fence.Assign(fenceSource);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");
        }

        [Fact()]
        public void Test_FenceTests_SetRectangleFence()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.Equal(4, fence.NumVertices);

            Assert.True(fence.Points[0].X == 0 && fence.Points[0].Y == 0 &&
                fence.Points[1].X == 0 && fence.Points[1].Y == 100 &&
                fence.Points[2].X == 100 && fence.Points[2].Y == 100 &&
                fence.Points[3].X == 100 && fence.Points[3].Y == 0,
                "Fence point vertices incorrect");

            Assert.True(fence.IsRectangle);
        }

        [Fact()]
        public void Test_FenceTests_IsNull()
        {
            Fence fence = new Fence();
            fence.SetRectangleFence(0, 0, 100, 100);

            Assert.False(fence.IsNull(), "Fence is null when it is not");

            fence[0].X = Consts.NullDouble;

            Assert.True(fence.IsNull(), "Fence is not null when it is");
        }
    }
}
