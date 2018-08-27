using System.Linq;
using VSS.TRex.Compression;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Compression
{
        public class AttributeValueRangeCalculatorTests
    {
        [Fact]
        public void Test_AttributeValueRangeCalculator_NullRangeHeights()
        { 
            // Simulate a subgrid containing the same non-null elevation in every cell, expressed as a single vector
            int value = AttributeValueModifiers.ModifiedHeight(12345.678F);

            int[] values = Enumerable.Range(0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid).Select(x => value).ToArray();

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();

            AttributeValueRangeCalculator.CalculateAttributeValueRange(values, 0xffffffff, 0x7fffffff, true, ref descriptor);

            Assert.Equal(descriptor.MinValue, value);
            Assert.Equal(descriptor.MaxValue, value);
            Assert.Equal(0, descriptor.RequiredBits);
        }

        [Fact]
        public void Test_AttributeValueRangeCalculator_SingleBitRange_NonNullable()
        {
            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();

            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 1 }, 0xffffffff, 0, false, ref descriptor);

            Assert.Equal(0, descriptor.MinValue);
            Assert.Equal(1, descriptor.MaxValue);
            Assert.Equal(1, descriptor.RequiredBits);
        }

        [Fact]
        public void Test_AttributeValueRangeCalculator_SingleBitRange_Nullable()
        {
            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();

            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 1 }, 0xffffffff, 5, true, ref descriptor);

            Assert.Equal(0, descriptor.MinValue);
            Assert.Equal(1, descriptor.MaxValue);
            Assert.False(descriptor.Nullable);
            Assert.Equal(5, descriptor.NativeNullValue);
            Assert.Equal(1, descriptor.RequiredBits);

            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 1, 5 }, 0xffffffff, 5, true, ref descriptor);

            Assert.Equal(0, descriptor.MinValue);
            Assert.Equal(2, descriptor.MaxValue);
            Assert.True(descriptor.Nullable);
            Assert.Equal(5, descriptor.NativeNullValue);
            Assert.Equal(2, descriptor.RequiredBits);
        }
    }
}
