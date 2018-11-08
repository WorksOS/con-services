using System;
using VSS.TRex.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
        public class MinMaxTests
    {
        [Fact]
        public void Test_Swap()
        {
            int a = 10;
            int b = 20;

            TRex.Utilities.MinMax.Swap(ref a, ref b);
            Assert.True(a == 20 && b == 10, "Swap failed to swap items");

            TRex.Utilities.MinMax.Swap<int>(ref a, ref b);
            Assert.True(a == 10 && b == 20, "Swap failed to swap items");
        }

        [Fact]
        public void Test_SetMinMax()
        {
            double a = 10;
            double b = 20;

            TRex.Utilities.MinMax.SetMinMax(ref a, ref b);
            Assert.True(a == 10 && b == 20, "SetMinMax swapped values when it should not");

            double c = 20;
            double d = 10;

            TRex.Utilities.MinMax.SetMinMax(ref c, ref d);
            Assert.True(c == 10 && d == 20, "SetMinMax did not swap values when it should");
        }

        [Fact]
        public void Test_SetMinMax_In_T()
        {
            int a = 10;
            int b = 20;

            TRex.Utilities.MinMax.SetMinMax<int>(ref a, ref b);
            Assert.True(a == 10 && b == 20, "SetMinMax swapped values when it should not");

            int c = 20;
            int d = 10;

            TRex.Utilities.MinMax.SetMinMax<int>(ref c, ref d);
            Assert.True(c == 10 && d == 20, "SetMinMax did not swap values when it should");
        }
    }
}
