using System;
using VSS.TRex.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
        public class BitFlagHelperTests
    {
        [Fact]
        public void Test_BitFlagHelperTests_BitOn()
        {
            byte value = 0;
            value =  BitFlagHelper.BitOn(value, 0);

            Assert.Equal(1, value);
        }

        [Fact]
        public void Test_BitFlagHelperTests_BitOff()
        {
            byte value = 1;
            value = BitFlagHelper.BitOff(value, 0);

            Assert.Equal(0, value);
        }

        [Fact]
        public void Test_BitFlagHelperTests_IsBitOff()
        {
            byte value = 0;

            Assert.True(BitFlagHelper.IsBitOff(value, 0), "IsBitOff() for 0 bit returned true (it should be false)");

            byte value2 = 1;

            Assert.False(BitFlagHelper.IsBitOff(value2, 0), "IsBitOff() for 0 bit returned true (it should be true)");
        }

        [Fact]
        public void Test_BitFlagHelperTests_IsBitOn()
        {
            byte value = 1;

            Assert.True(BitFlagHelper.IsBitOn(value, 0), "IsBitOn() for 0 bit returned false (it should be true)");

            byte value2 = 0;

            Assert.False(BitFlagHelper.IsBitOn(value2, 0), "IsBitOn() for 0 bit returned true (it should be false)");
        }

        [Fact]
        public void Test_BitFlagHelperTests_SetBit()
        {
            byte value = 0;

            BitFlagHelper.SetBit(ref value, 0, false);
            Assert.True(BitFlagHelper.IsBitOff(value, 0), "IsBitOff() for 0 bit returned false (it should be true)");

            BitFlagHelper.SetBit(ref value, 0, true);
            Assert.True(BitFlagHelper.IsBitOn(value, 0), "IsBitOn() for 0 bit returned false (it should be true)");
        }
    }
}
