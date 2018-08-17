using System;
using VSS.TRex;
using System.IO;
using System.Text;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTreeBitmapSubGridBitsTests
    {
        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Creation()
        {
            // Test the default constructor produces an AV as the bits array will be null
            bool SawAnExcpetion = false;
            try
            {
                SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits();
                bits.Bits[0] = 0;
            }
            catch (Exception E)
            {
                SawAnExcpetion = true;
                Assert.True(E is NullReferenceException, "Unexpected exception, should be NullReferenceException");
            }

            Assert.True(SawAnExcpetion, "Did not see an exception as expected.");

            // Test the constructor with filled false produces bitmask with all bits set to off
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            Assert.True(bits2.IsEmpty() && !bits2.IsFull(), "Bits is not empty as expected");

            // Test the constructor with filled true produces bitmask with all bits set to on
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            Assert.True(!bits3.IsEmpty() && bits3.IsFull(), "Bits is not full as expected");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Clear()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            Assert.True(bits.IsFull(), "Bits not full");

            bits.Clear();
            Assert.True(bits.IsEmpty(), "Bits not empty after performing a Clear()");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Fill()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            Assert.True(bits.IsEmpty(), "Bits not empty");

            bits.Fill();
            Assert.True(bits.IsFull(), "Bits not empty after performing a Fill()");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Equality()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            Assert.True(bits.Equals(bits2), "Bits does not equal bits2, which it should");
            Assert.False(bits.Equals(bits3), "Bits equals bits3, which it should not");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Assignment()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            Assert.True(bits.Equals(bits2), "Bits does not equal bits2, which it should");
            Assert.False(bits.Equals(bits3), "Bits equals bits3, which it should not");

            bits2.Assign(bits3);
            Assert.True(bits2.Equals(bits3), "Bits2 does not equal bits3 after assignment of bits3 to bits2, which it should");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_FullMask()
        {
            SubGridTreeBitmapSubGridBits bits = SubGridTreeBitmapSubGridBits.FullMask;

            Assert.True(bits.IsFull(), "Bits is not full after being assigned FullMask");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_CountBits()
        {
            SubGridTreeBitmapSubGridBits bits = SubGridTreeBitmapSubGridBits.FullMask;
            Assert.Equal(bits.CountBits(), SubGridTreeConsts.CellsPerSubgrid);

            bits.Clear();
            Assert.Equal((uint)0, bits.CountBits());

            bits.SetBit(1, 1);
            Assert.Equal((uint)1, bits.CountBits());
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Serialisation_Full()
        {
            // Test serialisation with full mask
            SubGridTreeBitmapSubGridBits bits = SubGridTreeBitmapSubGridBits.FullMask;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, true);

            bits.Write(bw, new byte[1000]);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8, true);
            ms.Position = 0;

            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits2.Read(br, new byte[10000]);

            Assert.True(bits.Equals(bits2), "Bits not equal after serialisation with full mask");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Serialisation_Empty()
        {
            // Test serialisation with full mask
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, true);

            bits.Write(bw, new byte[1000]);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8, true);
            ms.Position = 0;

            SubGridTreeBitmapSubGridBits bits2 = SubGridTreeBitmapSubGridBits.FullMask;
            bits2.Read(br, new byte[10000]);

            Assert.True(bits.Equals(bits2), "Bits not equal after serialisation with empty mask");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Serialisation_Arbitrary()
        {
            // Test serialisation with arbitrary bits set
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBit(0, 0);
            bits.SetBit(10, 10);
            bits.SetBit(20, 20);
            bits.SetBit(31, 31);

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, true);

            bits.Write(bw, new byte[1000]);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8, true);
            ms.Position = 0;

            SubGridTreeBitmapSubGridBits bits2 = SubGridTreeBitmapSubGridBits.FullMask;
            bits2.Read(br, new byte[10000]);

            Assert.True(bits.Equals(bits2), "Bits not equal after serialisation with arbitrary mask");
            Assert.Equal((uint)4, bits.CountBits());
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SetBitValue()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBitValue(0, 0, true);

            Assert.NotEqual((uint)0, bits.Bits[0]);
            Assert.Equal((uint)1, bits.CountBits());

            bits.SetBitValue(0, 0, false);

            Assert.Equal((uint)0, bits.Bits[0]);
            Assert.Equal((uint)0, bits.CountBits());

            bits.SetBitValue(31, 31, true);

            Assert.NotEqual((uint)0, bits.Bits[31]);
            Assert.Equal((uint)1, bits.CountBits());

            bits.SetBitValue(31, 31, false);

            Assert.Equal((uint)0, bits.Bits[31]);
            Assert.Equal((uint)0, bits.CountBits());

        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SetBit()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBit(0, 0);

            Assert.NotEqual((uint)0, bits.Bits[0]);
            Assert.Equal((uint)1, bits.CountBits());

            bits.SetBit(31, 31);

            Assert.NotEqual((uint)0, bits.Bits[31]);
            Assert.Equal((uint)2, bits.CountBits());
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ClearBit()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBit(0, 0);

            Assert.NotEqual((uint)0, bits.Bits[0]);
            Assert.Equal((uint)1, bits.CountBits());

            bits.ClearBit(0, 0);

            Assert.Equal((uint)0, bits.Bits[0]);
            Assert.Equal((uint)0, bits.CountBits());

            bits.SetBit(31, 31);

            Assert.NotEqual((uint)0, bits.Bits[31]);
            Assert.Equal((uint)1, bits.CountBits());

            bits.ClearBit(31, 31);

            Assert.Equal((uint)0, bits.Bits[31]);
            Assert.Equal((uint)0, bits.CountBits());

        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_BitSet()
        {
            // Test testing a bit is on or off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.False(bits.BitSet(0, 0));
            Assert.False(bits.BitSet(31, 31));

            bits.Fill();

            Assert.True(bits.BitSet(0, 0));
            Assert.True(bits.BitSet(31, 31));
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ComputeCellsExtents()
        {
            // Test extents for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            BoundingIntegerExtent2D boundsFull = bits.ComputeCellsExtents();
            Assert.True(boundsFull.Equals(new BoundingIntegerExtent2D(0, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1)),
                "ComputeCellsExtents is incorrect for full grid");

            bits.Clear();
            BoundingIntegerExtent2D boundsClear = bits.ComputeCellsExtents();
            Assert.False(boundsClear.IsValidExtent, "ComputeCellsExtents is incorrect for clear grid");

            bits.SetBit(1, 1);
            BoundingIntegerExtent2D bounds11 = bits.ComputeCellsExtents();
            Assert.True(bounds11.Equals(new BoundingIntegerExtent2D(1, 1, 1, 1)), "ComputeCellsExtents is incorrect for grid with bit set at (1, 1)");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEach_Action()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.True(sum == bits.CountBits() && sum == SubGridTreeConsts.CellsPerSubgrid, "Summation via ForEach on full mask did not give expected result");

            sum = 0;
            bits.Clear();
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.True(sum == bits.CountBits() && sum == 0, "Summation via ForEach on empty mask did not give expected result");

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.True(sum == bits.CountBits() && sum == 1, "Summation via ForEach on mask with single bit set at (1, 1) did not give expected result");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEach_Function()
        {
            // Test iteration function for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            bits.ForEach((x, y) => { return true; });
            Assert.Equal(bits.CountBits(), SubGridTreeConsts.CellsPerSubgrid);

            bits.Clear();
            bits.ForEach((x, y) => { return x < 16; });
            Assert.Equal(bits.CountBits(), SubGridTreeConsts.CellsPerSubgrid / 2);

            bits.Clear();
            bits.ForEach((x, y) => { return (x == 1) && (y == 1); });
            Assert.Equal((uint)1, bits.CountBits());
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEachSetBit()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.True(sum == bits.CountBits() && sum == SubGridTreeConsts.CellsPerSubgrid, "Summation via ForEachSetBit on full mask did not give expected result");

            sum = 0;
            bits.Clear();
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.True(sum == bits.CountBits() && sum == 0, "Summation via ForEachSetBit on empty mask did not give expected result");

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.True(sum == bits.CountBits() && sum == 1, "Summation via ForEachSetBit on mask with single bit set at (1, 1) did not give expected result");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEachClearBit()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.Equal(0, sum);

            sum = 0;
            bits.Clear();
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.Equal((uint)sum, SubGridTreeConsts.CellsPerSubgrid);

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.Equal((uint)sum, SubGridTreeConsts.CellsPerSubgrid - 1);
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_RowToString()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            string s = bits.RowToString(0);
            Assert.True(s == " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0");

            bits.Fill();
            string s2 = bits.RowToString(0);
            Assert.True(s2 == " 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Equality()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.Equal(bits1, bits2);
            Assert.NotEqual(bits2, bits3);
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Inequality()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.NotEqual(bits1, bits3);
            Assert.Equal(bits1, bits2);
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_AND()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.Equal((bits1 & bits2), bits2);

            bits1.ClearBit(1, 1);
            Assert.NotEqual((bits1 & bits2), bits1);
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_OR()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.Equal((bits1 | bits2), bits1);

            bits1.ClearBit(1, 1);
            Assert.Equal((bits1 | bits2), bits1);

            bits2.SetBit(1, 1);
            Assert.True((bits1 | bits2).IsFull(), "ORing after clearin/setting bits did not return full mask");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_XOR()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.True((bits1 ^ bits1).IsEmpty(), "XORing bits with self did not result in empty mask");
            Assert.Equal(bits1, (bits1 ^ bits2));
            Assert.True((bits1 ^ bits2).IsFull(), "XORing an empty mask with a full mask did not produce a full mask");

            bits2.SetBit(1, 1);
            Assert.True((bits1 ^ bits1).IsEmpty(), "XORing single bit with itself did not clear bit");
            Assert.False((bits1 ^ bits1).BitSet(1, 1), "XORing single bit with itself did not clear bit");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_NOT()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.Equal(~bits1, bits2);
            Assert.Equal(bits1, ~bits2);

            bits1.ClearBit(1, 1);
            Assert.True((~bits1).BitSet(1, 1), "NOTing single bit did not flip it");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Subtraction()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.True((bits1 - bits2).IsFull(), "Subtracting clear from full mask did not return full mask");
            Assert.True((bits2 - bits1).IsEmpty(), "Subtracting full from clear mask did not return clear mask");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_CellXY_Indexer()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.True(bits.IsEmpty(), "Bits not empty after creation");
            bits[0, 0] = true;
            Assert.False(bits.IsEmpty(), "Bits empty after setting bit");
            bits[0, 0] = false;
            Assert.True(bits.IsEmpty(), "Bits not empty after clearing bit");
        }

        [Fact]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SumBitRows()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.Equal(0, bits.SumBitRows());

            bits[0, 0] = true;
            Assert.NotEqual(bits.SumBitRows(), (1 << SubGridTreeConsts.SubGridTreeDimension) - 1);

            bits[0, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;
            Assert.NotEqual(bits.SumBitRows(), (1 << SubGridTreeConsts.SubGridTreeDimension));

            bits.Fill();
            Assert.Equal(bits.SumBitRows(), SubGridTreeBitmapSubGridBits.SumBitRowsFullCount);
        }
    }
}
