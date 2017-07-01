using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Compression.Tests
{
    [TestClass]
    public class BitFieldArrayTests
    {
        [TestMethod]
        public void Test_BitFieldArray_Creation()
        {
            BitFieldArray bfa = new BitFieldArray();

            Assert.IsTrue(bfa.NumBits == 0, "BitFieldArray number of bits not zero on creation");
            Assert.IsTrue(bfa.MemorySize() == 0, "BitFieldArray memory size not zero on creation");
        }

        [TestMethod]
        public void Test_BitFieldArray_Initialise1()
        {
            BitFieldArray bfa = new BitFieldArray();

            // Initialise with just a count of bits and records
            bfa.Initialise(10, 100);
            Assert.IsTrue(bfa.NumBits == 1000, "BitFieldArray.NumBits not 1000 as expected (= {0})", bfa.NumBits);
            Assert.IsTrue(bfa.MemorySize() == 125, "BitFieldArray.MemorySize not 125 as expected: (= {0})", bfa.MemorySize()); // 125 bytes to store 1000 bits
        }

        [TestMethod]
        public void Test_BitFieldArray_Initialise2()
        {
            BitFieldArray bfa = new BitFieldArray();

            // Intialise with more than one field in a record

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 10, NumRecords = 100 },
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 25, NumRecords = 500 }
            };

            bfa.Initialise(fieldsArray);

            Assert.IsTrue(bfa.NumBits == 13500, "BitFieldArray.NumBits not 13500 as expected (= {0})", bfa.NumBits);
            Assert.IsTrue(bfa.MemorySize() == 1688, "BitFieldArray.MemorySize not 1688 as expected: (= {0})", bfa.MemorySize()); // 1688 bytes to store 13500 bits
        }

        [TestMethod]
        public void Test_BitFieldArray_SingleRecordSingleBitField()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1 },
            };
            
            bfa.Initialise(fieldsArray);

            Assert.AreEqual((uint)1, bfa.NumBits, "Number of bits incorrect.");
            Assert.AreEqual((uint)1, bfa.NumStorageElements(), "Number of storage elements incorrect,");
            Assert.AreEqual((uint)1, bfa.MemorySize(), "Number of bytes required incorrect,");

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 1 }, 0xffffffff, 0, false, ref descriptor);

            // Write a '1' to the bfa
            bfa.StreamWriteStart();
            try
            {
                bfa.StreamWrite(1, descriptor); 
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, descriptor);

            Assert.IsTrue(readValue == 1, "Expected value read from BFA is {0}, actually read {1}", 0, readValue);
            Assert.IsTrue(bitAddress == 1, "Resulting bit address not {0} as expected, but is {1}", 1, bitAddress);
        }

        [TestMethod]
        public void Test_BitFieldArray_ManyRecordsSingleBitField()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.AreEqual((uint)1000, bfa.NumBits, "Number of bits incorrect.");
            Assert.AreEqual((uint)16, bfa.NumStorageElements(), "Number of storage elements incorrect,");
            Assert.AreEqual((uint)125, bfa.MemorySize(), "Number of bytes required incorrect,");

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 1 }, 0xffffffff, 0, false, ref descriptor);

            // Write a '1' to the bfa
            bfa.StreamWriteStart();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    bfa.StreamWrite(1, descriptor);
                }
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            for (int i = 0; i < 1000; i++)
            {
                long readValue = bfa.ReadBitField(ref bitAddress, descriptor);

                Assert.IsTrue(readValue == 1, "Expected value read from BFA is {0}, actually read {1}", 0, readValue);
                Assert.IsTrue(bitAddress == i + 1, "Resulting bit address not {0} as expected, but is {1}", i + 1, bitAddress);
            }
        }

        [TestMethod]
        public void Test_BitFieldArray_SingleRecordMultiBitField()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.AreEqual((uint)11, bfa.NumBits, "Number of bits incorrect.");
            Assert.AreEqual((uint)1, bfa.NumStorageElements(), "Number of storage elements incorrect,");
            Assert.AreEqual((uint)2, bfa.MemorySize(), "Number of bytes required incorrect,");

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 2047 }, 0xffffffff, 0, false, ref descriptor);

            // Write a '1234' to the bfa
            bfa.StreamWriteStart();
            try
            {
                bfa.StreamWrite(1234, descriptor);
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, descriptor);

            Assert.IsTrue(readValue == 1234, "Expected value read from BFA is {0}, actually read {1}", 1234, readValue);
            Assert.IsTrue(bitAddress == 11, "Resulting bit address not {0} as expected, but is {1}", 11, bitAddress);
        }

        [TestMethod]
        public void Test_BitFieldArray_MultiRecordMultiBitField()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.AreEqual((uint)11000, bfa.NumBits, "Number of bits incorrect.");
            Assert.AreEqual((uint)172, bfa.NumStorageElements(), "Number of storage elements incorrect,");
            Assert.AreEqual((uint)1375, bfa.MemorySize(), "Number of bytes required incorrect,");

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new int[] { 0, 2047 }, 0xffffffff, 0, false, ref descriptor);

            // Write a '1234' to the bfa
            bfa.StreamWriteStart();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    bfa.StreamWrite(1234, descriptor);
                }
            }
            finally
            {
                bfa.StreamWriteEnd();
            }


            // Read the value back again

            int bitAddress = 0;

            for (int i = 0; i < 1000; i++)
            {
                long readValue = bfa.ReadBitField(ref bitAddress, descriptor);

                Assert.IsTrue(readValue == 1234, "Expected value read from BFA is {0}, actually read {1} at record index {2}, bit address {3}", 1234, readValue, i, bitAddress);
                Assert.IsTrue(bitAddress == (i+1) * 11, "Resulting bit address not {0} as expected, but is {1} at index {2}, bit address {3}", (i+1) * 11, bitAddress, i, bitAddress);
            }
        }
    }
}
