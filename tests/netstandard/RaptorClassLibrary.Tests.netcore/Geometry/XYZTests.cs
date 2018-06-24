using VSS.TRex.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.TRex.Common;
using Xunit;

namespace VSS.TRex.Geometry.Tests
{
        public class XYZTests
    {
        [Fact()]
        public void Test_XYZTests_Creation()
        {
            XYZ p = new XYZ(1, 2, 3);

            Assert.True(p.X == 1 && p.Y == 2 & p.Z == 3, "XYZ did not create as expected");
        }

        [Fact()]
        public void Test_XYZTests_Explode()
        {
            // Test exploding of args into variables
            XYZ xyz = new XYZ(1, 2, 3);

            double X, Y, Z;
            xyz.Explode(out X, out Y, out Z);

            Assert.True(X == 1 && Y == 2 && Z == 3, "Explode did not return X, Y & Z correctly");
        }

        [Fact()]
        public void Test_XYZTests_ToString()
        {
            XYZ p = new XYZ(1, 2, 3);

            Assert.Equal("X:1.000, Y:2.000, Z:3.000", p.ToString());
        }

        [Fact()]
        public void Test_XYZTests_Null()
        {
            XYZ p = XYZ.Null;

            Assert.True(p.X == Consts.NullDouble && p.Y == Consts.NullDouble & p.Z == Consts.NullDouble, "XYZ did not create as expected");
        }

        [Fact()]
        public void Test_XYZTests_IsNull()
        {
            XYZ p = XYZ.Null;

            Assert.True(p.IsNull, "Null XYZ does not advertise as being null");

            Assert.False(p.IsNull && (p.X != Consts.NullDouble || p.Y != Consts.NullDouble || p.Z != Consts.NullDouble), 
                          "IsNull is true with non-null components");
        }

        [Fact()]
        public void Test_XYZTests_IsNullInPlan()
        {
            XYZ p = XYZ.Null;

            Assert.True(p.IsNullInPlan, "Null XYZ does not advertise as being null in plan");

            Assert.False(p.IsNullInPlan && (p.X != Consts.NullDouble || p.Y != Consts.NullDouble),
                          "IsNullInPlan is true with non-null plan components");
        }

        [Fact()]
        public void Test_XYZTests_NextSide()
        {
            Assert.True(XYZ.NextSide(1) == 2 && XYZ.NextSide(2) == 3 && XYZ.NextSide(3) == 1, "Incorrect next side advancement");
        }

        [Fact()]
        public void Test_XYZTests_PrevSide()
        {
            Assert.True(XYZ.PrevSide(1) == 3 && XYZ.PrevSide(2) == 1 && XYZ.PrevSide(3) == 2, "Incorrect prev side advancement");
        }

        [Fact()]
        public void Test_XYZTests_Equals_NotEquals()
        {
            XYZ a = new XYZ(1, 2, 3);
            XYZ b = new XYZ(1, 2, 3);
            XYZ c = new XYZ(1, 3, 2);

            Assert.True(a.Equals(b), "Equality failure");
            Assert.False(a.Equals(c), "Inequality failure");

            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }

        [Fact()]
        public void Test_XYZTests_Get2DLength()
        {
            XYZ a = new XYZ(1, 0, 0);
            XYZ b = new XYZ(11, 0, 0);

            Assert.Equal(10.0, XYZ.Get2DLength(a, b));
        }

        [Fact()]
        public void Test_XYZTests_Get3DLength()
        {
            XYZ a = new XYZ(0, 0, 0);
            XYZ b = new XYZ(1, 1, 1);

            Assert.True(Math.Abs(XYZ.Get3DLength(a, b) - Math.Sqrt(3)) < 0.00001, "3D Length not as expected");

            a = new XYZ(1, 2, 3);
            b = new XYZ(3, 4, 5);

            Assert.True(Math.Abs(XYZ.Get3DLength(a, b) - Math.Sqrt(12)) < 0.00001, "3D Length not as expected");
        }

        [Fact()]
        public void Test_XYZTests_DotProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(20, 30, 0);
            XYZ right = new XYZ(45, 70, 0);

            Assert.Equal(3000, XYZ.DotProduct(bottom, top, right));
        }

        [Fact()]
        public void Test_XYZTests_PerpDotProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(20, 30, 0);
            XYZ right = new XYZ(45, 70, 0);

            Assert.Equal(-50, XYZ.PerpDotProduct(bottom, top, right));
        }

        [Fact()]
        public void Test_XYZTests_PointOnRight()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(5, 5, 0);
            XYZ left = new XYZ(-5, 5, 0);

            Assert.True(XYZ.PointOnRight(bottom, top, right), "Point right is not detected as being on the right of the line");
            Assert.False(XYZ.PointOnRight(bottom, top, left), "Point left is incorrrectly detected as being on the right of the line");
        }

        [Fact()]
        public void Test_XYZTests_PointOnOrOnRight()
        {
            // Note
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ rightOrOn = new XYZ(0, 5, 0);

            Assert.True(XYZ.PointOnOrOnRight(bottom, top, rightOrOn), "Point right (or on) is not detected as being on the right of (or on) the line");
        }

        [Fact()]
        public void Test_XYZTests_GetPointOffset()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ left = new XYZ(-5, 5, 0);
            XYZ right1 = new XYZ(0, 5, 0);
            XYZ right2 = new XYZ(5, 5, 0);

            Assert.Equal(0.0, XYZ.GetPointOffset(bottom, top, right1));
            Assert.Equal(5.0, XYZ.GetPointOffset(bottom, top, right2));
            Assert.Equal(XYZ.GetPointOffset(bottom, top, left), -5.0);
        }

        [Fact()]
        public void Test_XYZTests_CrossProduct()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 0, 0);

            XYZ correct1 = new XYZ(0, 0, -100);
            XYZ correct2 = new XYZ(0, 0, 100);

            Assert.Equal(XYZ.CrossProduct(bottom, top, right), correct1);
            Assert.Equal(XYZ.CrossProduct(bottom, right, top), correct2);
        }

        [Fact()]
        public void Test_XYZTests_VectorLength()
        {
            XYZ a = new XYZ(1, 1, 1);
            Assert.Equal(XYZ.VectorLength(a), Math.Sqrt(3));
        }

        [Fact()]
        public void Test_XYZTests_GetTriArea()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.Equal(50.0, XYZ.GetTriArea(bottom, top, right));
        }

        [Fact()]
        public void Test_XYZTests_GetTriangleHeight()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.Equal(0.0, XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0));

            bottom.Z = 10;
            top.Z = 10;
            right.Z = 10;

            Assert.Equal(10.0, XYZ.GetTriangleHeight(bottom, top, right, 1.0, 9.0));
        }

        [Fact()]
        public void Test_XYZTests_PointInTriangle()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.True(XYZ.PointInTriangle(bottom, top, right, 1.0, 9.0), $"Expected point in triangle == true, got {XYZ.PointInTriangle(bottom, top, right, 1.0, 9.0)}" );
            Assert.False(XYZ.PointInTriangle(bottom, top, right, 10, 11), $"Expected point in triangle == false, got {XYZ.PointInTriangle(bottom, top, right, 10.0, 11.0)}" );
        }

        [Fact()]
        public void Test_XYZTests_PointInTriangleInclusive()
        {
            XYZ bottom = new XYZ(0, 0, 0);
            XYZ top = new XYZ(0, 10, 0);
            XYZ right = new XYZ(10, 10, 0);

            Assert.True(XYZ.PointInTriangleInclusive(bottom, top, right, 1.0, 9.0), $"0, Expected point in triangle inclusive == true, got {XYZ.PointInTriangleInclusive(bottom, top, right, 1.0, 9.0)}" );
            Assert.True(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 0.0), $"1, Expected point in triangle inclusive == true, got {XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 0.0)}" );
            Assert.True(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 10.0), $"2, Expected point in triangle inclusive == true, got {XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 10.0)}" );
            Assert.True(XYZ.PointInTriangleInclusive(bottom, top, right, 10.0, 10.0), $"3, Expected point in triangle inclusive == true, got {XYZ.PointInTriangleInclusive(bottom, top, right, 10.0, 10.0)}" );
            Assert.True(XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 5.0), $"4, Expected point in triangle inclusive == true, got { XYZ.PointInTriangleInclusive(bottom, top, right, 0.0, 5.0)}");
        }

        [Fact()]
        public void Test_XYZTests_UnitVector()
        {
            // Test simple vector
            XYZ a = new XYZ(1, 1, 1);
            Assert.Equal(1.0, XYZ.VectorLength(XYZ.UnitVector(a)));

            // Text two similar vectors produce the same result
            XYZ b = new XYZ(10, 10, 10);
            XYZ av = XYZ.UnitVector(a);
            XYZ bv = XYZ.UnitVector(b);
            XYZ diff = av - bv;
            Assert.True(XYZ.VectorLength(diff) < 0.00001, $"VectorLength for two similar vectors are different: {av} vs {bv}");
        }
    }
}
