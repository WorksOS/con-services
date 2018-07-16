using System;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests
{
        public class UTMCoordPointPairTests
    {
        [Fact]
        public void Test_UTMCoordPointPair_Creation_Specific()
        {
            UTMCoordPointPair ptPair = new UTMCoordPointPair(new XYZ(1, 2, 3), new XYZ(4, 5, 6), 7);

            Assert.True(ptPair.Left.X == 1 && ptPair.Left.Y == 2 && ptPair.Left.Z == 3 &&
                          ptPair.Right.X == 4 && ptPair.Right.Y == 5 && ptPair.Right.Z == 6 &&
                          ptPair.UTMZone == 7,
                          "Point pair not constructed as expected");
        }

        [Fact]
        public void Test_UTMCoordPointPair_Creation_Null()
        {
            UTMCoordPointPair ptPair = UTMCoordPointPair.Null;

            Assert.True(ptPair.Left.X == Consts.NullDouble && ptPair.Left.Y == Consts.NullDouble && ptPair.Left.Z == Consts.NullDouble &&
                          ptPair.Right.X == Consts.NullDouble && ptPair.Right.Y == Consts.NullDouble && ptPair.Right.Z == Consts.NullDouble &&
                          ptPair.UTMZone == byte.MaxValue,
                          "Point pair not constructed as expected");
        }
    }
}

