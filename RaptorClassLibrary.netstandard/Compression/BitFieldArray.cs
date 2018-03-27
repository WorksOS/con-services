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
    /// <summary>
    /// Bit field arrays implement arrays variable bit fields to mor eefficiently use memory.
    /// They are commonly used in conjunction with dynamic/entropic compression of vectors of values being
    /// stored by the arrays. Records may be implemented that represent sets of fields, where the fields for each record are
    /// contiguously stored in memory to improve access locality.
    /// </summary>
    public struct BitFieldArray
    {
        /// <summary>
        /// Read this many bytes at a time from the BitFieldArray storage when reading values into elements in the internal storage array
        /// </summary>
        private const int kNBytesReadAtATime = 8; // was 1

        /// <summary>
        /// Read this many bits at a time from the BitFieldArray storage when reading values
        /// </summary>
        private const int kNBitsReadAtATime = kNBytesReadAtATime * 8;

        /// <summary>
        /// The number of bits necessary to shift the offset address for a field in a bit field array to 
        /// convert the the bit address into logical blocks (ie: the block size at which bits are read from memory
        /// when reading or writing values)
        /// </summary>
        private const int kBitLocationToBlockShift = 6; // 1 Byte : 3, 2 Bytes : 4, 4 Bytes : 5, 8 bytes : 6

        /// <summary>
        /// The number of bits required to express the power of 2 sized block of bits, eg: a block of 8 bits needs
        /// 3 bits to express the number range 0..7m so the mask if 0x7 (0b111), etc
        /// </summary>
        private const int kBitsRemainingInStorageBlockMask = 0x3f; // 1 Byte : $7, 2 bytes : $f, 4 bytes : $1f, 8 bytes : $3f

        /// <summary>
        /// Storage is the memory allocated to storing the bit field array.
        /// </summary>
        private ulong[] Storage;

        /// <summary>
        /// The current bit address position in a bit field array during a stream write operation into it.
        /// </summary>
        private uint StreamWriteBitPos;

        // FNumBits indicates the total number of bits stored in the bit field array

        /// <summary>
        /// Allocates a block of memory large enough to store the number of bits required (See FNumBits * MemorySize())
        /// </summary>
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

            Storage = NewStorage();

            if (Storage == null)
            {
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray.AllocateBuffer failed to allocate a buffer of %d bytes', [MemorySize]), slmcError);
                throw new Exception(String.Format("BitFieldArray.AllocateBuffer failed to allocate a buffer of {0} bytes", MemorySize()));
            }
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
        public uint NumStorageElements() => MemorySize() % kNBytesReadAtATime == 0 ? MemorySize() / kNBytesReadAtATime : (MemorySize() / kNBytesReadAtATime) + 1; // FNumBits % kNBitsReadAtATime == 0 ? FNumBits / kNBitsReadAtATime : FNumBits / kNBitsReadAtATime + 1;

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
        /// <param name="RecordsArray"></param>
        public void Initialise(BitFieldArrayRecordsDescriptor[] RecordsArray)
        {
            NumBits = 0;

            foreach (BitFieldArrayRecordsDescriptor descriptor in RecordsArray)
            {
                NumBits += (uint)((long)(descriptor.NumRecords) * descriptor.BitsPerRecord);
            }

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
            {
                return;
            }

            byte[] buffer = new byte[Storage.Length * sizeof(ulong)];
            Buffer.BlockCopy(Storage, 0, buffer, 0, Storage.Length * sizeof(ulong));
            writer.Write(buffer);

            /*
            foreach (ulong item in Storage)
            {
                writer.Write(item);
            }
            */
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
            {
                return;
            }

            Storage = NewStorage();

            byte[] buffer = new byte[Storage.Length * sizeof(ulong)];
            reader.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, Storage, 0, buffer.Length);

            /*
            for (int i = 0; i < NumStorageElements(); i++)
            {
                Storage[i] = reader.ReadUInt64();
            }
            */
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
        /// plus a number of bits and writes the least significant <ValueBits> of that value
        /// into the bit field array.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// native to encoded null value conversion and deal with subtracting the minvalue
        /// from the descriptor in the encoded value.
        /// The variant that take an exact number of value bits is a raw version of the write method that
        /// just writes the given value in the given bits.
        /// </summary>
        /// <param name="AValue"></param>
        /// <param name="ADescriptor"></param>
        public void StreamWrite(long AValue, EncodedBitFieldDescriptor ADescriptor)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the element referenced by FStreamWritePos
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be written, then write
            //    individual elements from AValue into Storage until the reminining number of bits
            //    to be written is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty element in Storage

            int ValueBits = ADescriptor.RequiredBits;

            if (ValueBits == 0) // There's nothing to do!
            {
                return;
            }

            AValue = AValue == ADescriptor.NativeNullValue ? ADescriptor.EncodedNullValue - ADescriptor.MinValue : AValue - ADescriptor.MinValue;

            // Be paranoid! Ensure there are no bits set in the high order bits above the least significant AValueBits in Value
            AValue = AValue & ((1 << ValueBits) - 1);

            int StoragePointer = (int)(StreamWriteBitPos >> kBitLocationToBlockShift);
            int AvailBitsInCurrentStorageElement = kNBitsReadAtATime - (byte)(StreamWriteBitPos & kBitsRemainingInStorageBlockMask);

            // Write initial bits into storage element
            if (AvailBitsInCurrentStorageElement >= ValueBits)
            {
                Storage[StoragePointer] = Storage[StoragePointer] | ((ulong)AValue << (AvailBitsInCurrentStorageElement - ValueBits));
                StreamWriteBitPos += (uint)ValueBits;   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageElement
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = ValueBits - AvailBitsInCurrentStorageElement;
            Storage[StoragePointer] = (Storage[StoragePointer] | ((ulong)AValue >> RemainingBitsToWrite));

            /* When using long elements, there can never be a value stored that is larger that the storage element
            // Step 2: Write whole elements
            while (RemainingBitsToWrite > kNBitsReadAtATime)
            {
                RemainingBitsToWrite -= kNBitsReadAtATime;
                Storage[++StoragePointer] = (byte)(AValue >> RemainingBitsToWrite); // Peel of the next element and place it into storage
            }
            */

            // Step 3: Write remaining bits into next element in Storage
            if (RemainingBitsToWrite > 0)
            {
                Storage[StoragePointer + 1] = ((ulong)AValue & (((ulong)1 << RemainingBitsToWrite) - 1)) << (kNBitsReadAtATime - RemainingBitsToWrite); // Mask out the bits we want...
            }

            StreamWriteBitPos += (uint)ValueBits;   // Advance the current bit position pointer;
        }

        /// <summary>
        /// StreamWrite takes a native unsigned integer value value depending on OS,
        /// plus a number of bits and writes the least significant <ValueBits> of that value
        /// into the bit field array.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// native to encoded null value conversion and deal with subtracting the minvalue
        /// from the descriptor in the encoded value.
        /// The variant that take an exact number of value bits is a raw version of the write method that
        /// just writes the given value in the given bits.
        /// </summary>
        /// <param name="AValue"></param>
        /// <param name="AValueBits"></param>
        public void StreamWrite(long AValue, int AValueBits)
        {
            // Writing occurs in three stages:
            // 1: Fill the remaining unwritten bits in the element referenced by FStreamWritePos
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be written, then write
            //    individual elements from AValue into Storage until the reminining number of bits
            //    to be written is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be written, then write the remainder
            //    of the bits into the most significant bits of the next empty element in Storage

            if (AValueBits == 0) // There's nothing to do!
            {
                return;
            }

            // Be paranoid! Ensure there are no bits set in the high order bits above the
            // least significant AValueBits in Value
            AValue = AValue & ((1 << AValueBits) - 1);

            uint StoragePointer = StreamWriteBitPos >> kBitLocationToBlockShift;
            int AvailBitsInCurrentStorageElement = (int)(kNBitsReadAtATime - (StreamWriteBitPos & kBitsRemainingInStorageBlockMask));

            // Write initial bits into storage element
            if (AvailBitsInCurrentStorageElement >= AValueBits)
            {
                Storage[StoragePointer] = (Storage[StoragePointer] | ((ulong)AValue << (AvailBitsInCurrentStorageElement - AValueBits)));
                StreamWriteBitPos += (uint)AValueBits;   // Advance the current bit position pointer;
                return;
            }

            // There are more bits than can fit in AvailBitsInCurrentStorageElement
            // Step 1: Fill remaining bits
            int RemainingBitsToWrite = AValueBits - AvailBitsInCurrentStorageElement;
            Storage[StoragePointer] = (Storage[StoragePointer] | ((ulong)AValue >> RemainingBitsToWrite));

            /* When using long elements, there can never be a value stored that is larger that the storage element
            // Step 2: Write whole elements
            while (RemainingBitsToWrite > kNBitsReadAtATime)
            {
                RemainingBitsToWrite -= kNBitsReadAtATime;
                Storage[++StoragePointer] = (byte)(AValue >> RemainingBitsToWrite); // Peel of the next element and place it into storage
            }
            */

            // Step 3: Write remaining bits into next element in Storage
            if (RemainingBitsToWrite > 0)
            {
                Storage[StoragePointer + 1] = (((ulong)AValue & (((ulong)1 << RemainingBitsToWrite) - 1)) << (kNBitsReadAtATime - RemainingBitsToWrite)); // Mask out the bits we want...
            }

            StreamWriteBitPos += (uint)AValueBits;   // Advance the current bit position pointer;
        }

        public void StreamWriteEnd()
        {
            Debug.Assert(StreamWriteBitPos == NumBits, String.Format("BitFieldArray.StreamWriteEnd: Stream bit position is not after last bit in Storage (FStreamWriteBitPos={0}, FNumBits={1})", StreamWriteBitPos, NumBits));
        }

        /// <summary>
        /// ReadBitField reads a single bit-field from the bitfield array. The bitfield is
        /// identified by the bit location and number of bits in the value to be read.
        /// Once the value has been read the value of ABitLocation is set to the next bit after
        /// the end of the value just read.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// encoded to native null value conversion and deal with adding the minvalue
        /// from the descriptor in the read value.
        /// The variant that take an exct number of value bits is a raw version of the read method that
        /// just reads the value from the given bits.
        /// </summary>
        /// <param name="ABitLocation"></param>
        /// <param name="ADescriptor"></param>
        /// <returns></returns>
        public long ReadBitField(ref int ABitLocation, EncodedBitFieldDescriptor ADescriptor)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the element referenced by ABitLocation
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be read, then read
            //    individual elements from Storage into Result until the reminining number of bits
            //    to be read is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next element in Storage

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

            if (ABitLocation + ValueBits > NumBits)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits will read past end of data at %d', [ABitLocation, ValueBits, FNumBits]), slmcAssert);
                return 0;
            }
#endif

            int BlockPointer = ABitLocation >> BitFieldArray.kBitLocationToBlockShift;
            int RemainingBitsInCurrentStorageBlock = BitFieldArray.kNBitsReadAtATime - (ABitLocation & BitFieldArray.kBitsRemainingInStorageBlockMask);
            long Result;

            // Read initial bits from storage element
            if (RemainingBitsInCurrentStorageBlock >= ValueBits)
            {
                Result = (long)((Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - ValueBits)) & (ulong)(((ulong)1 << ValueBits) - 1));
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageElement
                // Step 1: Fill remaining bits
                Result = (long)Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1);
                int BitsToRead = ValueBits - RemainingBitsInCurrentStorageBlock;

                /* When using long elements, there can never be a value stored that is larger that the storage element
                // Step 2: Read whole elements
                while (BitsToRead > BitFieldArray.kNBitsReadAtATime)
                {
                    BitsToRead -= BitFieldArray.kNBitsReadAtATime;
                    Result = (Result << BitFieldArray.kNBitsReadAtATime) | Storage[++BlockPointer]; // Add the next element from storage and put it in result
                }
                */

                // Step 3: Read remaining bits from next block in Storage
                if (BitsToRead > 0)
                {
                    Result = (long)(((ulong)Result << BitsToRead) | (Storage[BlockPointer + 1] >> (BitFieldArray.kNBitsReadAtATime - BitsToRead)));
                }
            }

            // Compute the true result of the read by taking nullability and the offset of MinValue into account
            Result = ADescriptor.Nullable && (Result == (ADescriptor.EncodedNullValue - ADescriptor.MinValue)) ? ADescriptor.NativeNullValue : Result + ADescriptor.MinValue;

            ABitLocation += ValueBits; // Advance the current bit position pointer;

            return Result;
        }

        /// <summary>
        /// ReadBitField reads a single bit-field from the bitfield array. The bitfield is
        /// identified by the bit location and number of bits in the value to be read.
        /// Once the value has been read the value of ABitLocation is set to the next bit after
        /// the end of the value just read.
        /// The variant that takes a TEncodedBitFieldDescriptor will perform automatic
        /// encoded to native null value conversion and deal with adding the minvalue
        /// from the descriptor in the read value.
        /// The variant that take an exct number of value bits is a raw version of the read method that
        /// just reads the value from the given bits.
        /// </summary>
        /// <param name="ABitLocation"></param>
        /// <param name="AValueBits"></param>
        /// <returns></returns>
        public long ReadBitField(ref int ABitLocation, int AValueBits)
        {
            // Reading occurs in three stages:
            // 1: Read the remaining bits in the element referenced by ABitLocation
            // 2: If there are still more than kNBitsReadAtATime remaining bits to be read, then read
            //    individual elements from Storage into Result until the reminining number of bits
            //    to be read is less than kNBitsReadAtATime
            // 3: If there are still (less than kNBitsReadAtATime) bits to be read, then read the remainder
            //    of the bits from the most significant bits of the next element in Storage

            if (AValueBits == 0) // There's nothing to do!
            {
                return 0;
            }

#if DEBUG
            if (Storage == null)
            {
                // TODO read when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits with no storage allocated', [ABitLocation, AValueBits]), slmcAssert);
                return 0;
            }

            if ((ABitLocation + AValueBits) > NumBits)
            {
                // TODO read when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('BitFieldArray: Read request at %d of %d bits will read past end of data at %d', [ABitLocation, AValueBits, FNumBits]), slmcAssert);
                return 0;
            }
#endif

            long Result = 0;
            int BlockPointer = ABitLocation >> kBitLocationToBlockShift;
            int RemainingBitsInCurrentStorageBlock = BitFieldArray.kNBitsReadAtATime - (ABitLocation & BitFieldArray.kBitsRemainingInStorageBlockMask);

            // Read initial bits from storage block
            if (RemainingBitsInCurrentStorageBlock >= AValueBits)
            {
                Result = (long)(Storage[BlockPointer] >> (RemainingBitsInCurrentStorageBlock - AValueBits)) & ((1 << AValueBits) - 1);
            }
            else
            {
                // There are more bits than can fit in RemainingBitsInCurrentStorageElement
                // Step 1: Fill remaining bits
                Result = (long)Storage[BlockPointer] & ((1 << RemainingBitsInCurrentStorageBlock) - 1);
                int BitsToRead = AValueBits - RemainingBitsInCurrentStorageBlock;

                /* When using long elements, there can never be a value stored that is larger that the storage element
                // Step 2: Read whole elements
                while (BitsToRead > BitFieldArray.kNBitsReadAtATime)
                {
                    BitsToRead -= BitFieldArray.kNBitsReadAtATime;
                    Result = (Result << BitFieldArray.kNBitsReadAtATime) | Storage[++BlockPointer]; // Add the next element from storage and put it in result
                }
                */

                // Step 3: Read remaining bits from next block in Storage
                if (BitsToRead > 0)
                {
                    Result = (long)(((ulong)Result << BitsToRead) | (Storage[BlockPointer + 1] >> (BitFieldArray.kNBitsReadAtATime - BitsToRead)));
                }
            }

            ABitLocation += AValueBits; // Advance the current bit position pointer;

            return Result;
        }
    }
}
