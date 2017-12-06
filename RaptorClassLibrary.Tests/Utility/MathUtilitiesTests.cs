using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Utilities.Tests
{
    [TestClass]
    public class MathUtilitiesTests
    {
        [TestMethod]
        public void Test_MathUtilities_Hypot()
        {
            Assert.IsTrue(MathUtilities.Hypot(3, 4) == 5, "3, 4, 5 right angled triangle did not compute correct hypotenuse");
            Assert.IsTrue(MathUtilities.Hypot(1, 1) == Math.Sqrt(2), "1, 1, root(2) right angled triangle did not compute correct hypotenuse");
        }
    }
}
