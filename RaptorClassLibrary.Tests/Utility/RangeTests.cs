using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class Range_EnsureRangeTests
    {
        [TestMethod]
        public void Test_Range_EnsureRange_byte()
        {
            byte value = 100;
            byte lowerConstraint = 10;
            byte upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, byte.MinValue, byte.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, byte.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, byte.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_short()
        {
            short value = 100;
            short lowerConstraint = 10;
            short upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, short.MinValue, short.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, short.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, short.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_ushort()
        {
            ushort value = 100;
            ushort lowerConstraint = 10;
            ushort upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, ushort.MinValue, ushort.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, ushort.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, ushort.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_int()
        {
            int value = 100;
            int lowerConstraint = 10;
            int upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, int.MinValue, int.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, int.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, int.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_uint()
        {
            uint value = 100;
            uint lowerConstraint = 10;
            uint upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, uint.MinValue, uint.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, uint.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, uint.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_long()
        {
            long value = 100;
            long lowerConstraint = 10;
            long upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, long.MinValue, long.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, long.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, long.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_ulong()
        {
            ulong value = 100;
            ulong lowerConstraint = 10;
            ulong upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, ulong.MinValue, ulong.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, ulong.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, ulong.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_float()
        {
            float value = 100;
            float lowerConstraint = 10;
            float upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, float.MinValue, float.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, float.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, float.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_EnsureRange_double()
        {
            double value = 100;
            double lowerConstraint = 10;
            double upperConstraint = 200;

            // Test in range
            Assert.IsTrue(value == Range.EnsureRange(value, double.MinValue, double.MaxValue), "Range truncation failed on full underlying type value range");

            // Test out of range
            Assert.IsTrue(10 == Range.EnsureRange(value, double.MinValue, lowerConstraint), "Range truncation failed on partial underlying type value range");
            Assert.IsTrue(200 == Range.EnsureRange(value, upperConstraint, double.MaxValue), "Range truncation failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_byte()
        {
            byte value = 100;
            byte lowerConstraint = 10;
            byte upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, byte.MinValue, byte.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, byte.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, byte.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_short()
        {
            short value = 100;
            short lowerConstraint = 10;
            short upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, short.MinValue, short.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, short.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, short.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_ushort()
        {
            ushort value = 100;
            ushort lowerConstraint = 10;
            ushort upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, ushort.MinValue, ushort.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, ushort.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, ushort.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_int()
        {
            int value = 100;
            int lowerConstraint = 10;
            int upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, int.MinValue, int.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, int.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, int.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_uint()
        {
            uint value = 100;
            uint lowerConstraint = 10;
            uint upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, uint.MinValue, uint.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, uint.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, uint.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_long()
        {
            long value = 100;
            long lowerConstraint = 10;
            long upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, long.MinValue, long.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, long.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, long.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_ulong()
        {
            ulong value = 100;
            ulong lowerConstraint = 10;
            ulong upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, ulong.MinValue, ulong.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, ulong.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, ulong.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_float()
        {
            float value = 100;
            float lowerConstraint = 10;
            float upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, float.MinValue, float.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, float.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, float.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_double()
        {
            double value = 100;
            double lowerConstraint = 10;
            double upperConstraint = 200;

            // Test in range
            Assert.IsTrue(Range.InRange(value, double.MinValue, double.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, double.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, double.MaxValue), "InRange failed on partial underlying type value range");
        }

        [TestMethod]
        public void Test_Range_InRange_datetime()
        {
            DateTime value = new DateTime(2000, 1, 1);
            DateTime lowerConstraint = new DateTime(1990, 1, 1);
            DateTime upperConstraint = new DateTime(2010, 1, 1);

            // Test in range
            Assert.IsTrue(Range.InRange(value, DateTime.MinValue, DateTime.MaxValue), "InRange failed on full underlying type value range");

            // Test out of range
            Assert.IsFalse(Range.InRange(value, DateTime.MinValue, lowerConstraint), "InRange failed on partial underlying type value range");
            Assert.IsFalse(Range.InRange(value, upperConstraint, DateTime.MaxValue), "InRange failed on partial underlying type value range");
        }
    }
}
