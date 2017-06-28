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
        public void Test_BitFieldArray_Initialisexxx()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 10, NumRecords = 100 },
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 25, NumRecords = 500 }
            };
        }
    }
}
