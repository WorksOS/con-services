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
    public class LineIntersectionTests
    {
        [TestMethod()]
        public void Test_LinesIntersectTest_Colinear()
        {
            double positionX, positionY;
            bool linesAreColinear;

            // Check colinear lines fail with the coliner flag set
            Assert.IsFalse(LineIntersection.LinesIntersect(0.0, 0.0, 10.0, 0.0,
                           0.0, 10.0, 10.0, 10.0, out positionX, out positionY, false, out linesAreColinear),
                           "LinesIntersect call returned true");

            Assert.IsTrue(linesAreColinear, "Lines are not colinear as expected.");
        }

        [TestMethod()]
        public void Test_LinesIntersectTest_Intersecting()
        {
            double positionX, positionY;
            bool linesAreColinear;

            // Check lines intersection at origin do intersect there
            Assert.IsTrue(LineIntersection.LinesIntersect(-10.0, -10.0, 10.0, 10.0,
                          -10.0, 10.0, 10.0, -10.0, out positionX, out positionY, false, out linesAreColinear),
                          "LinesIntersect call returned false");
            Assert.IsFalse(linesAreColinear, "Lines are unexpectedly colinear");
            Assert.IsTrue(positionX == 0.0 && positionY == 0.0, "Intersection location incorrect = ({0}, {1})", positionX, positionY);
        }

    }
}