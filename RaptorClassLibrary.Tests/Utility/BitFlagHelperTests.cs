using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class BitFlagHelperTests
    {
        [TestMethod]
        public void Test_BitFlagHelperTests_BitOn()
        {
            byte value = 0;
            value =  BitFlagHelper.BitOn(value, 0);

            Assert.IsTrue(value == 1, "BitOn() for 0 bit returned true");
        }

        [TestMethod]
        public void Test_BitFlagHelperTests_BitOff()
        {
            byte value = 1;
            value = BitFlagHelper.BitOff(value, 0);

            Assert.IsTrue(value == 0, "BitOff() for 0 bit returned true");
        }

        [TestMethod]
        public void Test_BitFlagHelperTests_IsBitOff()
        {
            byte value = 0;

            Assert.IsTrue(BitFlagHelper.IsBitOff(value, 0), "IsBitOff() for 0 bit returned true (it should be false)");

            byte value2 = 1;

            Assert.IsFalse(BitFlagHelper.IsBitOff(value2, 0), "IsBitOff() for 0 bit returned true (it should be true)");
        }

        [TestMethod]
        public void Test_BitFlagHelperTests_IsBitOn()
        {
            byte value = 1;

            Assert.IsTrue(BitFlagHelper.IsBitOn(value, 0), "IsBitOn() for 0 bit returned false (it should be true)");

            byte value2 = 0;

            Assert.IsFalse(BitFlagHelper.IsBitOn(value2, 0), "IsBitOn() for 0 bit returned true (it should be false)");
        }

        [TestMethod]
        public void Test_BitFlagHelperTests_SetBit()
        {
            byte value = 0;

            BitFlagHelper.SetBit(ref value, 0, false);
            Assert.IsTrue(BitFlagHelper.IsBitOff(value, 0), "IsBitOff() for 0 bit returned false (it should be true)");

            BitFlagHelper.SetBit(ref value, 0, true);
            Assert.IsTrue(BitFlagHelper.IsBitOn(value, 0), "IsBitOn() for 0 bit returned false (it should be true)");
        }
    }
}
