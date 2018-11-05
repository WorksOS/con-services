using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Helpers;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Represents a subgrid in terms of one bit per cell in the subgrid. Many bit-wise operations are supported
    /// allowing efficient manipulation of bit masks representing subgrids and trees.
    /// </summary>
    public class SubGridTreeBitmapSubGridBits : IEquatable<SubGridTreeBitmapSubGridBits>
    {
        /// <summary>
        /// The code used in serialized bit masks to indicate all bits are unset (0), and so are not explicitly written
        /// </summary>
        private const byte Serialisation_NoBitsSet = 0;

        /// <summary>
        /// The code used in serialized bit masks to indicate all bits are set (1), and so are not explicitly written
        /// </summary>
        private const byte Serialisation_AllBitsSet = 1;

        /// <summary>
        /// The code used in serialized bit masks to indicate the number of set bits is in the range 
        /// 1..SubGridTreeConsts.CellsPerSubgrid and so are explicitly written
        /// </summary>
        private const byte Serialisation_ArbitraryBitsSet = 2;

        /// <summary>
        /// Represents the individual bit in the bits representing left most cell in a row for the TSubGridBitMap leaf cell.
        /// </summary>
        private const uint SubGridBitMapHighBitMask = (uint)1 << (SubGridTreeConsts.SubGridTreeDimensionMinus1);

        /// <summary>
        /// The number obtained when summed values of bit rows when all bits in each bit row are set
        /// </summary>
        public const long SumBitRowsFullCount = (((long)1 << SubGridTreeConsts.SubGridTreeDimension) - 1) * SubGridTreeConsts.SubGridTreeDimension;

        /// <summary>
        /// The array that stores the memory for the individual bit flags (of which there are 32x32 = 1024)
        /// </summary>
        public uint[] Bits;

        /// <summary>
        /// The number of byte occupied by the Bits array
        /// </summary>
        private const int BytesOccupiedByBits = SubGridTreeConsts.SubGridTreeDimension * sizeof(uint);

        /// <summary>
        /// Default indexer for bit values in the mask
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool this[byte x, byte y]
        {
            get { return BitSet(x, y); }
            set { SetBitValue(x, y, value); }
        }

        /// <summary>
        /// Initialise the internal state of a new structure. This must be called prior to use of the instance.
        /// If filled is true then all bits in the bits array will be set to '1'
        /// </summary>
        /// <param name="options"></param>
        public SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions options)
        {
            Bits = new uint[SubgridBitsHelper.SubGridTreeLeafBitmapSubGridBits_Clear.Length];

            if (options == SubGridBitsCreationOptions.Filled)
            {
                Fill();
                return;
            }

            if (options == SubGridBitsCreationOptions.Unfilled)
            {
                // Note: The default .Net behaviour of clearing the memory for the bit mask achieves the same result as Clear...
                // Clear();
                return;
            }

          Debug.Assert(false, "Unknown SubGridTreeBitmapSubGridBits creation option");
        }

        /// <summary>
        /// Initialise the internal state of a new structure based on an other bitmask.
        /// </summary>
        public SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits source)
        {
          Bits = new uint[SubgridBitsHelper.SubGridTreeLeafBitmapSubGridBits_Clear.Length];

          Assign(source);
        }

        /// <summary>
        /// Set all the bits in subgrid bits structure to 0.
        /// </summary>
        public void Clear()
        {
            // Perform a fast block copy of the bytes in the pre-calculated empty Bits 
            // array in BitsHelper.SubGridTreeLeafBitmapSubGridBits_Clear array into the local Bits array
            // Note: The copy is in terms of bytes, not elements. 
            // This is about as fast as a managed copy of array items can be.
            Buffer.BlockCopy(SubgridBitsHelper.SubGridTreeLeafBitmapSubGridBits_Clear, 0, Bits, 0, SubgridBitsHelper.BytesInBitsArray);
            //Array.Clear(Bits, 0, Bits.Length);
        }

        /// <summary>
        /// Set all the bits in subgrid bits structure to 1.
        /// </summary>
        public void Fill()
        {
            // Perform a fast block copy of the bytes in the pre-calculated empty Bits 
            // array in BitsHelper.SubGridTreeLeafBitmapSubGridBits_Clear array into the local Bits array
            // Note: The copy is in terms of bytes, not elements. 
            // This is about as fast as a managed copy of array items can be.
            Buffer.BlockCopy(SubgridBitsHelper.SubGridTreeLeafBitmapSubGridBits_Fill, 0, Bits, 0, SubgridBitsHelper.BytesInBitsArray);
        }

        /// <summary>
        /// Assigns the contents of one SubGridTreeBitmapSubGridBits to another
        /// </summary>
        /// <param name="source"></param>
        public void Assign(SubGridTreeBitmapSubGridBits source)
        {
            Buffer.BlockCopy(source.Bits, 0, Bits, 0, SubgridBitsHelper.BytesInBitsArray);
        }

        /// <summary>
        /// Determine if the bit at location (CellX, CellY) in the bit array is set (1)
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool BitSet(int CellX, int CellY) => (Bits[CellY] & (SubGridBitMapHighBitMask >> CellX)) != 0;

        /// <summary>
        /// Determine if the bit at location (CellX, CellY) in the bit array is set (1)
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool BitSet(uint CellX, uint CellY) => (Bits[CellY] & (SubGridBitMapHighBitMask >> (int)CellX)) != 0;

      /// <summary>
      /// Defines an overloaded bitwise equality operator for the Bits from a and b
      /// </summary>
      /// <param name="a"></param>
      /// <param name="b"></param>
      /// <returns></returns>
      public static bool operator ==(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
      {
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(a, null)) return false;
        if (ReferenceEquals(null, b)) return false;

        return a.Equals(b);
      }

      /// <summary>
      /// Defines an overloaded bitwise inequality operator for the Bits from a and b
      /// </summary>
      /// <param name="a"></param>
      /// <param name="b"></param>
      /// <returns></returns>
      public static bool operator !=(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
      {
        if (ReferenceEquals(a, b)) return false;
        if (ReferenceEquals(a, null)) return true;
        if (ReferenceEquals(null, b)) return true;
    
        return !a.Equals(b);
      }

        /// <summary>
        /// Defines an overloaded bitwise AND/& operator that ANDs together the Bits from a and b and returns a 
        /// new SubGridTreeLeafBitmapSubGridBits instance with the result
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SubGridTreeBitmapSubGridBits operator &(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            SubGridTreeBitmapSubGridBits result = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result.Bits[i] = a.Bits[i] & b.Bits[i];

            return result;
        }

        /// <summary>
        /// Performs the same logical operation as the '&' operator, but performs the AND inline on the called instance
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void AndWith(SubGridTreeBitmapSubGridBits other)
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                Bits[i] &= other.Bits[i];
        }

        /// <summary>
        /// Compute the AND of two bit masks and assign the result to this bitmask. This is faster than the & operator
        /// in that no intermediate bitmask instance is created and then assigned to the target bitmask
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void SetAndOf(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                Bits[i] = a.Bits[i] & b.Bits[i];
        }

        /// <summary>
        /// Defines an overloaded bitwise OR/| operator that ORs together the Bits from a and b and returns a 
        /// new SubGridTreeLeafBitmapSubGridBits instance with the result
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SubGridTreeBitmapSubGridBits operator |(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            SubGridTreeBitmapSubGridBits result = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result.Bits[i] = a.Bits[i] | b.Bits[i];

            return result;
        }

        /// <summary>
        /// Performs the same logical operation as the '|' operator, but performs the OR inline on the called instance
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void OrWith(SubGridTreeBitmapSubGridBits other)
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                Bits[i] |= other.Bits[i];
        }

        /// <summary>
        /// Compute the OR of two bit masks and assign the result to this bitmask. This is faster than the | operator
        /// in that no intermediate bitmask instance is created and then assigned to the target bitmask
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void SetOrOf(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                Bits[i] = a.Bits[i] | b.Bits[i];
        }

        /// <summary>
        /// Defines an overloaded bitwise XOR/^ operator that XORs together the Bits from a and b and returns a 
        /// new SubGridTreeLeafBitmapSubGridBits instance with the result
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SubGridTreeBitmapSubGridBits operator ^(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            SubGridTreeBitmapSubGridBits result = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result.Bits[i] = a.Bits[i] ^ b.Bits[i];

            return result;
        }

        /// <summary>
        /// Performs the same logical operation as the '^' operator, but performs the XOR inline on the called instance
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void XorWith(SubGridTreeBitmapSubGridBits other)
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                Bits[i] ^= other.Bits[i];
        }

        /// <summary>
        /// Subtracts all the set bits in b from the set bits in a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SubGridTreeBitmapSubGridBits operator -(SubGridTreeBitmapSubGridBits a, SubGridTreeBitmapSubGridBits b)
        {
            SubGridTreeBitmapSubGridBits result = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result.Bits[i] = a.Bits[i] ^ (a.Bits[i] & b.Bits[i]);

            return result;
        }

        /// <summary>
        /// Defines an overloaded bitwise NOT/~ operator that NOTs the Bits from a and returns a 
        /// new SubGridTreeLeafBitmapSubGridBits instance with the result
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static SubGridTreeBitmapSubGridBits operator ~(SubGridTreeBitmapSubGridBits bits)
        {
            SubGridTreeBitmapSubGridBits result = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result.Bits[i] = ~bits.Bits[i];

            return result;
        }

        /// <summary>
        /// Return a a 'full mask' SubGridTreeLeafBitmapSubGridBits instance with all bits set to on (1)
        /// </summary>
        public static SubGridTreeBitmapSubGridBits FullMask => new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);

        /// <summary>
        /// Clear the bit at the location given by CellX, CellY in the bits array
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void ClearBit(int CellX, int CellY) => Bits[CellY] &= ~(SubGridBitMapHighBitMask >> CellX);

        /// <summary>
        /// Count the number of bits in the bit mask that are set to on (1)
        /// </summary>
        /// <returns></returns>
        public uint CountBits()
        {
            uint result = 0;

            for (int i = 0; i < SubgridBitsHelper.BitsArrayLength; i++)
                result += BitCounterHelper.CountSetBits(Bits[i]);

            return result;
        }

        /// <summary>
        /// Compute the integer cell address extent that covers the set bits in the bitmask.
        /// The result is a bounding rectangle in the space of (0..SubGridTreeDimension - 1, 0..SubGridTreeDimension - 1)
        /// </summary>
        /// <returns></returns>
        public BoundingIntegerExtent2D ComputeCellsExtents()
        {
            BoundingIntegerExtent2D result = new BoundingIntegerExtent2D();
            result.SetInverted();

            for (int Y = 0; Y < SubGridTreeConsts.SubGridTreeDimension; Y++)
                if (Bits[Y] != 0)
                    for (int X = 0; X < SubGridTreeConsts.SubGridTreeDimension; X++)
                        if (BitSet(X, Y))
                            result.Include(X, Y);

            return result;
        }

        /// <summary>
        /// Determine if all of the bits in the bitmask are not set (0)
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                if (Bits[i] != 0)
                    return false;

            return true;
        }

        /// <summary>
        /// Determine if all of the bits in the bitmask are set (1)
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                if (Bits[i] != 0xFFFFFFFF)
                    return false;

            return true;
        }

        /// <summary>
        /// Adds all the rows containing bits together as if they were numbers. This provides fast determination of empty, full or partial bitsets
        /// </summary>
        /// <returns></returns>
        public long SumBitRows()
        {
            long result = 0;

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                result += Bits[i];

            return result;
        }

        /*
          // Not implemented in Next Gen. Any similar requirement can extract the bits and set them externally
                class function TSubGridTreeLeafBitmapSubGridBits.FromVariant(const Vrnt: Variant): TSubGridTreeLeafBitmapSubGridBits;
                var
                  Packager: TSubGridTreeLeafBitmapSubGridBitsPackager;
                begin
                  Packager := TSubGridTreeLeafBitmapSubGridBitsPackager.Create;

                  try
                    Packager.FromVariant(Vrnt);
                    Result := Packager.Bits;
                  finally
                    Packager.Free;
                  end;
                end;
                */

        /*
       // Not implemented in Next Gen. Any similar requirement can extract the bits and set them externally
       function TSubGridTreeLeafBitmapSubGridBits.ToVariant: Variant;
       var
         Packager: TSubGridTreeLeafBitmapSubGridBitsPackager;
       begin
         Packager := TSubGridTreeLeafBitmapSubGridBitsPackager.Create;

         try
           Packager.Bits := Self;
           Result := Packager.ToVariant;
         finally
           Packager.Free;
         end;
       end;
        */

        /// <summary>
        /// Set the bit at the location identified by [CellX, CellY] to set (1)
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void SetBit(int CellX, int CellY) => Bits[CellY] |= SubGridBitMapHighBitMask >> CellX;

        /// <summary>
        /// Set the bit at the location identified by [CellX, CellY] to set (1)
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void SetBit(uint CellX, uint CellY) => Bits[CellY] |= SubGridBitMapHighBitMask >> (int)CellX;

        /// <summary>
        /// Set the bit at the location identified by [CellX, CellY] to unset (0) or set (1) based on the value parameter
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="Value"></param>
        public void SetBitValue(int CellX, int CellY, bool Value)
        {
            if (Value)
                Bits[CellY] |= SubGridBitMapHighBitMask >> CellX;
            else
                Bits[CellY] &= ~(SubGridBitMapHighBitMask >> CellX);
        }

        /// <summary>
        /// Set the bit at the location identified by [CellX, CellY] to unset (0) or set (1) based on the value parameter
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="Value"></param>
        public void SetBitValue(uint CellX, uint CellY, bool Value)
        {
            if (Value)
                Bits[CellY] |= SubGridBitMapHighBitMask >> (int)CellX;
            else
                Bits[CellY] &= ~(SubGridBitMapHighBitMask >> (int)CellX);
        }

        /// <summary>
        /// Writes the contents of the mask in a binary form using the given binary write
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public void Write(BinaryWriter writer, byte [] buffer)
        {
            switch (SumBitRows())
            {
                case 0:
                    writer.Write(Serialisation_NoBitsSet);
                    break;
                case SumBitRowsFullCount:
                    writer.Write(Serialisation_AllBitsSet);
                    break;
                default:
                    writer.Write(Serialisation_ArbitraryBitsSet);

                    Buffer.BlockCopy(Bits, 0, buffer, 0, BytesOccupiedByBits);
                    writer.Write(buffer, 0, BytesOccupiedByBits);
                    break;
            }
        }

        /// <summary>
        /// Reads the contents of the mask from a binary form using the given binary reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        public void Read(BinaryReader reader, byte[] buffer)
        {
            switch (reader.ReadByte())
            {
                case Serialisation_NoBitsSet:
                    Clear();
                    break;
                case Serialisation_AllBitsSet:
                    Fill();
                    break;
                case Serialisation_ArbitraryBitsSet:
                    reader.Read(buffer, 0, BytesOccupiedByBits);
                    Buffer.BlockCopy(buffer, 0, Bits, 0, BytesOccupiedByBits);
                    /*
                    for (int i = 0; i < Bits.Length; i++)
                    {
                        Bits[i] = reader.ReadUInt32();
                    }
                    */
                    break;
                default:
                    Debug.Assert(false, "Unknown TSubGridTreeLeafBitmapSubGridBits control byte in read stream");
                    break;
            }
        }

        /// <summary>
        /// Iterate over all the bits in the bit map that are set (1) and execute the given action/lambda expression
        /// with the CellX, CellY (as integer indices) location of the set bit
        /// </summary>
        /// <param name="functor"></param>
        public void ForEachSetBit(Action<int, int> functor)
        {
            for (int Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
            {
                uint RowBits = Bits[Row];

                if (RowBits == 0)
                    continue;

                for (int Column = 0; Column < SubGridTreeConsts.SubGridTreeDimension; Column++)
                {
                    if ((RowBits & (SubGridBitMapHighBitMask >> Column)) != 0)
                        functor(Column, Row);
                }
            }
        }

        /// <summary>
        /// Iterate over all the bits in the bit map that are set (1) and execute the given action/lambda expression
        /// with the CellX, CellY (as integer indices) location of the set bit. The functor returns true/false to indicate
        /// if scanning of bits should continue
        /// </summary>
        /// <param name="functor"></param>
        public void ForEachSetBit(Func<int, int, bool> functor)
        {
            for (int Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
            {
                uint RowBits = Bits[Row];

                if (RowBits == 0)
                    continue;

                for (int Column = 0; Column < SubGridTreeConsts.SubGridTreeDimension; Column++)
                {
                    if ((RowBits & (SubGridBitMapHighBitMask >> Column)) != 0)
                        if (!functor(Column, Row))
                            return;
                }
            }
        }

        /// <summary>
        /// Iterate over all the bits in the bit map that are not set (0) and execute the given action/lambda expression
        /// with the CellX, CellY location of the not set bit
        /// </summary>
        /// <param name="functor"></param>
        public void ForEachClearBit(Action<int, int> functor)
        {
            for (byte Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
            {
                uint RowBits = Bits[Row];

                if (RowBits == 0xFFFFFFFF)
                    continue;

                for (byte Column = 0; Column < SubGridTreeConsts.SubGridTreeDimension; Column++)
                {
                    if ((RowBits & (SubGridBitMapHighBitMask >> Column)) == 0)
                        functor(Column, Row);
                }
            }
        }

        /// <summary>
        /// Iterate over all the bits in the bit map ) and execute the given action/lambda expression
        /// with the CellX, CellY location of the bit
        /// </summary>
        /// <param name="functor"></param>
        public void ForEach(Action<byte, byte> functor)
        {
            for (byte Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
            {
                for (byte Column = 0; Column < SubGridTreeConsts.SubGridTreeDimension; Column++)
                    functor(Column, Row);
            }
        }

        /// <summary>
        /// Iterate over every bit in the bit mask calling the given function/lambda expression on it.
        /// If the result of functor is true, then set the corresponding bit to set (1), else set it to not set (0)
        /// </summary>
        /// <param name="functor"></param>
        public void ForEach(Func<byte, byte, bool> functor)
        {
            for (byte Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
            {
                uint RowBits = Bits[Row];

                for (byte Column = 0; Column < SubGridTreeConsts.SubGridTreeDimension; Column++)
                {
                    if (functor(Column, Row))
                        RowBits |= (SubGridBitMapHighBitMask >> Column);
                    else
                        RowBits &= ~(SubGridBitMapHighBitMask >> Column);
                }

                Bits[Row] = RowBits;
            }
        }

        /// <summary>
        /// Convert a row of bits to a printable string that emits bits as a line of zero's and one's separated by spaces
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        public string RowToString(int Row)
        {
            // Initialise a string builder with appropriate number of spaces
            StringBuilder sb = new StringBuilder(new string(' ', 2 * SubGridTreeConsts.SubGridTreeDimension));

            // Set each alternate space in the string with a 0 or 1 for each bit
            uint RowBits = Bits[Row];
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                sb[2 * i + 1] = ((RowBits & (SubGridBitMapHighBitMask >> i)) != 0) ? '1' : '0';

            return sb.ToString();
        }

        /*
       Procedure TSubGridTreeLeafBitmapSubGridBits.DumpToLog(const Name : String);
       var
         Row : Integer;
       begin
         SIGLogMessage.PublishNoODS(Nil, 'Bit Mask: ' + Name, slmcDebug);

         if IsEmpty then
           SIGLogMessage.PublishNoODS(Nil, '<Empty>', slmcDebug)
         else
           if IsFull then
             SIGLogMessage.PublishNoODS(Nil, '<Full>', slmcDebug)
           else
             for Row := 0 to kSubGridTreeDimension - 1 do
               SIGLogMessage.PublishNoODS(Nil, Format('%2d: %s', [Row, RowToString(Row)]), slmcDebug);
       end;
       */

        /// <summary>
        /// Determines if this bitmask is equal to another bitmask
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SubGridTreeBitmapSubGridBits other)
        {
            if (other.Bits == null)
                return false;
         
            for (int i = 0; i < Bits.Length; i++)
                if (Bits[i] != other.Bits[i])
                    return false;
         
            return true;
        }
      
        /// <summary>
        /// Default override for the GetHashCode method. This returns the base GetHashCode() result.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Overloaded Equals() accepting generic object type parameter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals((SubGridTreeBitmapSubGridBits)obj);
        }

      /// <summary>
      /// Return an indicative size for memory consumption of this class to be used in cache tracking
      /// </summary>
      /// <returns></returns>
      public int IndicativeSizeInBytes()
      {
        return Bits?.Length * 4 ?? 0; // Array of 32 bit uint values
      }
    }
}
