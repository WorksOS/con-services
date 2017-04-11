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
    public class XYZTests
    {
        [TestMethod()]
        public void Test_XYZTests_Creation()
        {
            XYZ p = new XYZ(1, 2, 3);

            Assert.IsTrue(p.X == 1 && p.Y == 2 & p.Z == 3, "XYZ did not create as expected");
        }

        [TestMethod()]
        public void Test_XYZTests_Explode()
        {
            // Test exploding of args into variables
            XYZ xyz = new XYZ(1, 2, 3);

            double X, Y, Z;
            xyz.Explode(out X, out Y, out Z);

            Assert.IsTrue(X == 1 && Y == 2 && Z == 3, "Explode did not return X, Y & Z correctly");
        }

        [TestMethod()]
        public void Test_XYZTests_ToString()
        {
            XYZ p = new XYZ(1, 2, 3);

            Assert.IsTrue(p.ToString() == "X:1.000, Y:2.000, Z:3.000", "ToString is not correct: {0} [should be 'X:1.000, Y:2.000, Z:3.000']", p.ToString());
        }

        [TestMethod()]
        public void Test_XYZTests_Null()
        {
            XYZ p = XYZ.Null;

            Assert.IsTrue(p.X == Consts.NullDouble && p.Y == Consts.NullDouble & p.Z == Consts.NullDouble, "XYZ did not create as expected");
        }

        [TestMethod()]
        public void Test_XYZTests_IsNull()
        {
            XYZ p = XYZ.Null;

            Assert.IsTrue(p.IsNull, "Null XYZ does not advertise as being null");

            Assert.IsFalse(p.IsNull && (p.X != Consts.NullDouble || p.Y != Consts.NullDouble || p.Z != Consts.NullDouble), 
                          "IsNull is true with non-null components");
        }

        [TestMethod()]
        public void Test_XYZTests_IsNullInPlan()
        {
            XYZ p = XYZ.Null;

            Assert.IsTrue(p.IsNullInPlan, "Null XYZ does not advertise as being null in plan");

            Assert.IsFalse(p.IsNullInPlan && (p.X != Consts.NullDouble || p.Y != Consts.NullDouble),
                          "IsNullInPlan is true with non-null plan components");
        }

        [TestMethod()]
        public void Test_XYZTests_NextSide()
        {
            Assert.IsTrue(XYZ.NextSide(1) == 2 && XYZ.NextSide(2) == 3 && XYZ.NextSide(3) == 1, "Incorrect next side advancement");
        }

        [TestMethod()]
        public void Test_XYZTests_PrevSide()
        {
            Assert.IsTrue(XYZ.PrevSide(1) == 3 && XYZ.PrevSide(2) == 1 && XYZ.PrevSide(3) == 2, "Incorrect prev side advancement");
        }

        [TestMethod()]
        public void Test_XYZTests_Equals_NotEquals()
        {
            XYZ a = new XYZ(1, 2, 3);
            XYZ b = new XYZ(1, 2, 3);
            XYZ c = new XYZ(1, 3, 2);

            Assert.IsTrue(a.Equals(b), "Equality failure");
            Assert.IsFalse(a.Equals(c), "Inequality failure");

            Assert.IsTrue(a == b, "Equality (==) failure");
            Assert.IsTrue(a != c, "Inequality (!=) failure");
        }

        [TestMethod()]
        public void Test_XYZTests_Get2DLength()
        {
            XYZ a = new XYZ(1, 0, 0);
            XYZ b = new XYZ(11, 0, 0);

            Assert.IsTrue(XYZ.Get2DLength(a, b) == 10.0, "2DLength expected 10.0. got {0}", XYZ.Get2DLength(a, b));
        }

        [TestMethod()]
        public void Test_XYZTests_Get3DLength()
        {
            XYZ a = new XYZ(0, 0, 0);
            XYZ b = new XYZ(1, 1, 1);

            Assert.IsTrue(XYZ.Get3DLength(a, b) == Math.Sqrt(3), "3DLength expected {0}, got {1}", Math.Sqrt(3), XYZ.Get3DLength(a, b));
        }

        [TestMethod()]
        public void Test_XYZTests_DotProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(20, 30, 0);
            XYZ right = new XYZ(45, 70, 0);

            Assert.IsTrue(XYZ.DotProduct(bottom, top, right) == 3000, "Dot product expected {0}, got {1}", 3000, XYZ.DotProduct(bottom, top, right));
        }

        [TestMethod()]
        public void Test_XYZTests_PerpDotProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(20, 30, 0);
            XYZ right = new XYZ(45, 70, 0);

            Assert.IsTrue(XYZ.PerpDotProduct(bottom, top, right) == -50, "Dot product expected {0}, got {1}", -50, XYZ.PerpDotProduct(bottom, top, right));
        }

        [TestMethod()]
        public void Test_XYZTests_PointOnRight()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(5, 5, 0);
            XYZ left = new XYZ(-5, 5, 0);

            Assert.IsTrue(XYZ.PointOnRight(bottom, top, right), "Point right is not detected as being on the right of the line");
            Assert.IsFalse(XYZ.PointOnRight(bottom, top, left), "Point left is incorrrectly detected as being on the right of the line");
        }

        [TestMethod()]
        public void Test_XYZTests_PointOnOrOnRight()
        {
            // Note
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ rightOrOn = new XYZ(0, 5, 0);

            Assert.IsTrue(XYZ.PointOnOrOnRight(bottom, top, rightOrOn), "Point right (or on) is not detected as being on the right of (or on) the line");
        }

        [TestMethod()]
        public void Test_XYZTests_GetPointOffset()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ left = new XYZ(-5, 5, 0);
            XYZ right1 = new XYZ(0, 5, 0);
            XYZ right2 = new XYZ(5, 5, 0);

            Assert.IsTrue(XYZ.GetPointOffset(bottom, top, right1) == 0.0, "Get offset expected 0.0, got {0}", XYZ.GetPointOffset(bottom, top, right1));
            Assert.IsTrue(XYZ.GetPointOffset(bottom, top, right2) == 5.0, "Get offset expected 5.0, got {0}", XYZ.GetPointOffset(bottom, top, right2));
            Assert.IsTrue(XYZ.GetPointOffset(bottom, top, left) == -5.0, "Get offset expected -5.0, got {0}", XYZ.GetPointOffset(bottom, top, left));
        }

        [TestMethod()]
        public void Test_XYZTests_CrossProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 0, 0);

            XYZ correct1 = new XYZ(0, 0, -100);
            XYZ correct2 = new XYZ(0, 0, 100);

            Assert.IsTrue(XYZ.CrossProduct(bottom, top, right) == correct1, "Cross product expected {0}, got {1}", correct1, XYZ.CrossProduct(bottom, top, right));
            Assert.IsTrue(XYZ.CrossProduct(bottom, right, top) == correct2, "Cross product expected {0}, got {1}", correct2, XYZ.CrossProduct(bottom, right, top));
        }

        [TestMethod()]
        public void Test_XYZTests_VectorLength()
        {
            XYZ a = new XYZ(1, 1, 1);
            Assert.IsTrue(XYZ.VectorLength(a) == Math.Sqrt(3), "VectorLength expected {0}, got {1}", Math.Sqrt(3), XYZ.VectorLength(a));
        }

        [TestMethod()]
        public void Test_XYZTests_GetTriArea()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.IsTrue(XYZ.GetTriArea(bottom, top, right) == 50.0, "Expected triangle area = 50.0, got {0}", XYZ.GetTriArea(bottom, top, right));
        }

        [TestMethod()]
        public void Test_XYZTests_GetTriangleHeight()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.IsTrue(XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0) == 0.0, "Expected triangle area = 0.0, got {0}", XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0));

            bottom.Z = 10;
            top.Z = 10;
            right.Z = 10;

            Assert.IsTrue(XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0) == 10.0, "Expected triangle area = 10.0, got {0}", XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0));
        }

        [TestMethod()]
        public void Test_XYZTests_PointInTriangle()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.IsTrue(XYZ.PointInTriangle(bottom, top, right, 1.0, 9.0), "Expected point in triangle == true, got {0}", XYZ.PointInTriangle(bottom, top, right, 1.0, 9.0));
            Assert.IsFalse(XYZ.PointInTriangle(bottom, top, right, 10, 11), "Expected point in triangle == false, got {0}", XYZ.PointInTriangle(bottom, top, right, 10.0, 11.0));
        }

        [TestMethod()]
        public void Test_XYZTests_PointInTriangleInclusive()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.IsTrue(XYZ.PointInTriangleInclusive(bottom, top, right, 1.0, 9.0), "0, Expected point in triangle inclusive == true, got {0}", XYZ.PointInTriangleInclusive(bottom, top, right, 1.0, 9.0));
            Assert.IsTrue(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 0.0), "1, Expected point in triangle inclusive == true, got {0}", XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 0.0));
            Assert.IsTrue(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 10.0), "2, Expected point in triangle inclusive == true, got {0}", XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 10.0));
            Assert.IsTrue(XYZ.PointInTriangleInclusive(bottom, top, right, 10.0, 10.0), "3, Expected point in triangle inclusive == true, got {0}", XYZ.PointInTriangleInclusive(bottom, top, right, 10.0, 10.0));
            Assert.IsTrue(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 5.0), "4, Expected point in triangle inclusive == true, got {0}", XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 5.0));
        }

        [TestMethod()]
        public void Test_XYZTests_UnitVector()
        {
            // Test simple vector
            XYZ a = new XYZ(1, 1, 1);
            Assert.IsTrue(XYZ.VectorLength(XYZ.UnitVector(a)) == 1.0, "UnitVector expected {0}, got {1}", 1.0, XYZ.VectorLength(XYZ.UnitVector(a)));

            // Text two similar vectors produce the same result
            XYZ b = new XYZ(10, 10, 10);
            XYZ av = XYZ.UnitVector(a);
            XYZ bv = XYZ.UnitVector(b);
            XYZ diff = av - bv;
            Assert.IsTrue(XYZ.VectorLength(diff) < 0.00001, "VectorLength for two similar vectors are different: {0} vs {1}", av, bv);
        }
    }
}