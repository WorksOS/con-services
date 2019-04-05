using System;
using FluentAssertions;
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
            var origin = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc);

            Assert.Equal(0, AttributeValueModifiers.ModifiedTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), origin));
            Assert.Equal(100, AttributeValueModifiers.ModifiedTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 11), DateTimeKind.Utc), origin));
        }

        [Fact]
        public void Test_AttributeValueModifier_Time_FailWithNegativeOffset()
        {
          var origin = DateTime.SpecifyKind(new DateTime(2000, 2, 1, 1, 1, 1), DateTimeKind.Utc);

          Action act = () => AttributeValueModifiers.ModifiedTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), origin);
          act.Should().Throw<ArgumentException>().WithMessage("Time argument [*] should not be less that the origin [*]");
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
