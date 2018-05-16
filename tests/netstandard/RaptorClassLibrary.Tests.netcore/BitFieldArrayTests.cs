using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.TRex.Compression.Tests
{
        public class BitFieldArrayTests
    {
        [Fact]
        public void Test_BitFieldArray_Creation()
        {
            BitFieldArray bfa = new BitFieldArray();

            Assert.Equal((uint)0, bfa.NumBits);
            Assert.Equal((uint)0, bfa.MemorySize());
        }

        [Fact]
        public void Test_BitFieldArray_Initialise1()
        {
            BitFieldArray bfa = new BitFieldArray();

            // Initialise with just a count of bits and records
            bfa.Initialise(10, 100);
            Assert.Equal((uint)1000, bfa.NumBits);
            Assert.Equal((uint)125, bfa.MemorySize()); // 125 bytes to store 1000 bits
        }

        [Fact]
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

            Assert.Equal((uint)13500, bfa.NumBits);
            Assert.Equal((uint)1688, bfa.MemorySize()); // 1688 bytes to store 13500 bits
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordSingleBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1 },
            };
            
            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)1, bfa.NumBits);
            Assert.Equal((uint)1, bfa.NumStorageElements());
            Assert.Equal((uint)1, bfa.MemorySize());

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

            Assert.Equal(1, readValue);
            Assert.Equal(1, bitAddress);
        }

        [Fact]
        public void Test_BitFieldArray_ManyRecordsSingleBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)1000, bfa.NumBits);
            Assert.Equal((uint)16, bfa.NumStorageElements());
            Assert.Equal((uint)125, bfa.MemorySize());

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

                Assert.Equal(1, readValue);
                Assert.Equal(bitAddress, i + 1);
            }
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordMultiBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)11, bfa.NumBits);
            Assert.Equal((uint)1, bfa.NumStorageElements());
            Assert.Equal((uint)2, bfa.MemorySize());

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

            Assert.Equal(1234, readValue);
            Assert.Equal(11, bitAddress);
        }

        [Fact]
        public void Test_BitFieldArray_MultiRecordMultiBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)11000, bfa.NumBits);
            Assert.Equal((uint)172, bfa.NumStorageElements());
            Assert.Equal((uint)1375, bfa.MemorySize());

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

                Assert.Equal(1234, readValue);
                Assert.Equal(bitAddress, (i+1) * 11);
            }
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordSingleBitField_WithoutDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)1, bfa.NumBits);
            Assert.Equal((uint)1, bfa.NumStorageElements());
            Assert.Equal((uint)1, bfa.MemorySize());

            // Write a '1' to the bfa
            bfa.StreamWriteStart();
            try
            {
                bfa.StreamWrite(1, 1);
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, 1);

            Assert.Equal(1, readValue);
            Assert.Equal(1, bitAddress);
        }

        [Fact]
        public void Test_BitFieldArray_ManyRecordsSingleBitField_WithoutDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)1000, bfa.NumBits);
            Assert.Equal((uint)16, bfa.NumStorageElements());
            Assert.Equal((uint)125, bfa.MemorySize());

            // Write a '1' to the bfa
            bfa.StreamWriteStart();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    bfa.StreamWrite(1, 1);
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
                long readValue = bfa.ReadBitField(ref bitAddress, 1);

                Assert.Equal(1, readValue);
                Assert.Equal(bitAddress, i + 1);
            }
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordMultiBitField_WithoutDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)11, bfa.NumBits);
            Assert.Equal((uint)1, bfa.NumStorageElements());
            Assert.Equal((uint)2, bfa.MemorySize());

            // Write a '1234' to the bfa
            bfa.StreamWriteStart();
            try
            {
                bfa.StreamWrite(1234, 11);
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, 11);

            Assert.Equal(1234, readValue);
            Assert.Equal(11, bitAddress);
        }

        [Fact]
        public void Test_BitFieldArray_MultiRecordMultiBitField_WithoutDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal((uint)11000, bfa.NumBits);
            Assert.Equal((uint)172, bfa.NumStorageElements());
            Assert.Equal((uint)1375, bfa.MemorySize());

            // Write a '1234' to the bfa
            bfa.StreamWriteStart();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    bfa.StreamWrite(1234, 11);
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
                long readValue = bfa.ReadBitField(ref bitAddress, 11);

                Assert.Equal(1234, readValue);
                Assert.Equal(bitAddress, (i + 1) * 11);
            }
        }
    }
}
