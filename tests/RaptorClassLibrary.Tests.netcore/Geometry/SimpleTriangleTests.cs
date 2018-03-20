using VSS.VisionLink.Raptor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.VisionLink.Raptor.Geometry.Tests
{
        public class SimpleTriangleTests
    {
        [Fact()]
        public void Test_SimpleTriangleTests_Creation()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(1, 2, 3), new XYZ(4, 5, 6), new XYZ(7, 8, 9));

            Assert.True(tri.V1.X == 1 && tri.V1.Y == 2 && tri.V1.Z == 3 &&
                tri.V2.X == 4 && tri.V2.Y == 5 && tri.V2.Z == 6 &&
                tri.V3.X == 7 && tri.V3.Y == 8 && tri.V3.Z == 9,
                "Simple triangle not created as expected");
        }

        [Fact()]
        public void Test_SimpleTriangleTests_Area()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));

            Assert.Equal(5000, tri.Area);
        }

        [Fact()]
        public void Test_SimpleTriangleTests_IncludesPoint()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));

            Assert.True(tri.IncludesPoint(1, 1), "Point (1, 1) not included");
            Assert.False(tri.IncludesPoint(100, 100), "Point (1, 1) not included");
            Assert.False(tri.IncludesPoint(-1, -1), "Point (1, 1) not included");
        }

        [Fact()]
        public void Test_SimpleTriangleTests_InterpolateHeight()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));
            Assert.Equal(0, tri.InterpolateHeight(1, 1));

            SimpleTriangle tri2 = new SimpleTriangle(new XYZ(0, 0, 100), new XYZ(0, 100, 100), new XYZ(100, 0, 100));
            Assert.Equal(100, tri2.InterpolateHeight(1, 1));

            SimpleTriangle tri3 = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 100), new XYZ(100, 0, 0));
            Assert.Equal(50, tri3.InterpolateHeight(10, 50));
        }
    }
}