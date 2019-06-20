using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Cells.Extensions;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Compression;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Helpers;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Represents an efficiently compressed version of the cell pass information included within a segment.
    /// The compression achieves approximately 10:1 reduction in memory use while preserving random access with
    /// good performance.
    /// </summary>
    public class SubGridCellSegmentPassesDataWrapper_StaticCompressed : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridCellSegmentPassesDataWrapper_StaticCompressed>();

        /// <summary>
        /// The set of field descriptors for the attribute being stored in the bit field array compressed form
        /// </summary>
        private struct EncodedFieldDescriptorsStruct
        {
            public EncodedBitFieldDescriptor MachineIDIndex;
            public EncodedBitFieldDescriptor Time;
            public EncodedBitFieldDescriptor Height;
            public EncodedBitFieldDescriptor CCV;
            public EncodedBitFieldDescriptor RMV;          
            public EncodedBitFieldDescriptor MDP;
            public EncodedBitFieldDescriptor MaterialTemperature;
            public EncodedBitFieldDescriptor MachineSpeed;
            public EncodedBitFieldDescriptor RadioLatency;
            public EncodedBitFieldDescriptor GPSModeStore;
            public EncodedBitFieldDescriptor Frequency;
            public EncodedBitFieldDescriptor Amplitude;
            public EncodedBitFieldDescriptor CCA;

            /// <summary>
            /// Initialise all descriptors
            /// </summary>
            public void Init()
            {
                MachineIDIndex.Init();
                Time.Init();
                Height.Init();
                CCV.Init();
                RMV.Init();
                MDP.Init();
                MaterialTemperature.Init();
                MachineSpeed.Init();
                RadioLatency.Init();
                GPSModeStore.Init();
                Frequency.Init();
                Amplitude.Init();
                CCA.Init();
            }

            /// <summary>
            /// Serialise all descriptors to the supplied writer
            /// </summary>
            /// <param name="writer"></param>
            public void Write(BinaryWriter writer)
            {
                MachineIDIndex.Write(writer);
                Time.Write(writer);
                Height.Write(writer);
                CCV.Write(writer);
                RMV.Write(writer);
                MDP.Write(writer);
                MaterialTemperature.Write(writer);
                MachineSpeed.Write(writer);
                RadioLatency.Write(writer);
                GPSModeStore.Write(writer);
                Frequency.Write(writer);
                Amplitude.Write(writer);
                CCA.Write(writer);
            }

            /// <summary>
            /// Deserialize all descriptors using the supplied reader
            /// </summary>
            /// <param name="reader"></param>
            public void Read(BinaryReader reader)
            {
                MachineIDIndex.Read(reader);
                Time.Read(reader);
                Height.Read(reader);
                CCV.Read(reader);
                RMV.Read(reader);
                MDP.Read(reader);
                MaterialTemperature.Read(reader);
                MachineSpeed.Read(reader);
                RadioLatency.Read(reader);
                GPSModeStore.Read(reader);
                Frequency.Read(reader);
                Amplitude.Read(reader);
                CCA.Read(reader);
            }

            /// <summary>
            /// Calculate the chained offsets and numbers of required bits for each attribute being stored
            /// </summary>
            /// <param name="NumBitsPerCellPass"></param>
            public void CalculateTotalOffsetBits(out int NumBitsPerCellPass)
            {
                MachineIDIndex.OffsetBits = 0;
                Time.OffsetBits = (ushort)(MachineIDIndex.OffsetBits + MachineIDIndex.RequiredBits);
                Height.OffsetBits = (ushort)(Time.OffsetBits + Time.RequiredBits);
                CCV.OffsetBits = (ushort)(Height.OffsetBits + Height.RequiredBits);
                RMV.OffsetBits = (ushort)(CCV.OffsetBits + CCV.RequiredBits);
                MDP.OffsetBits = (ushort)(RMV.OffsetBits + RMV.RequiredBits);
                MaterialTemperature.OffsetBits = (ushort)(MDP.OffsetBits + MDP.RequiredBits);
                MachineSpeed.OffsetBits = (ushort)(MaterialTemperature.OffsetBits + MaterialTemperature.RequiredBits);
                RadioLatency.OffsetBits = (ushort)(MachineSpeed.OffsetBits + MachineSpeed.RequiredBits);
                GPSModeStore.OffsetBits = (ushort)(RadioLatency.OffsetBits + RadioLatency.RequiredBits);
                Frequency.OffsetBits = (ushort)(GPSModeStore.OffsetBits + GPSModeStore.RequiredBits);
                Amplitude.OffsetBits = (ushort)(Frequency.OffsetBits + Frequency.RequiredBits);
                CCA.OffsetBits = (ushort)(Amplitude.OffsetBits + Amplitude.RequiredBits);

                // Calculate the total number of bits required and pass back
                NumBitsPerCellPass = CCA.OffsetBits + CCA.RequiredBits;
            }
        }

        /// <summary>
        /// The time stamp of the earliest recorded cell pass stored in the segment. All time stamps for cell passes
        /// in the segment store times that are relative to this time stamp
        /// </summary>
        private DateTime FirstRealCellPassTime;

        /// <summary>
        /// BF_CellPasses contains all the cell pass information for the segment (read in via
        /// the transient CellPassesStorage reference and then encoded into the cache format)
        /// </summary>
        private BitFieldArray BF_CellPasses;

        // BF_PassCounts contains the pass count and first cell pass index information
        // for each cell in the segment. It is arranges in two parts:
        // 1. For each column, a value containing the summation of the pass counts up
        //    to the first cell in that column, stored as an entropic bitfield array
        //    at the start of BF_PassCounts.
        // 2. For each cell, the offset from the column value for the cell pass index
        //    of the first cell pass in that cell (so, the first cell passes index
        //    is always given as <FirstCellPassIndexOfColumn> + <FirstCellPassIndexForCellInColumn>
        private BitFieldArray BF_PassCounts;

        /// <summary>
        /// EncodedColPassCountsBits contains the number of bits required to store the per column counts in BF_PassCounts.
        /// </summary>
        private byte EncodedColPassCountsBits;

        /// <summary>
        /// The offset from the start of the bit field array containing the cell pass information for the cells in the 
        /// segment after the column index information that is also stored in the bit field array
        /// </summary>
        private int FirstPerCellPassIndexOffset;

        /// <summary>
        /// The set of encoded field descriptors that track ate attributes and parameters of each vector of values
        /// stored in the bit field array.
        /// </summary>
        private EncodedFieldDescriptorsStruct EncodedFieldDescriptors; // = new EncodedFieldDescriptorsStruct();

        /// <summary>
        /// Mapping of Machine/Asset GUIDs to the internal machine index within the site model
        /// </summary>
        private short[] MachineIDs;

        /// <summary>
        /// A bit set representing the set of machines defined in an efficient structure 
        /// for use in filtering operations during requests
        /// </summary>
        private BitArray MachineIDSet;

        /// <summary>
        /// The end coded field descriptor for the vector of pass counts of cell passes in the segment
        /// </summary>
        private EncodedBitFieldDescriptor PassCountEncodedFieldDescriptor;

        /// <summary>
        /// The number of bits required to store all the fields from a cell pass within the bit field array
        /// representation of that record.
        /// </summary>
        private int NumBitsPerCellPass;

        /// <summary>
        /// Default no-arg constructor that does not instantiate any state
        /// </summary>
        public SubGridCellSegmentPassesDataWrapper_StaticCompressed()
        {
        }

        /// <summary>
        /// Add a pass to a cell pass list. Not supported as this is an immutable structure.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pass"></param>
        /// <param name="position"></param>
        public void AddPass(int X, int Y, CellPass pass, int position = -1)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Allocate cell passes for a cell. Not supported as this is an immutable structure.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        public void AllocatePasses(int X, int Y, int passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public Cell_NonStatic ExtractCellPasses(int X, int Y)
        {
            throw new InvalidOperationException("Non-static cell descriptions not supported by compressed static segments");
        }

        /// <summary>
        /// Retrieves the number of passes present in this segment for the cell identified by col and row, as well as
        /// the index of the first cell pass in that list within the overall list of passes for the cell in this segment
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public SubGridCellPassCountRecord GetPassCountAndFirstCellPassIndex(int Col, int Row)
        {
            // Read the per column first cell pass index, then read the first cell pass offset for the cell itself.
            // Adding the two gives us the first cell pass index for this cell. Then, calculate the pass count by reading the
            // cell pass index for the next cell. This may be in the next column, in which case the relevant first cell pass index for
            // that cell is the per column first cell pass index for the next column in the sub grid. I the cell is the last cell in the last
            // column then the pass count is the difference between the segment pass count and per cell first cell pass count.

            var Result = new SubGridCellPassCountRecord();

            // Remember, the counts are written in column order first in the bit field array.
            int PerColBitFieldLocation = Col * EncodedColPassCountsBits;
            long PerColFirstCellPassIndex = BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits);

            int PerCellBitFieldLocation = unchecked((FirstPerCellPassIndexOffset + ((Col * SubGridTreeConsts.SubGridTreeDimension) + Row) * PassCountEncodedFieldDescriptor.RequiredBits));
            int PerCellFirstCellPassIndexOffset = unchecked((int)BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits));

            Result.FirstCellPass = unchecked((int)(PerColFirstCellPassIndex + PerCellFirstCellPassIndexOffset));

            if (Row < SubGridTreeConsts.SubGridTreeDimensionMinus1)
            {
                Result.PassCount = unchecked((int)(BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits) - PerCellFirstCellPassIndexOffset));
            }
            else
            {
                if (Col < SubGridTreeConsts.SubGridTreeDimensionMinus1)
                {
                    int NextPerColFirstCellPassIndex = unchecked((int)BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits));
                    Result.PassCount = NextPerColFirstCellPassIndex - Result.FirstCellPass;
                }
                else
                {
                    Result.PassCount = segmentPassCount - Result.FirstCellPass;
                }
            }

            return Result;
        }

        /// <summary>
        /// Decodes all passes for the identified cell within this sub grid segment
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public CellPass[] DecodePasses(int CellX, int CellY)
        {
            var Index = GetPassCountAndFirstCellPassIndex(CellX, CellY);
            var cellPasses = new CellPass[Index.PassCount];

            for (int i = 0; i < Index.PassCount; i++)
            {
                cellPasses[i] = ExtractCellPass(Index.FirstCellPass + i);
            }

            return cellPasses;
        }

        /// <summary>
        /// Extracts a single cell pass from the cell passes within this segment from the cell identified by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        public CellPass ExtractCellPass(int X, int Y, int passNumber)
        {
            // X & Y indicate the cell location in the sub grid, and passNumber represents the index of the pass in the cell that is required

            // First determine the starting cell pass index for that location in the segment
            var Index = GetPassCountAndFirstCellPassIndex(X, Y);

            // Then extract the appropriate cell pass from the list
            return ExtractCellPass(Index.FirstCellPass + passNumber);
        }

        /// <summary>
        /// Extracts a single cell pass from the cell passes held within this segment where the cell pass is identified by its index
        /// within the set of cell passes stored for the segment
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public CellPass ExtractCellPass(int Index)
        {
            // IMPORTANT: The fields read in this method must be read in the  same order as they were written during encoding

            var Result = Cells.CellPass.CLEARED_CELL_PASS;

            int CellPassBitLocation = Index * NumBitsPerCellPass;

            Result.InternalSiteModelMachineIndex = (short)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MachineIDIndex);

            Result.Time = FirstRealCellPassTime.AddMilliseconds(AttributeValueModifiers.MILLISECONDS_TO_DECISECONDS_FACTOR * BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Time));

            long IntegerHeight = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Height);
            Result.Height = IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : (float)(IntegerHeight / 1000.0);

            Result.CCV = (short)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCV);
            Result.RMV = (short)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RMV);
            Result.MDP = (short)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MDP);
            Result.MaterialTemperature = (ushort)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MaterialTemperature);
            Result.MachineSpeed = (ushort)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MachineSpeed);
            Result.RadioLatency = (byte)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RadioLatency);

            Result.GPSModeStore = (byte)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.GPSModeStore); 

            Result.Frequency = (ushort)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Frequency);
            Result.Amplitude = (ushort)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Amplitude);
            Result.CCA = (byte)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCA);

            return Result;
        }

        public void Integrate(int X, int Y, Cell_NonStatic sourcePasses, int StartIndex, int EndIndex, out int AddedCount, out int ModifiedCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time would be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(int X, int Y, DateTime time, out int index)
        {
            throw new NotImplementedException();
        }

        public CellPass Pass(int X, int Y, int passIndex)
        {
            return ExtractCellPass(X, Y, passIndex);
        }

        public int PassCount(int X, int Y)
        {
            return GetPassCountAndFirstCellPassIndex(X, Y).PassCount;
        }

        public float PassHeight(int passIndex)
        {
            int BitLocation = passIndex * NumBitsPerCellPass + EncodedFieldDescriptors.Height.OffsetBits;

            long IntegerHeight = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Height);

            return IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : (float)(IntegerHeight / 1000.0);
        }

        public float PassHeight(int X, int Y, int passNumber)
        {
            // Translate the cell based pass number to the segment cell list based pass number 
            return PassHeight(GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public DateTime PassTime(int passIndex)
        {
            int BitLocation = passIndex * NumBitsPerCellPass + EncodedFieldDescriptors.Time.OffsetBits;
            return FirstRealCellPassTime.AddMilliseconds(AttributeValueModifiers.MILLISECONDS_TO_DECISECONDS_FACTOR * BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Time));
        }

        public DateTime PassTime(int X, int Y, int passNumber)
        {
            // Translate the cell based pass count to the segment cell list based pass count 
            return PassTime(GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public void Read(BinaryReader reader)
        {            
            FirstRealCellPassTime = DateTime.FromBinary(reader.ReadInt64());

            segmentPassCount = reader.ReadInt32();

            BF_CellPasses.Read(reader);
            BF_PassCounts.Read(reader);

            EncodedColPassCountsBits = reader.ReadByte();
            FirstPerCellPassIndexOffset = reader.ReadInt32();

            EncodedFieldDescriptors.Read(reader);

            int Count = reader.ReadInt32();
            MachineIDs = new short[Count]; //SubGridCellSegmentMachineReference[Count];

            for (int i = 0; i < Count; i++)
            {
              MachineIDs[i] = reader.ReadInt16();
            }

            NumBitsPerCellPass = reader.ReadInt32();

            PassCountEncodedFieldDescriptor.Read(reader);

            MachineIDSet = InitialiseMachineIDsSet(MachineIDs);
        }

        /// <summary>
        /// Converts the machines present in this segment into a BitArray representing a set of bits where
        /// the position of each bit is the InternalMachineID on the machines in the site model where the
        /// internal ID is 0..NMachineInSiteModel - 1
        /// </summary>
        /// <param name="machineIDs"></param>
        /// <returns></returns>
        private BitArray InitialiseMachineIDsSet(short[] machineIDs)
        {
            var bits = new BitArray((machineIDs.Length > 0 ? machineIDs.MaxValue() : 0) + 1);

            for (int i = 0, length = machineIDs.Length; i < length; i++)
              bits[machineIDs[i]] = true;

            return bits;
        }

        public void ReplacePass(int X, int Y, int position, CellPass pass)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        public void RemovePass(int X, int Y, int position)
        {
          throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FirstRealCellPassTime.ToBinary());
            writer.Write(segmentPassCount);

            BF_CellPasses.Write(writer);
            BF_PassCounts.Write(writer);

            writer.Write(EncodedColPassCountsBits);
            writer.Write(FirstPerCellPassIndexOffset);

            EncodedFieldDescriptors.Write(writer);

            int count = MachineIDs?.Length ?? 0;
            writer.Write(count);

            for (int i = 0; i < count; i++)
            {
                writer.Write(MachineIDs[i]);
            }

            writer.Write(NumBitsPerCellPass);

            PassCountEncodedFieldDescriptor.Write(writer);
        }

        /// <summary>
        /// Takes a full set of cell passes for the segment and converts them into the internal representation for
        /// the static compressed cell pass segment
        /// </summary>
        /// <param name="cellPasses"></param>
        private void PerformEncodingStaticCompressedCache(Cell_NonStatic[,] cellPasses)
        {
            int[] ColFirstCellPassIndexes = new int[SubGridTreeConsts.SubGridTreeDimension];
            int[,] PerCellColRelativeFirstCellPassIndexes = new int[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
            int cellPassIndex;
            int testValue;
            bool observedANullValue;
            bool firstValue;

            // Given the value range for each attribute, calculate the number of bits required to store the values.
            EncodedFieldDescriptors.Init();

            // Calculate the set of machine IDs present in the cell passes in the
            // segment. Calculate the cell pass time of the earliest cell pass in the segment
            // and the lowest elevation of all cell passes in the segment.

            // Determine the total number of passes that need to be stored and create the array to hold them
            segmentPassCount = 0;
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                segmentPassCount += cellPasses[i, j].PassCount;
              }
            }

            if (segmentPassCount > 10000)
              Log.LogWarning($"Converting segment with {segmentPassCount} cell passes into compressed form - suspicious?");

            // Construct the first cell pass index map for the segment
            // First calculate the values of the first cell pass index for each column in the segment
            ColFirstCellPassIndexes[0] = 0;
            for (int Col = 0; Col < SubGridTreeConsts.SubGridTreeDimensionMinus1; Col++)
            {
                ColFirstCellPassIndexes[Col + 1] = ColFirstCellPassIndexes[Col];

                for (int Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
                {
                   ColFirstCellPassIndexes[Col + 1] += cellPasses[Col, Row].PassCount;
                }
            }

            // Next modify the cell passes array to hold first cell pass indices relative to the
            // 'per column' first cell pass indices
            for (int Col = 0; Col < SubGridTreeConsts.SubGridTreeDimension; Col++)
            {
                PerCellColRelativeFirstCellPassIndexes[Col, 0] = 0;

                for (int Row = 1; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
                {
                    PerCellColRelativeFirstCellPassIndexes[Col, Row] = PerCellColRelativeFirstCellPassIndexes[Col, Row - 1] + cellPasses[Col, Row - 1].PassCount;
                }
            }

            // Compute the value range and number of bits required to store the column first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.MinValue = ColFirstCellPassIndexes.MinValue();
            PassCountEncodedFieldDescriptor.MaxValue = ColFirstCellPassIndexes.MaxValue();

            PassCountEncodedFieldDescriptor.CalculateRequiredBitFieldSize();
            EncodedColPassCountsBits = PassCountEncodedFieldDescriptor.RequiredBits;
            FirstPerCellPassIndexOffset = SubGridTreeConsts.SubGridTreeDimension * EncodedColPassCountsBits;

            // Compute the value range and number of bits required to store the cell first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.NativeNullValue = 0;
            PassCountEncodedFieldDescriptor.MinValue = 0;
            PassCountEncodedFieldDescriptor.MaxValue = 0;

            observedANullValue = false;
            firstValue = true;

            for (int Col = 0; Col < SubGridTreeConsts.SubGridTreeDimension; Col++)
            {
              for (int Row = 0; Row < SubGridTreeConsts.SubGridTreeDimension; Row++)
              {
                testValue = PerCellColRelativeFirstCellPassIndexes[Col, Row];

                if (PassCountEncodedFieldDescriptor.Nullable)
                {
                  if (PassCountEncodedFieldDescriptor.MinValue == PassCountEncodedFieldDescriptor.NativeNullValue
                      || (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) &&
                      (testValue < PassCountEncodedFieldDescriptor.MinValue))
                  {
                    PassCountEncodedFieldDescriptor.MinValue = testValue;
                  }

                  if (PassCountEncodedFieldDescriptor.MaxValue == PassCountEncodedFieldDescriptor.NativeNullValue ||
                      (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) &&
                      (testValue > PassCountEncodedFieldDescriptor.MaxValue))
                  {
                    PassCountEncodedFieldDescriptor.MaxValue = testValue;
                  }
                }
                else
                {
                  if (firstValue || testValue < PassCountEncodedFieldDescriptor.MinValue)
                  {
                    PassCountEncodedFieldDescriptor.MinValue = testValue;
                  }

                  if (firstValue || testValue > PassCountEncodedFieldDescriptor.MaxValue)
                  {
                    PassCountEncodedFieldDescriptor.MaxValue = testValue;
                  }
                }

                if (!observedANullValue && testValue == PassCountEncodedFieldDescriptor.NativeNullValue)
                {
                  observedANullValue = true;
                }

                firstValue = false;
              }
            }

            // If the data stream processed contained no null values, then force the
            // nullable flag to false so we don't encode an extra token for a null value
            // that will never be written.
            if (!observedANullValue)
            {
                PassCountEncodedFieldDescriptor.Nullable = false;
            }

            if (PassCountEncodedFieldDescriptor.Nullable && PassCountEncodedFieldDescriptor.MaxValue != PassCountEncodedFieldDescriptor.NativeNullValue)
            {
                PassCountEncodedFieldDescriptor.MaxValue++;
                PassCountEncodedFieldDescriptor.EncodedNullValue = PassCountEncodedFieldDescriptor.MaxValue;
            }
            else
            {
                PassCountEncodedFieldDescriptor.EncodedNullValue = 0;
            }

            PassCountEncodedFieldDescriptor.CalculateRequiredBitFieldSize();

            // For ease of management convert all the cell passes into a single list for the following operations
            // Compute the earliest cell pass time while we are at it
            FirstRealCellPassTime = Consts.MAX_DATETIME_AS_UTC;

            var allCellPassesArray = CellPassArrayPoolCacheHelper.Caches.Rent(segmentPassCount);
            try
            {
              cellPassIndex = 0;

              for (int col = 0; col < SubGridTreeConsts.SubGridTreeDimension; col++)
              {
                for (int row = 0; row < SubGridTreeConsts.SubGridTreeDimension; row++)
                {
                  var passes = cellPasses[col, row].Passes;

                  if (passes.Count > 0)
                  {
                    Array.Copy(passes.Elements, passes.Offset, allCellPassesArray, cellPassIndex, passes.Count);
                    var firstPassTime = allCellPassesArray[cellPassIndex].Time;

                    if (firstPassTime < FirstRealCellPassTime)
                      FirstRealCellPassTime = firstPassTime;

                    #if CELLDEBUG
                    for (int i = cellPassIndex + 1; i < cellPassIndex + passes.Count; i++)
                    {
                      if (allCellPassesArray[i].Time < FirstRealCellPassTime)
                        throw new Exception($"Cell passes out of order at index {i}: {FirstRealCellPassTime.Ticks} should be less than or equal to {allCellPassesArray[i].Time.Ticks}");
                    }
                    #endif

                    cellPassIndex += passes.Count;
                  }
                }
              }

              // Finalize computing the time of the earliest real cell pass within the segment
              if (FirstRealCellPassTime == Consts.MAX_DATETIME_AS_UTC)
                FirstRealCellPassTime = Consts.MIN_DATETIME_AS_UTC;

              // Initialise the MachineIDs array and the MachineIDSet that encodes it as a bit array
              MachineIDSet = new BitArray(allCellPassesArray.MaxInternalSiteModelMachineIndex(segmentPassCount) + 1);
              for (int i = 0; i < segmentPassCount; i++)
                MachineIDSet[allCellPassesArray[i].InternalSiteModelMachineIndex] = true;

              MachineIDs = new short[MachineIDSet.Length];
              int machineIDCount = 0;
              for (int i = 0, length = MachineIDs.Length; i < length; i++)
              {
                if (MachineIDSet[i])
                  MachineIDs[machineIDCount++] = MachineIDs[i];
              }

              Array.Resize(ref MachineIDs, machineIDCount);

              // Convert time and elevation value to offset values in the appropriate units
              // from the lowest values of those attributes. 

              // Work out the value ranges of all the attributes and given the value range
              // for each attribute, calculate the number of bits required to store the values.
              // Note:
              // Time - based on the long word, second accurate times overriding the TDateTime times
              // Height - based on the long word, millimeter accurate elevations overriding the IEEE double elevations

              long[] CalculateAttributeValueRange_Buffer = new long[segmentPassCount];

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].InternalSiteModelMachineIndex;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullInternalSiteModelMachineIndex, true, ref EncodedFieldDescriptors.MachineIDIndex);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] =
                  AttributeValueModifiers.ModifiedTime(allCellPassesArray[i].Time, FirstRealCellPassTime);
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer,
                0x7fff_ffff_ffff_ffff, -1, true, ref EncodedFieldDescriptors.Time);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] =
                  AttributeValueModifiers.ModifiedHeight(allCellPassesArray[i].Height);
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer,
                0xffffffff, 0x7fffffff, true, ref EncodedFieldDescriptors.Height);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].CCV;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullCCV, true, ref EncodedFieldDescriptors.CCV);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].RMV;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullRMV, true, ref EncodedFieldDescriptors.RMV);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].MDP;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullMDP, true, ref EncodedFieldDescriptors.MDP);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].MaterialTemperature;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullMaterialTemperatureValue, true, ref EncodedFieldDescriptors.MaterialTemperature);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].GPSModeStore;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xff,
                (int) CellPassConsts.NullGPSMode, true, ref EncodedFieldDescriptors.GPSModeStore);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].MachineSpeed;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullMachineSpeed, true, ref EncodedFieldDescriptors.MachineSpeed);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].RadioLatency;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xff,
                CellPassConsts.NullRadioLatency, true, ref EncodedFieldDescriptors.RadioLatency);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].Frequency;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullFrequency, true, ref EncodedFieldDescriptors.Frequency);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].Amplitude;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xffff,
                CellPassConsts.NullAmplitude, true, ref EncodedFieldDescriptors.Amplitude);

              for (int i = 0; i < segmentPassCount; i++)
                CalculateAttributeValueRange_Buffer[i] = allCellPassesArray[i].CCA;
              AttributeValueRangeCalculator.CalculateAttributeValueRange(CalculateAttributeValueRange_Buffer, 0xff,
                CellPassConsts.NullCCA, true, ref EncodedFieldDescriptors.CCA);

              // Calculate the offset bit locations for the cell pass attributes
              EncodedFieldDescriptors.CalculateTotalOffsetBits(out NumBitsPerCellPass);

              // Create the bit field arrays to contain the segment call pass index & count plus passes.
              BitFieldArrayRecordsDescriptor[] recordDescriptors =
              {
                new BitFieldArrayRecordsDescriptor
                {
                  NumRecords = SubGridTreeConsts.SubGridTreeDimension, BitsPerRecord = EncodedColPassCountsBits
                },
                new BitFieldArrayRecordsDescriptor
                {
                  NumRecords = SubGridTreeConsts.SubGridTreeCellsPerSubGrid,
                  BitsPerRecord = PassCountEncodedFieldDescriptor.RequiredBits
                }
              };

              BF_PassCounts.Initialise(recordDescriptors);
              BF_PassCounts.StreamWriteStart();
              try
              {
                // Write the column based first cell pass indexes into BF_PassCounts
                for (int i = 0, length = ColFirstCellPassIndexes.Length; i < length; i++)
                {
                  BF_PassCounts.StreamWrite(ColFirstCellPassIndexes[i], EncodedColPassCountsBits);
                }

                // Write the cell pass count for each cell relative to the column based cell pass count
                for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
                {
                  for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                  {
                    BF_PassCounts.StreamWrite(PerCellColRelativeFirstCellPassIndexes[i, j],
                      PassCountEncodedFieldDescriptor.RequiredBits);
                  }
                }
              }
              finally
              {
                BF_PassCounts.StreamWriteEnd();
              }

              // Copy the call passes themselves into BF
              recordDescriptors = new[]
              {
                new BitFieldArrayRecordsDescriptor {NumRecords = segmentPassCount, BitsPerRecord = NumBitsPerCellPass}
              };

              BF_CellPasses.Initialise(recordDescriptors);
              BF_CellPasses.StreamWriteStart();
              try
              {
                for (int i = 0; i < segmentPassCount; i++)
                {
                  var pass = allCellPassesArray[i];

                  BF_CellPasses.StreamWrite(pass.InternalSiteModelMachineIndex, EncodedFieldDescriptors.MachineIDIndex);
                  BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedTime(pass.Time, FirstRealCellPassTime), EncodedFieldDescriptors.Time);
                  BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedHeight(pass.Height), EncodedFieldDescriptors.Height);
                  BF_CellPasses.StreamWrite(pass.CCV, EncodedFieldDescriptors.CCV);
                  BF_CellPasses.StreamWrite(pass.RMV, EncodedFieldDescriptors.RMV);
                  BF_CellPasses.StreamWrite(pass.MDP, EncodedFieldDescriptors.MDP);
                  BF_CellPasses.StreamWrite(pass.MaterialTemperature, EncodedFieldDescriptors.MaterialTemperature);
                  BF_CellPasses.StreamWrite(pass.MachineSpeed, EncodedFieldDescriptors.MachineSpeed);
                  BF_CellPasses.StreamWrite(pass.RadioLatency, EncodedFieldDescriptors.RadioLatency);
                  BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedGPSMode(pass.gpsMode), EncodedFieldDescriptors.GPSModeStore);
                  BF_CellPasses.StreamWrite(pass.Frequency, EncodedFieldDescriptors.Frequency);
                  BF_CellPasses.StreamWrite(pass.Amplitude, EncodedFieldDescriptors.Amplitude);
                  BF_CellPasses.StreamWrite(pass.CCA, EncodedFieldDescriptors.CCA);
                }
              }
              finally
              {
                BF_CellPasses.StreamWriteEnd();
              }
            }
            finally
            {
              CellPassArrayPoolCacheHelper.Caches.Return(allCellPassesArray);
            }

            //Log.LogInformation($"Internal cache encoding for cell passes required {BF_CellPasses.NumBits / 8} bytes @ {NumBitsPerCellPass} bits per cell pass & {BF_PassCounts.NumBits / 8} bytes for pass counts");

            // if not VerifyCellPassEncoding then
            //   SIGLogMessage.PublishNoODS(Self, 'Segment VerifyCellPassEncoding failed', slmcMessage);
        }

        /// <summary>
        /// Takes a description of the cell passes to be placed into this segment and converts them into the
        /// internal compressed representation
        /// </summary>
        /// <param name="cellPasses"></param>
        public void SetState(Cell_NonStatic[,] cellPasses)
        {
            // Convert the supplied cell passes into the appropriate bit field arrays
            PerformEncodingStaticCompressedCache(cellPasses);
        }

        public void SetStatePassingOwnership(ref Cell_NonStatic[,] cellPasses)
        {
          this.SetState(cellPasses);
          cellPasses = null;
        }

        public Cell_NonStatic[,] GetState()
        {
            throw new NotImplementedException("Does not support GetState()");
        }

        /// <summary>
        /// Note that this information is immutable
        /// </summary>
        /// <returns></returns>
        public bool IsImmutable() => true;

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MinPassCount"></param>
        /// <param name="MaxPassCount"></param>
        public void CalculateTotalPasses(out int TotalPasses, out int MinPassCount, out int MaxPassCount)
        {
            SegmentTotalPassesCalculator.CalculateTotalPasses(this, out TotalPasses, out MinPassCount, out MaxPassCount);
        }

        /// <summary>
        /// Calculates the time range covering all the cell passes within this segment
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void CalculateTimeRange(out DateTime startTime, out DateTime endTime)
        {
            SegmentTimeRangeCalculator.CalculateTimeRange(this, out startTime, out endTime);
        }

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        public void CalculatePassesBeforeTime(DateTime searchTime, out int totalPasses, out int maxPassCount)
        {
            SegmentTimeRangeCalculator.CalculatePassesBeforeTime(this, searchTime, out totalPasses, out maxPassCount);
        }

        public void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime)
        {
            throw new NotImplementedException("Static cell segment passes wrappers do not support cell pass adoption");
        }

        /// <summary>
        /// Returns a the machine ID set for cell pass wrappers. This set contains a bit flag per machine
        /// present within the cell passes in this segment
        /// </summary>
        /// <returns></returns>

        public BitArray GetMachineIDSet() => MachineIDSet;

      /// <summary>
      /// Sets the internal machine ID for the cell pass identified by x & y spatial location and passNumber.
      /// </summary>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="passNumber"></param>
      /// <param name="internalMachineID"></param>
      public void SetInternalMachineID(int X, int Y, int passNumber, short internalMachineID)
      {
        throw new InvalidOperationException("Immutable cell pass segment.");
      }

      /// <summary>
      /// Sets the internal machine ID for all cell passes within the segment to the provided ID.
      /// </summary>
      /// <param name="internalMachineIndex"></param>
      /// <param name="numModifiedPasses"></param>
      public void SetAllInternalMachineIDs(short internalMachineIndex, out long numModifiedPasses)
      {
        throw new InvalidOperationException("Immutable cell pass segment.");
      }

      public void GetSegmentElevationRange(out double MinElev, out double MaxElev)
      {
        if (EncodedFieldDescriptors.Height.AllValuesAreNull)
        {
          MinElev = Consts.NullDouble;
          MaxElev = Consts.NullDouble;
        }
        else
        {
          MinElev = EncodedFieldDescriptors.Height.MinValue / 1000.0d;
          MaxElev = EncodedFieldDescriptors.Height.MaxValue / 1000.0d;
        }
      }

      public bool HasPassData() => true; //BF_CellPasses != null;

      public void ReplacePasses(int X, int Y, CellPass[] cellPasses, int passCount)
      {
        throw new NotImplementedException("Does not support ReplacePasses()");
      }

      public void AllocatePassesExact(int X, int Y, int passCount)
      {
        throw new NotImplementedException("Does not support AllocatePassesExact()");
      }

#region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        // Treat disposal and finalization as the same, dependent on the primary disposedValue flag
        // No IDisposable obligation in this class

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
#endregion
  }
}
