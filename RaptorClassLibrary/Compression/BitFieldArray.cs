using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This unit implements support for storing attribute values as variable bit field arrays and records 

namespace VSS.VisionLink.Raptor.Compression
{
    public struct BitFieldArray
    {
        public const int kNBytesReadAtATime = 1;
        public const int kNBitsReadAtATime = kNBytesReadAtATime * 8;
        public const int kBitLocationToBlockShift = 3; // 1 Byte : 3, 2 Bytes : 4, 4 Bytes : 5
        public const int kBitsRemainingInStorageBlockMask = 0x7; // 1 Byte : $7, 2 bytes : $f, 4 bytes : $1f

        /// <summary>
        /// Storage is the memory allocated to storing the bit field array
        /// </summary>
        private byte[] Storage;

        private uint StreamWriteBitPos;

        // FNumBits indicates the total number of bits stored in the bit field array
        private uint FNumBits;

        private void AllocateBuffer()
        {
            if (MemorySize() == 0)
            {
                return; // No storage required (yes, this can happen, eg: PassCounts for a segment with only a single cell pass)
            }

            if (MemorySize() > (256 * 1024 * 1024))
            {
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray.AllocateBuffer limited to 256Mb in size (%d bytes requested)', [MemorySize]), slmcError);
                throw new Exception(String.Format("BitFieldArray.AllocateBuffer limited to 256Mb in size ({0}Mb requested)", MemorySize() / (1024 * 1024)));
            }

            Storage = new byte[MemorySize()];

            if (Storage == null)
            {
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray.AllocateBuffer failed to allocate a buffer of %d bytes', [MemorySize]), slmcError);
                throw new Exception(String.Format("BitFieldArray.AllocateBuffer failed to allocate a buffer of {0} bytes", MemorySize()));
            }
        }

        public uint NumBits { get { return FNumBits; } }

        /// <summary>
        /// MemorySize returns the total number of bytes occupied by the array of bitfield records
        /// </summary>
        /// <returns></returns>
        public uint MemorySize() => (NumBits & 0x7) != 0 ? NumBits >> 3 + 1 : NumBits >> 3;

        public void Initialise(int ABitsPerRecord, int ANumRecords)
        {
            FNumBits = (uint)((long)ABitsPerRecord * ANumRecords);

            AllocateBuffer();
        }
        public void Initialise(BitFieldArrayRecordsDescriptor[] RecordsArray)
        {
            FNumBits = 0;

            foreach (BitFieldArrayRecordsDescriptor descriptor in RecordsArray)
            {
                FNumBits += (uint)((long)(descriptor.NumRecords) * descriptor.BitsPerRecord);
            }

            AllocateBuffer();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((long)FNumBits);

            if (FNumBits > 0)
            {
                writer.Write((long)MemorySize());
                writer.Write(Storage);
            }
        }

        public void Read(BinaryReader reader)
        {
            Storage = null;
            FNumBits = (uint)reader.ReadInt64();

            if (FNumBits > 0)
            {
                int NumBytes = (int)reader.ReadInt64();
                Storage = reader.ReadBytes(NumBytes);
            }
        }

        // StreamWriteStart initialise the bit field array for streamed writing from the start of the allocated memory
        public void StreamWriteStart()
        {
            StreamWriteBitPos = 0;
        }

        // StreamWrite takes a native unsigned integer value (32/64) value depending on OS,
        // plus a number of bits and write the least significant <ValueBits> of that value
        // into the bit field array.
        // The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        // native to encoded null value conversion and deal with subtracting the minvalue
        // from the descriptor in the encoded value.
        // The variant that take an exct number of value bits is a raw version of the write method that
        // just writes the given value in the given bits.
        public void StreamWrite(int AValue, EncodedBitFieldDescriptor ADescriptor)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the byte referenced by FStreamWritePos
            // 2: If there are still more than 8 remaining bits to be written, then write
            //    individual bytes from AValue into FStorage until the reminining number of bits
            //    to be written is less than 8
            // 3: If there are still (less than 8) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty byte in FStorage

            int ValueBits = ADescriptor.RequiredBits;
            if (ValueBits == 0) // There's nothing to do!
            {
                return;
            }

            AValue = AValue == ADescriptor.NativeNullValue ? ADescriptor.EncodedNullValue - ADescriptor.MinValue : AValue - ADescriptor.MinValue;

            // Be paranoid! Ensure there are no bits set in the high order bits above the
            // least significant AValueBits in Value
            AValue = AValue & ((1 << ValueBits) - 1);

            int BytePointer = (int)(StreamWriteBitPos >> 3);
            int AvailBitsInCurrentStorageByte = 8 - (byte)(StreamWriteBitPos & 0x7);

            // Write initial bits into storage byte
            if (AvailBitsInCurrentStorageByte >= ValueBits)
            {
                Storage[BytePointer] = (byte)(Storage[BytePointer] | (AValue << (AvailBitsInCurrentStorageByte - ValueBits)));
                StreamWriteBitPos += (uint)ValueBits;   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageByte
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = ValueBits - AvailBitsInCurrentStorageByte;
            Storage[BytePointer] = (byte)(Storage[BytePointer] | (AValue >> RemainingBitsToWrite));

            // Step 2: Write whole bytes
            while (RemainingBitsToWrite > 8)
            {
                BytePointer++; // Move to the next byte in FStorage;
                RemainingBitsToWrite -= 8;
                Storage[BytePointer] = (byte)(AValue >> RemainingBitsToWrite); // Peel of the next byte and place it into storage
            }

            // Step 3: Write remaining bits into next byte in FStorage
            if (RemainingBitsToWrite > 0)
            {
                BytePointer++; // Move to the next byte in FStorage;
                Storage[BytePointer] = (byte)((AValue & ((1 << RemainingBitsToWrite) - 1)) << (8 - RemainingBitsToWrite)); // Mask out the bits we want...
            }

            StreamWriteBitPos += (byte)ValueBits;   // Advance the current bit position pointer;
        }

        public void StreamWrite(int AValue, int AValueBits)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the byte referenced by FStreamWritePos
            // 2: If there are still more than 8 remaining bits to be written, then write
            //    individual bytes from AValue into FStorage until the reminining number of bits
            //    to be written is less than 8
            // 3: If there are still (less than 8) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty byte in FStorage

            if (AValueBits == 0) // There's nothing to do!
            {
                return;
            }

            // Be paranoid! Ensure there are no bits set in the high order bits above the
            // least significant AValueBits in Value
            AValue = AValue & ((1 << AValueBits) - 1);

            uint BytePointer = StreamWriteBitPos >> 3; // Pointer(NativeUInt(FStorage) + NativeUInt(FStreamWriteBitPos SHR 3));
            int AvailBitsInCurrentStorageByte = (int)(8 - (StreamWriteBitPos & 0x7));

            // Write initial bits into storage byte
            if (AvailBitsInCurrentStorageByte >= AValueBits)
            {
                Storage[BytePointer] = (byte)(Storage[BytePointer] | (AValue << (AvailBitsInCurrentStorageByte - AValueBits)));
                StreamWriteBitPos += (uint)AValueBits;   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageByte
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = AValueBits - AvailBitsInCurrentStorageByte;
            Storage[BytePointer] = (byte)(Storage[BytePointer] | (AValue >> RemainingBitsToWrite));

            // Step 2: Write whole bytes
            while (RemainingBitsToWrite > 8)
            {
                BytePointer++; // Move to the next byte in FStorage;
                RemainingBitsToWrite -= 8;
                Storage[BytePointer] = (byte)(AValue >> RemainingBitsToWrite); // Peel of the next byte and place it into storage
            }

            // Step 3: Write remaining bits into next byte in FStorage
            if (RemainingBitsToWrite > 0)
            {
                BytePointer++; // Move to the next byte in FStorage;
                Storage[BytePointer] = (byte)((AValue & ((1 << RemainingBitsToWrite) - 1)) << (8 - RemainingBitsToWrite)); // Mask out the bits we want...
            }

            StreamWriteBitPos += (uint)AValueBits;   // Advance the current bit position pointer;
        }

        public void StreamWriteEnd()
        {
            Debug.Assert(StreamWriteBitPos == FNumBits, String.Format("BitFieldArray.StreamWriteEnd: Stream bit position is not after last bit in FStorage (FStreamWriteBitPos={0}, FNumBits={1})", StreamWriteBitPos, NumBits));
        }

        // ReadBitField reads a single bit-field from the bitfield array. The bitfield is
        // identified by the bit location and number of bits in the value to be read.
        // Once the value has been read the value of ABitLocation is set to the next bit after
        // the end of the value just read.
        // The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        // encoded to native null value conversion and deal with adding the minvalue
        // from the descriptor in the read value.
        // The variant that take an exct number of value bits is a raw version of the read method that
        // just reads the value from the given bits.
        public int ReadBitField(ref int ABitLocation, EncodedBitFieldDescriptor ADescriptor)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the byte referenced by ABitLocation
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be read, then read
            //    individual bytes from FStorage into Result until the reminining number of bits
            //    to be read is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next byte in FStorage

            int ValueBits = ADescriptor.RequiredBits;

            if (ValueBits == 0) // There's nothing to do!
            {
                return ADescriptor.AllValuesAreNull ? ADescriptor.NativeNullValue : ADescriptor.MinValue;
            }

#if DEBUG
            if (Storage == null)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits with no storage allocated', [ABitLocation, ValueBits]), slmcAssert);
                return 0;
            }

            if (ABitLocation + ValueBits > FNumBits)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits will read past end of data at %d', [ABitLocation, ValueBits, FNumBits]), slmcAssert);
                return 0;
            }
#endif

            int BlockPointer = ABitLocation >> BitFieldArray.kBitLocationToBlockShift;
            int RemainingBitsInCurrentStorageBlock = BitFieldArray.kNBitsReadAtATime - (ABitLocation & BitFieldArray.kBitsRemainingInStorageBlockMask);
            int Result;

            // Read initial bits from storage byte
            if (RemainingBitsInCurrentStorageBlock >= ValueBits)
            {
                Result = Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - ValueBits) & ((1 << ValueBits) - 1);
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageByte
                // Step 1: Fill remaining bits
                Result = Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1);
                int BitsToRead = ValueBits - RemainingBitsInCurrentStorageBlock;

                // Step 2: Read whole bytes
                while (BitsToRead > BitFieldArray.kNBitsReadAtATime)
                {
                    BlockPointer++; // Move to the next block in FStorage;
                    BitsToRead -= BitFieldArray.kNBitsReadAtATime;
                    Result = (Result << BitFieldArray.kNBitsReadAtATime) | Storage[BlockPointer]; // Add the next byte from storage and put it in result
                }

                // Step 3: Read remaining bits from next block in FStorage
                if (BitsToRead > 0)
                {
                    BlockPointer++;  // Move to the next block in FStorage;
                    Result = (Result << BitsToRead) | (Storage[BlockPointer] >> (BitFieldArray.kNBitsReadAtATime - BitsToRead));
                }
            }

            Result = ADescriptor.Nullable && (Result == (ADescriptor.EncodedNullValue - ADescriptor.MinValue)) ? ADescriptor.NativeNullValue : Result + ADescriptor.MinValue;

            ABitLocation += ValueBits; // Advance the current bit position pointer;

            return Result;
        }

        public int ReadBitField(ref int ABitLocation, int AValueBits)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the byte referenced by ABitLocation
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be read, then read
            //    individual bytes from FStorage into Result until the reminining number of bits
            //    to be read is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next byte in FStorage

            if (AValueBits == 0) // There's nothing to do!
            {
                return 0;
            }

#if DEBUG
            if (Storage == null)
            {
                // TODO read when logging availabl
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits with no storage allocated', [ABitLocation, AValueBits]), slmcAssert);
                return 0;
            }

            if ((ABitLocation + AValueBits) > FNumBits)
            {
                // TODO read when logging availabl
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits will read past end of data at %d', [ABitLocation, AValueBits, FNumBits]), slmcAssert);
                return 0;
            }
#endif

            int Result = 0;
            int BlockPointer = ABitLocation >> kBitLocationToBlockShift;
            int RemainingBitsInCurrentStorageBlock = BitFieldArray.kNBitsReadAtATime - (ABitLocation & BitFieldArray.kBitsRemainingInStorageBlockMask);

            // Read initial bits from storage block
            if (RemainingBitsInCurrentStorageBlock >= AValueBits)
            {
                Result = (Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - AValueBits)) & ((1 << AValueBits) - 1);
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageByte
                // Step 1: Fill remaining bits
                Result = Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1);
                int BitsToRead = AValueBits - RemainingBitsInCurrentStorageBlock;

                // Step 2: Read whole bytes
                while (BitsToRead > BitFieldArray.kNBitsReadAtATime)
                {
                    BlockPointer++; // Move to the next block in FStorage;
                    BitsToRead -= BitFieldArray.kNBitsReadAtATime;
                    Result = (Result << BitFieldArray.kNBitsReadAtATime) | Storage[BlockPointer]; // Add the next byte from storage and put it in result
                }

                // Step 3: Read remaining bits from next block in FStorage
                if (BitsToRead > 0)
                {
                    BlockPointer++;  // Move to the next block in FStorage;
                    Result = (Result << BitsToRead) | (Storage[BlockPointer] >> (BitFieldArray.kNBitsReadAtATime - BitsToRead));
                }
            }

            ABitLocation += AValueBits; // Advance the current bit position pointer;

            return Result;
        }
    }
}
