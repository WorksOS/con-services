using System;
using VSS.TRex.Common;
using VSS.TRex.Compression;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Compression
{
        public class AttributeValueModifierTests
    {
        [Fact]
        public void Test_AttributeValueModifier_Height()
        {
            Assert.Equal(AttributeValueModifiers.ModifiedHeight(Consts.NullHeight), int.MaxValue);
            Assert.Equal(12345678, AttributeValueModifiers.ModifiedHeight(12345.678F));
        }

        [Fact]
        public void Test_AttributeValueModifier_Time()
        {
            DateTime origin = new DateTime(2000, 1, 1, 1, 1, 1);

            Assert.Equal(1, AttributeValueModifiers.ModifiedTime(new DateTime(2000, 1, 1, 1, 1, 1), origin));
            Assert.Equal(11, AttributeValueModifiers.ModifiedTime(new DateTime(2000, 1, 1, 1, 1, 11), origin));
        }

        [Fact]
        public void Test_AttributeValueModifier_GPSMode()
        {
            Assert.Equal(0, AttributeValueModifiers.ModifiedGPSMode(GPSMode.Old));
            Assert.Equal(15, AttributeValueModifiers.ModifiedGPSMode(GPSMode.NoGPS));
            Assert.Equal(0, AttributeValueModifiers.ModifiedGPSMode((GPSMode)16));
        }
    }
}
