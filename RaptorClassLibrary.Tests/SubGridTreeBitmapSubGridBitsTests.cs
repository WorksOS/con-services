using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor;
using System.IO;
using VSS.VisionLink.Raptor.Geometry;
using System.Text;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class SubGridTreeBitmapSubGridBitsTests
    {
        [TestMethod]
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
                Assert.IsTrue(E is NullReferenceException, "Unexpected exception, should be NullReferenceException");
            }

            Assert.IsTrue(SawAnExcpetion, "Did not see an exception as expected.");

            // Test the constructor with filled false produces bitmask with all bits set to off
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            Assert.IsTrue(bits2.IsEmpty() && !bits2.IsFull(), "Bits is not empty as expected");

            // Test the constructor with filled true produces bitmask with all bits set to on
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            Assert.IsTrue(!bits3.IsEmpty() && bits3.IsFull(), "Bits is not full as expected");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Clear()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            Assert.IsTrue(bits.IsFull(), "Bits not full");

            bits.Clear();
            Assert.IsTrue(bits.IsEmpty(), "Bits not empty after performing a Clear()");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Fill()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            Assert.IsTrue(bits.IsEmpty(), "Bits not empty");

            bits.Fill();
            Assert.IsTrue(bits.IsFull(), "Bits not empty after performing a Fill()");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Equality()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            Assert.IsTrue(bits.Equals(bits2), "Bits does not equal bits2, which it should");
            Assert.IsFalse(bits.Equals(bits3), "Bits equals bits3, which it should not");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Assignment()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            Assert.IsTrue(bits.Equals(bits2), "Bits does not equal bits2, which it should");
            Assert.IsFalse(bits.Equals(bits3), "Bits equals bits3, which it should not");

            bits2.Assign(bits3);
            Assert.IsTrue(bits2.Equals(bits3), "Bits2 does not equal bits3 after assignment of bits3 to bits2, which it should");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_FullMask()
        {
            SubGridTreeBitmapSubGridBits bits = SubGridTreeBitmapSubGridBits.FullMask;

            Assert.IsTrue(bits.IsFull(), "Bits is not full after being assigned FullMask");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_CountBits()
        {
            SubGridTreeBitmapSubGridBits bits = SubGridTreeBitmapSubGridBits.FullMask;
            Assert.IsTrue(bits.CountBits() == SubGridTree.CellsPerSubgrid, "Countbits (result: {0}) is incorrect for a full mask (Expected: {1}", bits.CountBits(), SubGridTree.CellsPerSubgrid);

            bits.Clear();
            Assert.IsTrue(bits.CountBits() == 0, "Countbits is incorrect for an empty mask");

            bits.SetBit(1, 1);
            Assert.IsTrue(bits.CountBits() == 1, "Countbits is incorrect for an empty mask with a single bit then set in it");
        }

        [TestMethod]
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

            Assert.IsTrue(bits.Equals(bits2), "Bits not equal after serialisation with full mask");
        }

        [TestMethod]
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

            Assert.IsTrue(bits.Equals(bits2), "Bits not equal after serialisation with empty mask");
        }

        [TestMethod]
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

            Assert.IsTrue(bits.Equals(bits2), "Bits not equal after serialisation with arbitrary mask");
            Assert.IsTrue(bits.CountBits() == 4, "Count of bits in deserialised mask is not 4 as expect (result - {0}", bits.CountBits());
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SetBitValue()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBitValue(0, 0, true);

            Assert.IsTrue(bits.Bits[0] != 0, "Bit row 0 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 1, "Bit count not 1");

            bits.SetBitValue(0, 0, false);

            Assert.IsTrue(bits.Bits[0] == 0, "Bit row 0 not modified to 0 as expected");
            Assert.IsTrue(bits.CountBits() == 0, "Bit count not 0");

            bits.SetBitValue(31, 31, true);

            Assert.IsTrue(bits.Bits[31] != 0, "Bit row 31 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 1, "Bit count not 1");

            bits.SetBitValue(31, 31, false);

            Assert.IsTrue(bits.Bits[31] == 0, "Bit row 31 not modified to 0 as expected");
            Assert.IsTrue(bits.CountBits() == 0, "Bit count not 0");

        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SetBit()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBit(0, 0);

            Assert.IsTrue(bits.Bits[0] != 0, "Bit row 0 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 1, "Bit count not 1");

            bits.SetBit(31, 31);

            Assert.IsTrue(bits.Bits[31] != 0, "Bit row 0 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 2, "Bit count not 2");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ClearBit()
        {
            // Test setting a bit on and off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
            bits.SetBit(0, 0);

            Assert.IsTrue(bits.Bits[0] != 0, "Bit row 0 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 1, "Bit count not 1");

            bits.ClearBit(0, 0);

            Assert.IsTrue(bits.Bits[0] == 0, "Bit row 0 not modified to zero as expected");
            Assert.IsTrue(bits.CountBits() == 0, "Bit count not 0");

            bits.SetBit(31, 31);

            Assert.IsTrue(bits.Bits[31] != 0, "Bit row 31 not modified to non-zero as expected");
            Assert.IsTrue(bits.CountBits() == 1, "Bit count not 1");

            bits.ClearBit(31, 31);

            Assert.IsTrue(bits.Bits[31] == 0, "Bit row 31 not modified to zero as expected");
            Assert.IsTrue(bits.CountBits() == 0, "Bit count not 0");

        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_BitSet()
        {
            // Test testing a bit is on or off, at two corners to test boundary conditions
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(bits.BitSet(0, 0) == false, "Bit (0, 0) unexpectedly set to 1");
            Assert.IsTrue(bits.BitSet(31, 31) == false, "Bit (31, 31) unexpectedly set to 1");

            bits = SubGridTreeBitmapSubGridBits.FullMask;

            Assert.IsTrue(bits.BitSet(0, 0) == true, "Bit (0, 0) unexpectedly set to 0");
            Assert.IsTrue(bits.BitSet(31, 31) == true, "Bit (31, 31) unexpectedly set to 0");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ComputeCellsExtents()
        {
            // Test extents for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            BoundingIntegerExtent2D boundsFull = bits.ComputeCellsExtents();
            Assert.IsTrue(boundsFull.Equals(new BoundingIntegerExtent2D(0, 0, SubGridTree.SubGridTreeDimensionMinus1, SubGridTree.SubGridTreeDimensionMinus1)),
                "ComputeCellsExtents is incorrect for full grid");

            bits.Clear();
            BoundingIntegerExtent2D boundsClear = bits.ComputeCellsExtents();
            Assert.IsFalse(boundsClear.IsValidExtent, "ComputeCellsExtents is incorrect for clear grid");

            bits.SetBit(1, 1);
            BoundingIntegerExtent2D bounds11 = bits.ComputeCellsExtents();
            Assert.IsTrue(bounds11.Equals(new BoundingIntegerExtent2D(1, 1, 1, 1)), "ComputeCellsExtents is incorrect for grid with bit set at (1, 1)");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEach_Action()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == SubGridTree.CellsPerSubgrid, "Summation via ForEach on full mask did not give expected result");

            sum = 0;
            bits.Clear();
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == 0, "Summation via ForEach on empty mask did not give expected result");

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEach((x, y) => { if (bits.BitSet(x, y)) sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == 1, "Summation via ForEach on mask with single bit set at (1, 1) did not give expected result");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEach_Function()
        {
            // Test iteration function for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            bits.ForEach((x, y) => { return true; });
            Assert.IsTrue(bits.CountBits() == SubGridTree.CellsPerSubgrid, "All bits not set with function returning true");

            bits.Clear();
            bits.ForEach((x, y) => { return x < 16; });
            Assert.IsTrue(bits.CountBits() == SubGridTree.CellsPerSubgrid / 2, "Bits not set with function returning true for half of bits");

            bits.Clear();
            bits.ForEach((x, y) => { return (x == 1) && (y == 1); });
            Assert.IsTrue(bits.CountBits() == 1, "Bit not set fo rfunction returning (1, 1) as true");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEachSetBit()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == SubGridTree.CellsPerSubgrid, "Summation via ForEachSetBit on full mask did not give expected result");

            sum = 0;
            bits.Clear();
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == 0, "Summation via ForEachSetBit on empty mask did not give expected result");

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEachSetBit((x, y) => { sum++; });
            Assert.IsTrue(sum == bits.CountBits() && sum == 1, "Summation via ForEachSetBit on mask with single bit set at (1, 1) did not give expected result");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_ForEachClearBit()
        {
            // Test iteration action for empty, full and arbitrary masks
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

            int sum;

            sum = 0;
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.IsTrue(sum == 0, "Summation via ForEachClearBit on full mask did not give expected result");

            sum = 0;
            bits.Clear();
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.IsTrue(sum == SubGridTree.CellsPerSubgrid, "Summation via ForEachClearBit on empty mask did not give expected result: {0} vs {1}", sum, SubGridTree.CellsPerSubgrid);

            sum = 0;
            bits.SetBit(1, 1);
            bits.ForEachClearBit((x, y) => { sum++; });
            Assert.IsTrue(sum == SubGridTree.CellsPerSubgrid - 1, "Summation via ForEachClearBit on mask with single bit set at (1, 1) did not give expected result");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_RowToString()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            string s = bits.RowToString(0);
            Assert.IsTrue(s == " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0", "Row to string on empty row is incorrect, result = '{0}'", s);

            bits.Fill();
            string s2 = bits.RowToString(0);
            Assert.IsTrue(s2 == " 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1", "Row to string on full row is incorrect, result = '{0}'", s2);
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Equality()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(bits1 == bits2, "Equality check failed on identical masks");
            Assert.IsFalse(bits2 == bits3, "Equality check succeeded on different masks");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Inequality()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits3 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(bits1 != bits3, "Inequality check failed on different masks");
            Assert.IsFalse(bits1 != bits2, "Inequality check succeeded on identical masks");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_AND()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue((bits1 & bits2) == bits2, "ANDing clear and full masks did not produce an empty mask");

            bits1.ClearBit(1, 1);
            Assert.IsFalse((bits1 & bits2) == bits1, "ANDing after clearing bit did not return empty mask");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_OR()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue((bits1 | bits2) == bits1, "Oring clear and full masks did not produce a full mask");

            bits1.ClearBit(1, 1);
            Assert.IsTrue((bits1 | bits2) == bits1, "ORing after clearing bit did not return mask with clear bit");

            bits2.SetBit(1, 1);
            Assert.IsTrue((bits1 | bits2).IsFull(), "ORing after clearin/setting bits did not return full mask");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_XOR()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue((bits1 ^ bits1).IsEmpty(), "XORing bits with self did not result in empty mask");
            Assert.IsTrue(bits1 == (bits1 ^ bits2), "XORing an empty mask with a full mask did not produce a full mask");
            Assert.IsTrue((bits1 ^ bits2).IsFull(), "XORing an empty mask with a full mask did not produce a full mask");

            bits2.SetBit(1, 1);
            Assert.IsTrue((bits1 ^ bits1).IsEmpty(), "XORing single bit with itself did not clear bit");
            Assert.IsFalse((bits1 ^ bits1).BitSet(1, 1), "XORing single bit with itself did not clear bit");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_NOT()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(~bits1 == bits2, "NOTing a full mask did not produce an empty mask");
            Assert.IsTrue(bits1 == ~bits2, "NOTing an empty mask did not produce a full mask");

            bits1.ClearBit(1, 1);
            Assert.IsTrue((~bits1).BitSet(1, 1), "NOTing single bit did not flip it");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_Operators_Subtraction()
        {
            SubGridTreeBitmapSubGridBits bits1 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
            SubGridTreeBitmapSubGridBits bits2 = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue((bits1 - bits2).IsFull(), "Subtracting clear from full mask did not return full mask");
            Assert.IsTrue((bits2 - bits1).IsEmpty(), "Subtracting full from clear mask did not return clear mask");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_CellXY_Indexer()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(bits.IsEmpty(), "Bits not empty after creation");
            bits[0, 0] = true;
            Assert.IsFalse(bits.IsEmpty(), "Bits empty after setting bit");
            bits[0, 0] = false;
            Assert.IsTrue(bits.IsEmpty(), "Bits not empty after clearing bit");
        }

        [TestMethod]
        public void Test_SubGridTreeBitmapSubGridBitsTests_SumBitRows()
        {
            SubGridTreeBitmapSubGridBits bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            Assert.IsTrue(bits.SumBitRows() == 0, "Non-zero number of bits for empty bit mask");

            bits[0, 0] = true;
            Assert.IsFalse(bits.SumBitRows()== (1 << SubGridTree.SubGridTreeDimension) - 1, "Incorrect number of bits when bits[0,0] set");

            bits[0, SubGridTree.SubGridTreeDimensionMinus1] = true;
            Assert.IsFalse(bits.SumBitRows() == (1 << SubGridTree.SubGridTreeDimension), "Incorrect number of bits when bits[0,0] and bits[0,31] set");

            bits.Fill();
            Assert.IsTrue(bits.SumBitRows() == SubGridTreeBitmapSubGridBits.SumBitRowsFullCount, "Non-full number of bits for full bit mask");
        }
    }
}