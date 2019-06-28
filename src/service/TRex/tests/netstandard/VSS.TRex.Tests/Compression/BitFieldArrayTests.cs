using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Compression;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Compression
{
    public class BitFieldArrayTests : IClassFixture<DILoggingFixture>
    {
        [Fact]
        public void Test_BitFieldArray_Creation()
        {
          using (var bfa = new BitFieldArray())
          {
            Assert.Equal(0, bfa.NumBits);
            Assert.Equal(0, bfa.MemorySize());
          }
        }

        [Fact]
        public void Test_BitFieldArray_Initialise1()
        {
          using (var bfa = new BitFieldArray())
          {
            // Initialise with just a count of bits and records
            bfa.Initialise(10, 100);
            Assert.Equal(1000, bfa.NumBits);
            Assert.Equal(125, bfa.MemorySize()); // 125 bytes to store 1000 bits
          }
        }

        [Fact]
        public void Test_BitFieldArray_Initialise2()
        {
          using (var bfa = new BitFieldArray())
          {
            // Initialise with more than one field in a record

            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = 10, NumRecords = 100},
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = 25, NumRecords = 500}
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(13500, bfa.NumBits);
            Assert.Equal(1688, bfa.MemorySize()); // 1688 bytes to store 13500 bits
          }
        }

        [Fact]
        public void Test_BitFieldArray_Initialise_FailWithTooLargeForUintError()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = 100, NumRecords = 1_000_000_000},
            };

            Action act = () => bfa.Initialise(fieldsArray);
            act.Should().Throw<TRexPersistencyException>().WithMessage("Attempt to create bit field array with*");
          }
        }
    
        [Fact]
        public void Test_BitFieldArray_Initialise_FailWithTooLargeToStore()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new BitFieldArrayRecordsDescriptor[]
            {
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = 8, NumRecords = 260_000_000},
            };

            Action act = () => bfa.Initialise(fieldsArray);
            act.Should().Throw<TRexPersistencyException>().WithMessage("*AllocateBuffer limited to*in size*");
          }
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordSingleBitField_WithDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = 1, NumRecords = 1},
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(1, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(1, bfa.MemorySize());

            var descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] {0, 1}, 0, 2, 0xffffffff, 0, false,
              ref descriptor);

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
           using (var bfa = new BitFieldArray())
           {
             BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
             {
               new BitFieldArrayRecordsDescriptor {BitsPerRecord = bitsPerRecord, NumRecords = numRecords},
             };

             bfa.Initialise(fieldsArray);

             bitsPerRecord.Should().Be(bfa.NumBits);
             numStorageElements.Should().Be(bfa.NumStorageElements());
             Assert.Equal((int) memorySize, bfa.MemorySize());

             var descriptor = new EncodedBitFieldDescriptor();
             AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] {minValue, maxValue}, 0, 2, mask,
               nativeNullValue, fieldIsNullable, ref descriptor);

             descriptor.RequiredBits.Should().Be((byte) bitsPerRecord);
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
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor {BitsPerRecord = bitsPerRecord, NumRecords = numRecords},
            };

            bfa.Initialise(fieldsArray);

            bitsPerRecord.Should().Be(bfa.NumBits);
            numStorageElements.Should().Be(bfa.NumStorageElements());
            Assert.Equal((int) memorySize, bfa.MemorySize());

            // Write a value to the bfa
            bfa.StreamWriteStart();
            try
            {
              bfa.StreamWrite(valueToWrite, (int) bitsPerRecord);
            }
            finally
            {
              bfa.StreamWriteEnd();
            }

            // Read the value back again

            int bitAddress = 0;
            long readValue = bfa.ReadBitField(ref bitAddress, (int) bitsPerRecord);

            valueToWrite.Should().Be(readValue);
            bitsPerRecord.Should().Be(bitAddress);
          }
        }
      
        [Fact]
        public void Test_BitFieldArray_ManyRecordsSingleBitField_WithDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 1, NumRecords = 1000},
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(1000, bfa.NumBits);
            Assert.Equal(16, bfa.NumStorageElements());
            Assert.Equal(125, bfa.MemorySize());

            var descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] {0, 1}, 0, 2, 0xffffffff, 0, false,
              ref descriptor);

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
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordMultiBitField_WithDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 11, NumRecords = 1},
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11, bfa.NumBits);
            Assert.Equal(1, bfa.NumStorageElements());
            Assert.Equal(2, bfa.MemorySize());

            var descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] {0, 2047}, 0, 2, 0xffffffff, 0, false,
              ref descriptor);

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
        }

        [Fact]
        public void Test_BitFieldArray_MultiRecordMultiBitField_WithDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 11, NumRecords = 1000},
            };

            bfa.Initialise(fieldsArray);

            Assert.Equal(11000, bfa.NumBits);
            Assert.Equal(172, bfa.NumStorageElements());
            Assert.Equal(1375, bfa.MemorySize());

            var descriptor = new EncodedBitFieldDescriptor();
            AttributeValueRangeCalculator.CalculateAttributeValueRange(new long[] {0, 2047}, 0, 2, 0xffffffff, 0, false,
              ref descriptor);

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
              Assert.Equal(bitAddress, (i + 1) * 11);
            }
          }
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordSingleBitField_WithoutDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 1, NumRecords = 1},
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
        }

        [Fact]
        public void Test_BitFieldArray_ManyRecordsSingleBitField_WithoutDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 1, NumRecords = 1000},
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
        }

        [Fact]
        public void Test_BitFieldArray_SingleRecordMultiBitField_WithoutDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 11, NumRecords = 1},
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
        }

        [Fact]
        public void Test_BitFieldArray_MultiRecordMultiBitField_WithoutDescriptor()
        {
          using (var bfa = new BitFieldArray())
          {
            BitFieldArrayRecordsDescriptor[] fieldsArray = new[]
            {
              new BitFieldArrayRecordsDescriptor() {BitsPerRecord = 11, NumRecords = 1000},
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
        }

        [Fact]
        public void Test_BitFieldArray_StreamWriteEnd_FailWithImproperStreamEndPosition()
        {
          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(1, 1);

            // Write a '1' to the bfa
            bfa.StreamWriteStart();
            bfa.StreamWrite(1, 1);
            bfa.StreamWriteStart(); // Set bit pos back to 0

            Action act = () => bfa.StreamWriteEnd(); // throw exception
            act.Should().Throw<TRexException>().WithMessage("*Stream bit position is not after last bit in storage*");
          }
        }

        [Fact]
        public void WriteBitFieldVector_Success()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length);

            int bitLocation = 0;
            bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length, values);

            bitLocation.Should().Be(values.Length * descriptor.RequiredBits);
          }
        }

        [Fact]
        public void WriteBitFieldVector_Success_PartialWrite()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;
          int partialWriteSize = 5;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length);

            int bitLocation = 0;
            bfa.WriteBitFieldVector(ref bitLocation, descriptor, partialWriteSize, values);

            bitLocation.Should().Be(descriptor.RequiredBits * partialWriteSize);
          }
        }

        [Fact]
        public void WriteBitFieldVector_FailWithValuesArraySize()
        {
          long[] values = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length);

            int bitLocation = 0;

            Action act = () => bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length + 1, values);
            act.Should().Throw<ArgumentException>().WithMessage("Supplied values array is null or not large enough*");
          }
        }

        [Fact]
        public void WriteBitFieldVector_FailWithStorageSize()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length - 1);

            int bitLocation = 0;

            Action act = () => bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length, values);
            act.Should().Throw<ArgumentException>().WithMessage("Insufficient storage present to satisfy*");
          }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(48)]
        [InlineData(63)]
        public void ReadBitFieldVector_Success(byte valueBits)
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = valueBits;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(valueBits, values.Length);

            int bitLocation = 0;
            bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length, values);

            // Read the vector back
            bitLocation = 0;
            long[] outValues = new long[values.Length];
            bfa.ReadBitFieldVector(ref bitLocation, descriptor, outValues.Length, outValues);
            bitLocation.Should().Be(outValues.Length * valueBits);

            outValues.Should().BeEquivalentTo(values);
          }
        }

        [Fact]
        public void ReadBitFieldVector_SuccessPartial()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length);

            int bitLocation = 0;
            bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length, values);

            // Read the vector back
            bitLocation = 0;
            long[] outValues = new long[5];
            bfa.ReadBitFieldVector(ref bitLocation, descriptor, outValues.Length, outValues);
            bitLocation.Should().Be(outValues.Length * descriptor.RequiredBits);

            outValues.ForEach((l, i) => l.Should().Be(values[i]));
          }
        }

        [Fact]
        public void ReadBitFieldVector_FailWithValuesArraySize()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length);

            int bitLocation = 0;

            bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length, values);

            Action act = () => bfa.ReadBitFieldVector(ref bitLocation, descriptor, values.Length + 1, values);
            act.Should().Throw<ArgumentException>().WithMessage("Supplied values array is null or not large enough*");
          }
        }

        [Fact]
        public void ReadBitFieldVector_FailWithStorageSize()
        {
          long[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
          var descriptor = new EncodedBitFieldDescriptor();
          descriptor.RequiredBits = 4;

          using (var bfa = new BitFieldArray())
          {
            bfa.Initialise(descriptor.RequiredBits, values.Length - 1);

            int bitLocation = 0;
            bfa.WriteBitFieldVector(ref bitLocation, descriptor, values.Length - 1, values);

            Action act = () => bfa.ReadBitFieldVector(ref bitLocation, descriptor, values.Length, values);
            act.Should().Throw<ArgumentException>().WithMessage("Insufficient values present to satisfy a*");
          }
        }
  }
}
