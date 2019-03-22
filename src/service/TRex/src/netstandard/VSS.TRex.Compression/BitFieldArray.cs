using System;
using System.IO;
using VSS.TRex.Common.Exceptions;

// This unit implements support for storing attribute values as variable bit field arrays and records 

namespace VSS.TRex.Compression
{
    /// <summary>
    /// Bit field arrays implement arrays variable bit fields to more efficiently use memory.
    /// They are commonly used in conjunction with dynamic/entropic compression of vectors of values being
    /// stored by the arrays. Records may be implemented that represent sets of fields, where the fields for each record are
    /// contiguously stored in memory to improve access locality.
    /// </summary>
    public struct BitFieldArray
    {
        /// <summary>
        /// Read this many bytes at a time from the BitFieldArray storage when reading values into elements in the internal storage array
        /// </summary>
        private const int N_BYTES_TO_READ_AT_A_TIME = 8; // was 1

        /// <summary>
        /// Read this many bits at a time from the BitFieldArray storage when reading values
        /// </summary>
        private const int N_BITS_TO_READ_AT_A_TIME = N_BYTES_TO_READ_AT_A_TIME * 8;

        /// <summary>
        /// The number of bits necessary to shift the offset address for a field in a bit field array to 
        /// convert the the bit address into logical blocks (ie: the block size at which bits are read from memory
        /// when reading or writing values)
        /// </summary>
        private const int BIT_LOCATION_TO_BLOCK_SHIFT = 6; // 1 Byte : 3, 2 Bytes : 4, 4 Bytes : 5, 8 bytes : 6

        /// <summary>
        /// The number of bits required to express the power of 2 sized block of bits, eg: a block of 8 bits needs
        /// 3 bits to express the number range 0..7m so the mask if 0x7 (0b111), etc
        /// </summary>
        private const uint BITS_REMAINING_IN_STORAGE_BLOCK_MASK = 0x3f; // 1 Byte : $7, 2 bytes : $f, 4 bytes : $1f, 8 bytes : $3f

        /// <summary>
        /// The maximum number of bytes a bit field array is permitted to allocate to store its contents
        /// </summary>
        public const uint MAXIMUM_BIT_FIELD_ARRAY_MEMORY_SIZE_BYTES = 256 * 1024 * 1024;

        /// <summary>
        /// Storage is the memory allocated to storing the bit field array.
        /// </summary>
        private ulong[] Storage;

        /// <summary>
        /// The current bit address position in a bit field array during a stream write operation into it.
        /// </summary>
        private uint StreamWriteBitPos;

        /// <summary>
        /// Allocates a block of memory large enough to store the number of bits required (See FNumBits * MemorySize())
        /// </summary>
        private void AllocateBuffer()
        {
            var memorySize = MemorySize();
            if (memorySize == 0)
                return; // No storage required (yes, this can happen, eg: PassCounts for a segment with only a single cell pass)

            if (memorySize > MAXIMUM_BIT_FIELD_ARRAY_MEMORY_SIZE_BYTES)            
                throw new TRexPersistencyException($"BitFieldArray.AllocateBuffer limited to {MAXIMUM_BIT_FIELD_ARRAY_MEMORY_SIZE_BYTES / (1024 * 1024)}Mb in size ({MemorySize() / (1024 * 1024)}Mb requested)");

            Storage = NewStorage();
        }

        /// <summary>
        /// The total number of bits required to be stored in the bit field array
        /// </summary>
        public uint NumBits { get; private set; }

        /// <summary>
        /// MemorySize returns the total number of bytes required to store the information in the bit field array
        /// </summary>
        /// <returns></returns>
        public uint MemorySize() => (NumBits & 0x7) != 0 ? (NumBits >> 3) + 1 : NumBits >> 3;

        /// <summary>
        /// Determines the number of elements required in the storage array depending on the 'read/write block size'
        /// </summary>
        /// <returns></returns>
        public uint NumStorageElements() => MemorySize() % N_BYTES_TO_READ_AT_A_TIME == 0 ? MemorySize() / N_BYTES_TO_READ_AT_A_TIME : (MemorySize() / N_BYTES_TO_READ_AT_A_TIME) + 1; // FNumBits % N_BITS_TO_READ_AT_A_TIME == 0 ? FNumBits / N_BITS_TO_READ_AT_A_TIME : FNumBits / N_BITS_TO_READ_AT_A_TIME + 1;

        /// <summary>
        /// Allocates the storage array for storing the block that comprise the bit field array
        /// </summary>
        /// <returns></returns>
        private ulong[] NewStorage() => new ulong[NumStorageElements()];

        /// <summary>
        /// Initialise the bit field array ready to store NumRecords each requiring BitsPerRecord storage
        /// </summary>
        /// <param name="bitsPerRecord"></param>
        /// <param name="numRecords"></param>
        public void Initialise(int bitsPerRecord, int numRecords)
        {
            NumBits = (uint)((long)bitsPerRecord * numRecords);

            AllocateBuffer();
        }

        /// <summary>
        /// Initialise the BitFieldArray using a descriptor that details the number of records and the size in bits of each of the records
        /// </summary>
        /// <param name="recordsArray"></param>
        public void Initialise(BitFieldArrayRecordsDescriptor[] recordsArray)
        {
            long _numBits = 0;

            foreach (BitFieldArrayRecordsDescriptor descriptor in recordsArray)
                _numBits += (long)descriptor.NumRecords * descriptor.BitsPerRecord;

            if (_numBits > uint.MaxValue)
               throw new TRexPersistencyException($"Attempt to create bit field array with {_numBits} which is more than the {uint.MaxValue} limit");

            NumBits = checked((uint) _numBits);

            AllocateBuffer();
        }

        /// <summary>
        /// Writes the content of the BitFieldArray using the supplied BinaryWriter
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(NumBits);

            if (NumBits == 0)
                return;

            byte[] buffer = new byte[Storage.Length * sizeof(ulong)];
            Buffer.BlockCopy(Storage, 0, buffer, 0, Storage.Length * sizeof(ulong));
            writer.Write(buffer);
        }

        /// <summary>
        /// Reads the content of the BitFieldArray using the supplied reader
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            Storage = null;
            NumBits = reader.ReadUInt32();

            if (NumBits == 0)
                return;

            Storage = NewStorage();

            byte[] buffer = new byte[Storage.Length * sizeof(ulong)];
            reader.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, Storage, 0, buffer.Length);
        }

        /// <summary>
        /// StreamWriteStart initialise the bit field array for streamed writing from the start of the allocated memory
        /// </summary>
        public void StreamWriteStart()
        {
            StreamWriteBitPos = 0;
        }

        /// <summary>
        /// StreamWrite takes a native unsigned integer value value depending on OS,
        /// plus a number of bits and writes the least significant ValueBits of that value
        /// into the bit field array.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// native to encoded null value conversion and deal with subtracting the minvalue
        /// from the descriptor in the encoded value.
        /// The variant that take an exact number of value bits is a raw version of the write method that
        /// just writes the given value in the given bits.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="descriptor"></param>
        public void StreamWrite(long value, EncodedBitFieldDescriptor descriptor)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the element referenced by FStreamWritePos
            // 2: If there are still more than N_BITS_TO_READ_AT_A_TIME remaining bits to be written, then write
            //    individual elements from value into Storage until the remaining number of bits
            //    to be written is less than N_BITS_TO_READ_AT_A_TIME
            // 3: If there are still (less than N_BITS_TO_READ_AT_A_TIME) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty element in Storage

            int ValueBits = descriptor.RequiredBits;

            if (ValueBits == 0) // There's nothing to do!
                return;

            value = descriptor.Nullable && value == descriptor.NativeNullValue 
              ? descriptor.EncodedNullValue - descriptor.MinValue 
              : value - descriptor.MinValue;

            // Be paranoid! Ensure there are no bits set in the high order bits above the least significant valueBits in Value
            value = value & ((1 << ValueBits) - 1);

            int StoragePointer = unchecked((int)(StreamWriteBitPos >> BIT_LOCATION_TO_BLOCK_SHIFT));
            int AvailBitsInCurrentStorageElement = N_BITS_TO_READ_AT_A_TIME - unchecked((byte)(StreamWriteBitPos & BITS_REMAINING_IN_STORAGE_BLOCK_MASK));

            // Write initial bits into storage element
            if (AvailBitsInCurrentStorageElement >= ValueBits)
            {
                Storage[StoragePointer] |=  unchecked((ulong)value) << (AvailBitsInCurrentStorageElement - ValueBits);
                StreamWriteBitPos += unchecked((uint)ValueBits);   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageElement
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = ValueBits - AvailBitsInCurrentStorageElement;
            Storage[StoragePointer] |= unchecked((ulong)value) >> RemainingBitsToWrite;

            /* When using long elements, there can never be a value stored that is larger that the storage element
            // Step 2: Write whole elements
            while (RemainingBitsToWrite > N_BITS_TO_READ_AT_A_TIME)
            {
                RemainingBitsToWrite -= N_BITS_TO_READ_AT_A_TIME;
                Storage[++StoragePointer] = (byte)(value >> RemainingBitsToWrite); // Peel of the next element and place it into storage
            }
            */

            // Step 3: Write remaining bits into next element in Storage
            if (RemainingBitsToWrite > 0) // Mask out the bits we want...
                Storage[StoragePointer + 1] = (unchecked((ulong)value) & (((ulong)1 << RemainingBitsToWrite) - 1)) << (N_BITS_TO_READ_AT_A_TIME - RemainingBitsToWrite); 

            StreamWriteBitPos += (uint)ValueBits;   // Advance the current bit position pointer;
        }

        /// <summary>
        /// StreamWrite takes a native unsigned integer value value depending on OS,
        /// plus a number of bits and writes the least significant ValueBits of that value
        /// into the bit field array.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// native to encoded null value conversion and deal with subtracting the minvalue
        /// from the descriptor in the encoded value.
        /// The variant that take an exact number of value bits is a raw version of the write method that
        /// just writes the given value in the given bits.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueBits"></param>
        public void StreamWrite(long value, int valueBits)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the element referenced by FStreamWritePos
            // 2: If there are still more than N_BITS_TO_READ_AT_A_TIME remaining bits to be written, then write
            //    individual elements from value into Storage until the remaining number of bits
            //    to be written is less than N_BITS_TO_READ_AT_A_TIME
            // 3: If there are still (less than N_BITS_TO_READ_AT_A_TIME) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty element in Storage

            if (valueBits == 0) // There's nothing to do!
                return;

            // Be paranoid! Ensure there are no bits set in the high order bits above the
            // least significant valueBits in Value
            value = value & ((1 << valueBits) - 1);

            uint StoragePointer = StreamWriteBitPos >> BIT_LOCATION_TO_BLOCK_SHIFT;
            int AvailBitsInCurrentStorageElement = unchecked((int)(N_BITS_TO_READ_AT_A_TIME - (StreamWriteBitPos & BITS_REMAINING_IN_STORAGE_BLOCK_MASK)));

            // Write initial bits into storage element
            if (AvailBitsInCurrentStorageElement >= valueBits)
            {
                Storage[StoragePointer] |= (unchecked((ulong)value) << (AvailBitsInCurrentStorageElement - valueBits));
                StreamWriteBitPos += unchecked((uint)valueBits);   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageElement
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = valueBits - AvailBitsInCurrentStorageElement;
            Storage[StoragePointer] |= (unchecked((ulong)value) >> RemainingBitsToWrite);

            /* When using long elements, there can never be a value stored that is larger that the storage element
            // Step 2: Write whole elements
            while (RemainingBitsToWrite > N_BITS_TO_READ_AT_A_TIME)
            {
                RemainingBitsToWrite -= N_BITS_TO_READ_AT_A_TIME;
                Storage[++StoragePointer] = (byte)(value >> RemainingBitsToWrite); // Peel of the next element and place it into storage
            }
            */

            // Step 3: Write remaining bits into next element in Storage
            if (RemainingBitsToWrite > 0) // Mask out the bits we want...
                Storage[StoragePointer + 1] = ((unchecked((ulong)value) & (((ulong)1 << RemainingBitsToWrite) - 1)) << (N_BITS_TO_READ_AT_A_TIME - RemainingBitsToWrite)); 

            StreamWriteBitPos += unchecked((uint)valueBits);   // Advance the current bit position pointer;
        }

        public void StreamWriteEnd()
        {
          if (StreamWriteBitPos != NumBits)
            throw new TRexException($"{nameof(StreamWriteEnd)}: Stream bit position is not after last bit in storage (StreamWriteBitPos={StreamWriteBitPos}), NumBits={NumBits})");
        }

        /// <summary>
        /// ReadBitField reads a single bit-field from the bitfield array. The bitfield is
        /// identified by the bit location and number of bits in the value to be read.
        /// Once the value has been read the value of bitLocation is set to the next bit after
        /// the end of the value just read.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// encoded to native null value conversion and deal with adding the minvalue
        /// from the descriptor in the read value.
        /// The variant that take an exact number of value bits is a raw version of the read method that
        /// just reads the value from the given bits.
        /// </summary>
        /// <param name="bitLocation"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public long ReadBitField(ref uint bitLocation, EncodedBitFieldDescriptor descriptor)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the element referenced by bitLocation
            // 2: If there are still more than N_BITS_TO_READ_AT_A_TIME remaining bits to be read, then read
            //    individual elements from Storage into Result until the remaining number of bits
            //    to be read is less than N_BITS_TO_READ_AT_A_TIME
            // 3: If there are still (less than N_BITS_TO_READ_AT_A_TIME) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next element in Storage

            int valueBits = descriptor.RequiredBits;

            if (valueBits == 0) // There's nothing to do!
                return descriptor.AllValuesAreNull ? descriptor.NativeNullValue : descriptor.MinValue;

            uint BlockPointer = bitLocation >> BIT_LOCATION_TO_BLOCK_SHIFT;
            int RemainingBitsInCurrentStorageBlock = unchecked((int)(N_BITS_TO_READ_AT_A_TIME - (bitLocation & BITS_REMAINING_IN_STORAGE_BLOCK_MASK)));
            long Result;

            // Read initial bits from storage element
            if (RemainingBitsInCurrentStorageBlock >= valueBits)
            {
                Result = unchecked((long)(Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - valueBits)) & ((1L << valueBits) - 1));
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageElement
                // Step 1: Fill remaining bits
                Result = unchecked((long)Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1));
                int BitsToRead = valueBits - RemainingBitsInCurrentStorageBlock;

                /* When using long elements, there can never be a value stored that is larger that the storage element
                // Step 2: Read whole elements
                while (BitsToRead > BitFieldArray.N_BITS_TO_READ_AT_A_TIME)
                {
                    BitsToRead -= BitFieldArray.N_BITS_TO_READ_AT_A_TIME;
                    Result = (Result << BitFieldArray.N_BITS_TO_READ_AT_A_TIME) | Storage[++BlockPointer]; // Add the next element from storage and put it in result
                }
                */

                // Step 3: Read remaining bits from next block in Storage
                if (BitsToRead > 0)
                    Result = unchecked((long)((unchecked((ulong)Result) << BitsToRead) | (Storage[BlockPointer + 1] >> (N_BITS_TO_READ_AT_A_TIME - BitsToRead))));
            }

            // Compute the true result of the read by taking nullability and the offset of MinValue into account
            Result = descriptor.Nullable && Result == descriptor.EncodedNullValue - descriptor.MinValue ? descriptor.NativeNullValue : Result + descriptor.MinValue;

            bitLocation += unchecked((uint)valueBits); // Advance the current bit position pointer;

            return Result;
        }

        /// <summary>
        /// ReadBitField reads a single bit-field from the bitfield array. The bitfield is
        /// identified by the bit location and number of bits in the value to be read.
        /// Once the value has been read the value of bitLocation is set to the next bit after
        /// the end of the value just read.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// encoded to native null value conversion and deal with adding the minvalue
        /// from the descriptor in the read value.
        /// The variant that take an exact number of value bits is a raw version of the read method that
        /// just reads the value from the given bits.
        /// </summary>
        /// <param name="bitLocation"></param>
        /// <param name="valueBits"></param>
        /// <returns></returns>
        public long ReadBitField(ref uint bitLocation, int valueBits)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the element referenced by bitLocation
            // 2: If there are still more than N_BITS_TO_READ_AT_A_TIME remaining bits to be read, then read
            //    individual elements from Storage into Result until the remaining number of bits
            //    to be read is less than N_BITS_TO_READ_AT_A_TIME
            // 3: If there are still (less than N_BITS_TO_READ_AT_A_TIME) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next element in Storage

            if (valueBits == 0) // There's nothing to do!
                return 0;

            long Result;
            uint BlockPointer = bitLocation >> BIT_LOCATION_TO_BLOCK_SHIFT;
            int RemainingBitsInCurrentStorageBlock = unchecked((int)(N_BITS_TO_READ_AT_A_TIME - (bitLocation & BITS_REMAINING_IN_STORAGE_BLOCK_MASK)));

            // Read initial bits from storage block
            if (RemainingBitsInCurrentStorageBlock >= valueBits)
            {
                Result = unchecked((long)(Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - valueBits)) & ((1 << valueBits) - 1));
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageElement
                // Step 1: Fill remaining bits
                Result = unchecked((long)Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1));
                int BitsToRead = valueBits - RemainingBitsInCurrentStorageBlock;

                /* When using long elements, there can never be a value stored that is larger that the storage element
                // Step 2: Read whole elements
                while (BitsToRead > BitFieldArray.N_BITS_TO_READ_AT_A_TIME)
                {
                    BitsToRead -= BitFieldArray.N_BITS_TO_READ_AT_A_TIME;
                    Result = (Result << BitFieldArray.N_BITS_TO_READ_AT_A_TIME) | Storage[++BlockPointer]; // Add the next element from storage and put it in result
                }
                */

                // Step 3: Read remaining bits from next block in Storage
                if (BitsToRead > 0)
                    Result = unchecked((long)((unchecked((ulong)Result) << BitsToRead) | (Storage[BlockPointer + 1] >> (N_BITS_TO_READ_AT_A_TIME - BitsToRead))));
            }

            bitLocation += unchecked((uint)valueBits); // Advance the current bit position pointer;

            return Result;
        }
    }
}
