using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class MinMaxTests
    {
        [TestMethod]
        public void Test_Swap()
        {
            int a = 10;
            int b = 20;

            MinMax.Swap(ref a, ref b);
            Assert.IsTrue(a == 20 && b == 10, "Swap failed to swap items");

            MinMax.Swap<int>(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "Swap failed to swap items");
        }

        [TestMethod]
        public void Test_SetMinMax()
        {
            double a = 10;
            double b = 20;

            MinMax.SetMinMax(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax swapped values when it did not");

            double c = 20;
            double d = 10;

            MinMax.SetMinMax(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax did not swap values when it should");
        }

        [TestMethod]
        public void Test_SetMinMax_In_T()
        {
            int a = 10;
            int b = 20;

            MinMax.SetMinMax<int>(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax swapped values when it did not");

            int c = 20;
            int d = 10;

            MinMax.SetMinMax<int>(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax did not swap values when it should");
        }
    }
}
