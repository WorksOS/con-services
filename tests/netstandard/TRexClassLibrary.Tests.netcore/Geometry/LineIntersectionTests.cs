using VSS.TRex.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.TRex.Geometry.Tests
{
        public class LineIntersectionTests
    {
        [Fact()]
        public void Test_LinesIntersectTest_Colinear()
        {
            double positionX, positionY;
            bool linesAreColinear;

            // Check colinear lines fail with the coliner flag set
            Assert.False(LineIntersection.LinesIntersect(0.0, 0.0, 10.0, 0.0,
                           0.0, 10.0, 10.0, 10.0, out positionX, out positionY, false, out linesAreColinear),
                           "LinesIntersect call returned true");

            Assert.True(linesAreColinear, "Lines are not colinear as expected.");
        }

        [Fact()]
        public void Test_LinesIntersectTest_Intersecting()
        {
            double positionX, positionY;
            bool linesAreColinear;

            // Check lines intersection at origin do intersect there
            Assert.True(LineIntersection.LinesIntersect(-10.0, -10.0, 10.0, 10.0,
                          -10.0, 10.0, 10.0, -10.0, out positionX, out positionY, false, out linesAreColinear),
                          "LinesIntersect call returned false");
            Assert.False(linesAreColinear, "Lines are unexpectedly colinear");
            Assert.True(positionX == 0.0 && positionY == 0.0, $"Intersection location incorrect = ({positionX}, {positionY})");
        }

    }
}