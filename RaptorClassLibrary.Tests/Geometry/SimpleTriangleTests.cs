using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Geometry.Tests
{
    [TestClass()]
    public class SimpleTriangleTests
    {
        [TestMethod()]
        public void Test_SimpleTriangleTests_Creation()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(1, 2, 3), new XYZ(4, 5, 6), new XYZ(7, 8, 9));

            Assert.IsTrue(tri.V1.X == 1 && tri.V1.Y == 2 && tri.V1.Z == 3 &&
                tri.V2.X == 4 && tri.V2.Y == 5 && tri.V2.Z == 6 &&
                tri.V3.X == 7 && tri.V3.Y == 8 && tri.V3.Z == 9,
                "Simple triangle not created as expected");
        }

        [TestMethod()]
        public void Test_SimpleTriangleTests_Area()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));

            Assert.IsTrue(tri.Area == 5000, "Area should be 50.0, instead it is {0}", tri.Area);
        }

        [TestMethod()]
        public void Test_SimpleTriangleTests_IncludesPoint()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));

            Assert.IsTrue(tri.IncludesPoint(1, 1), "Point (1, 1) not included");
            Assert.IsFalse(tri.IncludesPoint(100, 100), "Point (1, 1) not included");
            Assert.IsFalse(tri.IncludesPoint(-1, -1), "Point (1, 1) not included");
        }

        [TestMethod()]
        public void Test_SimpleTriangleTests_InterpolateHeight()
        {
            SimpleTriangle tri = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 0), new XYZ(100, 0, 0));
            Assert.IsTrue(tri.InterpolateHeight(1, 1) == 0, "Interpolated height should be zero");

            SimpleTriangle tri2 = new SimpleTriangle(new XYZ(0, 0, 100), new XYZ(0, 100, 100), new XYZ(100, 0, 100));
            Assert.IsTrue(tri2.InterpolateHeight(1, 1) == 100, "Interpolated height should be 100");

            SimpleTriangle tri3 = new SimpleTriangle(new XYZ(0, 0, 0), new XYZ(0, 100, 100), new XYZ(100, 0, 0));
            Assert.IsTrue(tri3.InterpolateHeight(10, 50) == 50, "Interpolated height should be 50");
        }
    }
}