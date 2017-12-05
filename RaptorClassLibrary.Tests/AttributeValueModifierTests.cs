using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.Compression.Tests
{
    [TestClass]
    public class AttributeValueModifierTests
    {
        [TestMethod]
        public void Test_AttributeValueModifier_Height()
        {
            Assert.IsTrue(AttributeValueModifiers.ModifiedHeight(Consts.NullHeight) == int.MaxValue, "Null height not represented by int.MaxValue as expected");
            Assert.IsTrue(AttributeValueModifiers.ModifiedHeight(12345.678F) == 12345678, "Height of 12345.678 not modified to 12345678mm as expected");
        }

        [TestMethod]
        public void Test_AttributeValueModifier_Time()
        {
            DateTime origin = new DateTime(2000, 1, 1, 1, 1, 1);

            Assert.IsTrue(AttributeValueModifiers.ModifiedTime(new DateTime(2000, 1, 1, 1, 1, 1), origin) == 1, "Time with 0 offset from origin not represented by 1 as expected");
            Assert.IsTrue(AttributeValueModifiers.ModifiedTime(new DateTime(2000, 1, 1, 1, 1, 11), origin) == 11, "Time with 10 seconds offset from origin not represented by 11 as expected");
        }

        [TestMethod]
        public void Test_AttributeValueModifier_GPSMode()
        {
            Assert.IsTrue(AttributeValueModifiers.ModifiedGPSMode(GPSMode.Old) == 0, "GPSMode.Old not zero as expected");
            Assert.IsTrue(AttributeValueModifiers.ModifiedGPSMode(GPSMode.NoGPS) == 15, "GPSMode.NoGPS not 15 as expected");
            Assert.IsTrue(AttributeValueModifiers.ModifiedGPSMode((GPSMode)16) == 0, "(GPSMode)16 not zero as expected");
        }
    }
}
