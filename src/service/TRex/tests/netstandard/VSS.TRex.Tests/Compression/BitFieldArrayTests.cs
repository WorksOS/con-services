using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Compression;
using Xunit;

namespace VSS.TRex.Tests.Compression
{
        public class BitFieldArrayTests
    {
        [Fact]
        public void Test_BitFieldArray_Creation()
        {
            BitFieldArray bfa = new BitFieldArray();

            Assert.Equal(0, bfa.NumBits);
            Assert.Equal(0, bfa.MemorySize());
        }

        [Fact]
        public void Test_BitFieldArray_Initialise1()
        {
            BitFieldArray bfa = new BitFieldArray();

            // Initialise with just a count of bits and records
            bfa.Initialise(10, 100);
            Assert.Equal(1000, bfa.NumBits);
            Assert.Equal(125, bfa.MemorySize()); // 125 bytes to store 1000 bits
        }

        [Fact]
        public void Test_BitFieldArray_Initialise2()
        {
            BitFieldArray bfa = new BitFieldArray();

            // Initialise with more than one field in a record

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor { BitsPerRecord = 10, NumRecords = 100 },
                new BitFieldArrayRecordsDescriptor { BitsPerRecord = 25, NumRecords = 500 }
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(13500, bfa.NumBits);
            Assert.Equal(1688, bfa.MemorySize()); // 1688 bytes to store 13500 bits
        }

        [Fact]
        public void Test_BitFieldArray_Initialise_FailWithTooLargeForUintError()
        {
            BitFieldArray bfa = new BitFieldArray();
        
            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
               new BitFieldArrayRecordsDescriptor { BitsPerRecord = 100, NumRecords = 1_000_000_000 },
            };

            Action act = () => bfa.Initialise(fieldsArray);
            act.Should().Throw<TRexPersistencyException>().WithMessage("Attempt to create bit field array with*");
        }
    
        [Fact]
        public void Test_BitFieldArray_Initialise_FailWithTooLargeToStore()
        {
          BitFieldArray bfa = new BitFieldArray();
    
          BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
          {
            new BitFieldArrayRecordsDescriptor { BitsPerRecord = 8, NumRecords = 260_000_000 },
          };
    
          Action act = () => bfa.Initialise(fieldsArray);
          act.Should().Throw<TRexPersistencyException>().WithMessage("*AllocateBuffer limited to*in size*");
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordSingleBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();
        
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor { BitsPerRecord = 1, NumRecords = 1 },
            };
        
            bfa.Initialise(fieldsArray);
        
            Assert.Equal(1, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(1, bfa.MemorySize());
        
            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] { 0, 1 }, 0xffffffff, 0, false, ref descriptor);
        
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

        private const bool NULLABLE = true;
        private const bool NOT_NULLABLE = false;

        [Theory]
        // Zero bits expected on number range of (0, 1), writing a value of 0 using 0 storage elements, 0 bytes of memory with 0 as the native null
        [InlineData(0, 1, 0, 0, 0x00000001, 0, 0, 0, 0, NULLABLE, 1)]
        [InlineData(0, 1, 0, 0, 0x00000001, 0, 0, 0, 0, NOT_NULLABLE, 1)]

        // One bits expected on number range of (0, 1), writing a value of 0 using 1 storage elements, 1 bytes of memory with 0 as the native null
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 0, NULLABLE, 2)]
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 0, NOT_NULLABLE, 2)]

        // One bits expected on number range of (0, 1), writing a value of 0 using 1 storage elements, 1 bytes of memory with 1 as the native null
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 1, NULLABLE, 2)]
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 1, NOT_NULLABLE, 2)]
        
        // One bits expected on number range of (0, 1), writing a value of 0 using 1 storage elements, 1 bytes of memory with 2 as the native null
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 2, NULLABLE, 2)]
        [InlineData(1, 1, 0, 1, 0x00000001, 0, 1, 1, 2, NOT_NULLABLE, 2)]
        
        // One bits expected on number range of (1, 2), writing a value of 1 using 1 storage elements, 1 bytes of memory with 1 as the native null
        [InlineData(1, 1, 1, 2, 0x00000003, 1, 1, 1, 1, NULLABLE, 2)]
        [InlineData(1, 1, 1, 2, 0x00000003, 1, 1, 1, 1, NOT_NULLABLE, 2)]
        
        // One/Three bits expected on number range of (5, 10), writing a value of 10 using 1 storage elements, 1 bytes of memory with 10 as the native null
        [InlineData(1, 1, 5, 10, 0x0000000f, 10, 1, 1, 10, NULLABLE, 2)]
        [InlineData(3, 1, 5, 10, 0x0000000f, 10, 1, 1, 10, NOT_NULLABLE, 6)]
        public void Test_BitFieldArray_SingleRecordSingleVariableSizeField_WithDescriptor
          (int bitsPerRecord, int numRecords, int minValue, int maxValue, int mask, long valueToWrite, 
           int numStorageElements, int memorySize, int nativeNullValue, bool fieldIsNullable, int expectedNumValues)
         {
             BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor { BitsPerRecord = bitsPerRecord, NumRecords = numRecords },
            };
            
            bfa.Initialise(fieldsArray);

            bitsPerRecord.Should().Be(bfa.NumBits);
            numStorageElements.Should().Be(bfa.NumStorageElements());
            Assert.Equal((int)memorySize, bfa.MemorySize());

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long [] { minValue, maxValue }, mask, nativeNullValue, fieldIsNullable, ref descriptor);

            descriptor.RequiredBits.Should().Be((byte)bitsPerRecord);
            descriptor.NumValues.Should().Be(expectedNumValues);

            // Write a value to the bfa
            bfa.StreamWriteStart();
            try
            {
                bfa.StreamWrite(valueToWrite, descriptor); 
            }
            finally
            {
                bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, descriptor);

            valueToWrite.Should().Be(readValue);
            bitsPerRecord.Should().Be(bitAddress);
        }

        [Theory]
        // Zero bits expected, writing a value of 0 using 0 storage elements, 0 bytes of memory
        [InlineData(0, 1, 0, 0, 0)]
      
        // One bits expected, writing a value of 0 using 1 storage elements, 1 bytes of memory
        [InlineData(1, 1, 0, 1, 1)]
                 
        // One bits expected, writing a value of 1 using 1 storage elements, 1 bytes of memory
        [InlineData(1, 1, 1, 1, 1)]

        // Four bits, writing a value of 10 using 1 storage elements, 1 bytes of memory
        [InlineData(4, 1, 10, 1, 1)]
        public void Test_BitFieldArray_SingleRecordSingleVariableSizeField_WithoutDescriptor
        (int bitsPerRecord, int numRecords, long valueToWrite, int numStorageElements, int memorySize)
        {
          BitFieldArray bfa = new BitFieldArray();
      
          BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
          {
              new BitFieldArrayRecordsDescriptor { BitsPerRecord = bitsPerRecord, NumRecords = numRecords },
          };
      
          bfa.Initialise(fieldsArray);
      
          bitsPerRecord.Should().Be(bfa.NumBits);
          numStorageElements.Should().Be(bfa.NumStorageElements());
          Assert.Equal((int)memorySize, bfa.MemorySize());
      
          // Write a value to the bfa
          bfa.StreamWriteStart();
          try
          {
            bfa.StreamWrite(valueToWrite, (int)bitsPerRecord);
          }
          finally
          {
            bfa.StreamWriteEnd();
          }
      
          // Read the value back again
      
          int bitAddress = 0;
          long readValue = bfa.ReadBitField(ref bitAddress, (int)bitsPerRecord);
      
          valueToWrite.Should().Be(readValue);
          bitsPerRecord.Should().Be(bitAddress);
        }
      
        [Fact]
        public void Test_BitFieldArray_ManyRecordsSingleBitField_WithDescriptor()
        {
            BitFieldArray bfa = new BitFieldArray();

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(1000, bfa.NumBits);
            Assert.Equal(16, bfa.NumStorageElements());
            Assert.Equal(125, bfa.MemorySize());

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long [] { 0, 1 }, 0xffffffff, 0, false, ref descriptor);

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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(2, bfa.MemorySize());

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long [] { 0, 2047 }, 0xffffffff, 0, false, ref descriptor);

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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11000, bfa.NumBits);
            Assert.Equal(172, bfa.NumStorageElements());
            Assert.Equal(1375, bfa.MemorySize());

            EncodedBitFieldDescriptor descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] { 0, 2047 }, 0xffffffff, 0, false, ref descriptor);

            // Write a '1234' to the bfa
            bfa.StreamWriteStart();
            try
            {
                for (int i = 0; i < 1000; i++)
                    bfa.StreamWrite(1234, descriptor);
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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(1, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(1, bfa.MemorySize());

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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 1, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(1000, bfa.NumBits);
            Assert.Equal(16, bfa.NumStorageElements());
            Assert.Equal(125, bfa.MemorySize());

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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(2, bfa.MemorySize());

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

            BitFieldArrayRecordsDescriptor[] fieldsArray = new []
            {
                new BitFieldArrayRecordsDescriptor() { BitsPerRecord = 11, NumRecords = 1000 },
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11000, bfa.NumBits);
            Assert.Equal(172, bfa.NumStorageElements());
            Assert.Equal(1375, bfa.MemorySize());

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

        [Fact]
        public void Test_BitFieldArray_StreamWriteEnd_FailWithImproperStreamEndPosition()
        {
          BitFieldArray bfa = new BitFieldArray();
          bfa.Initialise(1, 1);

          // Write a '1' to the bfa
          bfa.StreamWriteStart();
          bfa.StreamWrite(1, 1);
          bfa.StreamWriteStart(); // Set bit pos back to 0

          Action act = () => bfa.StreamWriteEnd(); // throw exception
          act.Should().Throw<TRexException>().WithMessage("*Stream bit position is not after last bit in storage*");
        }
    }
}
