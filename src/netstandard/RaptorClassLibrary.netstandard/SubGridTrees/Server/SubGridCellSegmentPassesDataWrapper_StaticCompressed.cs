using System;
using System.Collections;
using System.IO;
using System.Linq;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Compression;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Represents an efficiently compressed version of the cell pass information included within a segment.
    /// The compression achieves approximately 10:1 reduction in memory use while preserving random access with
    /// good performance.
    /// </summary>
    [Serializable]
    public class SubGridCellSegmentPassesDataWrapper_StaticCompressed : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
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
            /// Deserialise all descriptors using the supplied reader
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
            /// Calculate the chained offsets and numbers of requiredbits for each attribute being stored
            /// </summary>
            /// <param name="NumBitsPerCellPass"></param>
            public void CalculateTotalOffsetBits(out int NumBitsPerCellPass)
            {
                MachineIDIndex.OffsetBits = 0;
                Time.OffsetBits = (byte)(MachineIDIndex.OffsetBits + MachineIDIndex.RequiredBits);
                Height.OffsetBits = (byte)(Time.OffsetBits + Time.RequiredBits);
                CCV.OffsetBits = (byte)(Height.OffsetBits + Height.RequiredBits);
                RMV.OffsetBits = (byte)(CCV.OffsetBits + CCV.RequiredBits);
                MDP.OffsetBits = (byte)(RMV.OffsetBits + RMV.RequiredBits);
                MaterialTemperature.OffsetBits = (byte)(MDP.OffsetBits + MDP.RequiredBits);
                MachineSpeed.OffsetBits = (byte)(MaterialTemperature.OffsetBits + MaterialTemperature.RequiredBits);
                RadioLatency.OffsetBits = (byte)(MachineSpeed.OffsetBits + MachineSpeed.RequiredBits);
                GPSModeStore.OffsetBits = (byte)(RadioLatency.OffsetBits + RadioLatency.RequiredBits);
                Frequency.OffsetBits = (byte)(GPSModeStore.OffsetBits + GPSModeStore.RequiredBits);
                Amplitude.OffsetBits = (byte)(Frequency.OffsetBits + Frequency.RequiredBits);
                CCA.OffsetBits = (byte)(Amplitude.OffsetBits + Amplitude.RequiredBits);

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
        /// EncodedColPassCountsBits containes the number of bits required to store the per column counts in BF_PassCounts.
        /// </summary>
        private byte EncodedColPassCountsBits;

        /// <summary>
        /// The offset from the start of the bit field array containing the cell pass information for the cells in the 
        /// segment after the column index information that is also stored in the bit field arrat
        /// </summary>
        private int FirstPerCellPassIndexOffset;

        /// <summary>
        /// The set of encoded field descriptors that track ate attributes and parameters of each vector of values
        /// stored in the bit field array.
        /// </summary>
        private EncodedFieldDescriptorsStruct EncodedFieldDescriptors; // = new EncodedFieldDescriptorsStruct();

        /// <summary>
        /// Mapping of Machine/Asset Guids to the internal machine index within the site model
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
        public void AddPass(uint X, uint Y, CellPass pass, int position = -1)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Allocate cell passes for a cell. Not supported as this is an immutable structure.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        public void AllocatePasses(uint X, uint Y, uint passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass[] ExtractCellPasses(uint X, uint Y)
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
        public SubGridCellPassCountRecord GetPassCountAndFirstCellPassIndex(uint Col, uint Row)
        {
            // Read the per column first cell pass index, then read the first cell pass offset for the cell itself.
            // Adding the two gives us the first cell pass index for this cell. Then, calculate the passcount by reading the
            // cell pass index for the next cell. This may be in the next column, in which case the relavant first cell pass index for
            // that cell is the  per column first cell pass index for the next column in the subgrid. I the cell is the last cell in the last
            // column then the pass count is the difference between the segment pass count and per cell first cell pass count.

            SubGridCellPassCountRecord Result = new SubGridCellPassCountRecord();

            // Remember, the counts are written in column order first in the bit field array.
            int PerColBitFieldLocation = (int)Col * EncodedColPassCountsBits;
            int PerColFirstCellPassIndex = (int)BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits);

            int PerCellBitFieldLocation = (int)(FirstPerCellPassIndexOffset + ((Col * SubGridTree.SubGridTreeDimension) + Row) * PassCountEncodedFieldDescriptor.RequiredBits);
            int PerCellFirstCellPassIndexOffset = (int)BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits);

            Result.FirstCellPass = PerColFirstCellPassIndex + PerCellFirstCellPassIndexOffset;

            if (Row < SubGridTree.SubGridTreeDimensionMinus1)
            {
                Result.PassCount = (int)BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits) - PerCellFirstCellPassIndexOffset;
            }
            else
            {
                if (Col < SubGridTree.SubGridTreeDimensionMinus1)
                {
                    int NextPerColFirstCellPassIndex = (int)BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits);
                    Result.PassCount = NextPerColFirstCellPassIndex - Result.FirstCellPass;
                }
                else
                {
                    Result.PassCount = SegmentPassCount - Result.FirstCellPass;
                }
            }

            return Result;
        }

        /// <summary>
        /// Decodes all passes for the identified cell within this subgrid segment
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public CellPass[] DecodePasses(uint CellX, uint CellY)
        {
            SubGridCellPassCountRecord Index = GetPassCountAndFirstCellPassIndex(CellX, CellY);
            CellPass[] cellPasses = new CellPass[Index.PassCount];

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
        public CellPass ExtractCellPass(uint X, uint Y, int passNumber)
        {
            // X & Y indicate the cell lcoation in the subgrid, and passNumber represents the index of the pass in the cell that is required

            // First determine the starting cell pass index for that location in the segment
            SubGridCellPassCountRecord Index = GetPassCountAndFirstCellPassIndex(X, Y);

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

            CellPass Result = new CellPass();

            int CellPassBitLocation = Index * NumBitsPerCellPass;

            Result.InternalSiteModelMachineIndex = (short)BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MachineIDIndex);

            Result.Time = FirstRealCellPassTime.AddSeconds(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Time));

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

        public void Integrate(uint X, uint Y, CellPass[] sourcePasses, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time woule be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(uint X, uint Y, DateTime time, out int index)
        {
            throw new NotImplementedException();
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return ExtractCellPass(X, Y, (int)passIndex);
        }

        public uint PassCount(uint X, uint Y)
        {
            return (uint)GetPassCountAndFirstCellPassIndex(X, Y).PassCount;
        }

        public float PassHeight(uint passIndex)
        {
            int BitLocation = (int)(passIndex * NumBitsPerCellPass) + EncodedFieldDescriptors.Height.OffsetBits;

            long IntegerHeight = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Height);

            return IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : (float)(IntegerHeight / 1000.0);
        }

        public float PassHeight(uint X, uint Y, uint passNumber)
        {
            // Translate the cell based pass number to the segment cell list based pass number 
            return PassHeight((uint)GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public DateTime PassTime(uint passIndex)
        {
            int BitLocation = (int)(passIndex * NumBitsPerCellPass + EncodedFieldDescriptors.Time.OffsetBits);
            return FirstRealCellPassTime.AddSeconds(BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Time));
        }

        public DateTime PassTime(uint X, uint Y, uint passNumber)
        {
            // Translate the cell based pass number to the segment cell list based pass number 
            return PassTime((uint)GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public void Read(BinaryReader reader)
        {            
            FirstRealCellPassTime = DateTime.FromBinary(reader.ReadInt64());

            SegmentPassCount = reader.ReadInt32();

            BF_CellPasses.Read(reader);
            BF_PassCounts.Read(reader);

            EncodedColPassCountsBits = reader.ReadByte();
            FirstPerCellPassIndexOffset = reader.ReadInt32();

            EncodedFieldDescriptors.Read(reader);

            int Count = reader.ReadInt32();
            MachineIDs = new short[Count]; //SubgridCellSegmentMachineReference[Count];

            for (int i = 0; i < Count; i++)
                MachineIDs[i] = reader.ReadInt16();

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
            BitArray bits = new BitArray(machineIDs.Max() + 1);

            foreach (var machineID in machineIDs)
                bits[machineID] = true;

            return bits;
        }

        public void ReplacePass(uint X, uint Y, int position, CellPass pass)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FirstRealCellPassTime.ToBinary());
            writer.Write(SegmentPassCount);

            BF_CellPasses.Write(writer);
            BF_PassCounts.Write(writer);

            writer.Write(EncodedColPassCountsBits);
            writer.Write(FirstPerCellPassIndexOffset);

            EncodedFieldDescriptors.Write(writer);

            int count = MachineIDs.Length;
            writer.Write(count);
            
            for (int i = 0; i < count; i++)
            {
                writer.Write(MachineIDs[i]);
            }

            writer.Write(NumBitsPerCellPass);

            PassCountEncodedFieldDescriptor.Write(writer);
        }

        /// <summary>
        /// Takes a full set of cell passes for the sement and converts them into the internal representation for
        /// the static compressed cell pass segment
        /// </summary>
        /// <param name="cellPasses"></param>
        private void PerformEncodingStaticCompressedCache(CellPass[,][] cellPasses)
        {
            int[] ColFirstCellPassIndexes = new int[SubGridTree.SubGridTreeDimension];
            int[,] PerCellColRelativeFirstCellPassIndexes = new int[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            int cellPassIndex;
            int testValue;
            bool observedANullValue;
            bool firstValue;

            // Given the value range for each attribute, calculate the number of bits required to store the values.
            EncodedFieldDescriptors.Init();

            // Calculate the set of machine IDs present in the cell passes in the
            // segment. Calculate the cell pass time of the earliest cell pass in the segment
            // and the lowest elevation of all cell passes in the segment.

            SegmentPassCount = 0;
            foreach (CellPass[] passes in cellPasses)
            {
                if (passes != null)
                {
                    SegmentPassCount += passes.Length;
                }
            }

            // Construct the first cell pass index map for the segment
            // First calculate the values of the first cell pass index for each column in the segment
            ColFirstCellPassIndexes[0] = 0;
            for (int Col = 0; Col < SubGridTree.SubGridTreeDimensionMinus1; Col++)
            {
                ColFirstCellPassIndexes[Col + 1] = ColFirstCellPassIndexes[Col];

                for (int Row = 0; Row < SubGridTree.SubGridTreeDimension; Row++)
                {
                    if (cellPasses[Col, Row] != null)
                    {
                        ColFirstCellPassIndexes[Col + 1] += cellPasses[Col, Row].Length;
                    }
                }
            }

            // Next modify the cell passes array to hold first cell pass indices relative to the
            // 'per column' first cell pass indices
            for (int Col = 0; Col < SubGridTree.SubGridTreeDimension; Col++)
            {
                PerCellColRelativeFirstCellPassIndexes[Col, 0] = 0;

                for (int Row = 1; Row < SubGridTree.SubGridTreeDimension; Row++)
                {
                    PerCellColRelativeFirstCellPassIndexes[Col, Row] = PerCellColRelativeFirstCellPassIndexes[Col, Row - 1] + (cellPasses[Col, Row - 1] == null ? 0 : cellPasses[Col, Row - 1].Length);
                }
            }

            // Compute the value range and number of bits required to store the column first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.MinValue = ColFirstCellPassIndexes.Min(x => x);
            PassCountEncodedFieldDescriptor.MaxValue = ColFirstCellPassIndexes.Max(x => x);

            PassCountEncodedFieldDescriptor.CalculateRequiredBitFieldSize();
            EncodedColPassCountsBits = PassCountEncodedFieldDescriptor.RequiredBits;
            FirstPerCellPassIndexOffset = SubGridTree.SubGridTreeDimension * EncodedColPassCountsBits;

            // Compute the value range and number of bits required to store the cell first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.NativeNullValue = 0;
            PassCountEncodedFieldDescriptor.MinValue = 0;
            PassCountEncodedFieldDescriptor.MaxValue = 0;

            observedANullValue = false;
            firstValue = true;

            SubGridUtilities.SubGridDimensionalIterator((Col, Row) =>
            {
                testValue = PerCellColRelativeFirstCellPassIndexes[Col, Row];

                if (PassCountEncodedFieldDescriptor.Nullable)
                {
                    if (PassCountEncodedFieldDescriptor.MinValue == PassCountEncodedFieldDescriptor.NativeNullValue
                              || (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) && (testValue < PassCountEncodedFieldDescriptor.MinValue))
                    {
                        PassCountEncodedFieldDescriptor.MinValue = testValue;
                    }

                    if (PassCountEncodedFieldDescriptor.MaxValue == PassCountEncodedFieldDescriptor.NativeNullValue ||
                        (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) && (testValue > PassCountEncodedFieldDescriptor.MaxValue))
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
            });

            // If the data stream processed contained no null values, then force the
            // nullable flag to flas so we don;t encode an extra token for a null value
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
            CellPass[] allCellPassesArray = new CellPass[SegmentPassCount];
            cellPassIndex = 0;

            SubGridUtilities.SubGridDimensionalIterator((col, row) =>
            {
                CellPass[] passes = cellPasses[col, row];

                if (passes != null)
                {
                    Array.Copy(passes, 0, allCellPassesArray, cellPassIndex, passes.Length);
                    cellPassIndex += passes.Length;
                }
            });

            // Compute the time of the earliest real cell pass within the segment
            FirstRealCellPassTime = allCellPassesArray.Length > 0 ? allCellPassesArray.Min(x => x.Time) : DateTime.MinValue;

            // Inialise the MachineIDs array and the MachineIDSet that encodes it as a bit array
            MachineIDSet = new BitArray(allCellPassesArray.Max(x => x.InternalSiteModelMachineIndex) + 1);
            foreach (var cellPass in allCellPassesArray)
                MachineIDSet[cellPass.InternalSiteModelMachineIndex] = true;
            MachineIDs = Enumerable.Range(0, MachineIDSet.Length).Where(x => MachineIDSet[x]).Select(x => (short) x).ToArray();

            // Convert time and elevation value to offset values in the appropriate units
            // from the lowest values of those attributes. 

            // Work out the value ranges of all the attributes and given the value range
            // for each attribute, calculate the number of bits required to store the values.
            // Note:
            // Time - based on the longword, second accurate times overriding the TDateTime times
            // Height - based on the longword, millimeter accurate elevations overriding the IEEE double elevations

            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.InternalSiteModelMachineIndex).ToArray(), 0xffffffff, 0, false, ref EncodedFieldDescriptors.MachineIDIndex);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedTime(x.Time, FirstRealCellPassTime)).ToArray(), 0xffffffff, 0, false, ref EncodedFieldDescriptors.Time);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedHeight(x.Height)).ToArray(), 0xffffffff, 0x7fffffff, true, ref EncodedFieldDescriptors.Height);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCV).ToArray(), 0xffffffff, CellPass.NullCCV, true, ref EncodedFieldDescriptors.CCV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.RMV).ToArray(), 0xffffffff, CellPass.NullRMV, true, ref EncodedFieldDescriptors.RMV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MDP).ToArray(), 0xffffffff, CellPass.NullMDP, true, ref EncodedFieldDescriptors.MDP);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MaterialTemperature).ToArray(), 0xffffffff, CellPass.NullMaterialTemp, true, ref EncodedFieldDescriptors.MaterialTemperature);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.GPSModeStore).ToArray(), 0xff, (int)CellPass.NullGPSMode, true, ref EncodedFieldDescriptors.GPSModeStore);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MachineSpeed).ToArray(), 0xffffffff, CellPass.NullMachineSpeed, true, ref EncodedFieldDescriptors.MachineSpeed);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.RadioLatency).ToArray(), 0xffffffff, CellPass.NullRadioLatency, true, ref EncodedFieldDescriptors.RadioLatency);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.Frequency).ToArray(), 0xffffffff, CellPass.NullFrequency, true, ref EncodedFieldDescriptors.Frequency);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.Amplitude).ToArray(), 0xffffffff, CellPass.NullAmplitude, true, ref EncodedFieldDescriptors.Amplitude);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCA).ToArray(), 0xff, CellPass.NullCCA, true, ref EncodedFieldDescriptors.CCA);

            // Calculate the offset bit locations for the cell pass attributes
            EncodedFieldDescriptors.CalculateTotalOffsetBits(out NumBitsPerCellPass);

            // Create the bit field arrays to contain the segment call pass index & count plus passes.
            BitFieldArrayRecordsDescriptor[] recordDescriptors = new [] 
            {
                new BitFieldArrayRecordsDescriptor { NumRecords = SubGridTree.SubGridTreeDimension, BitsPerRecord = EncodedColPassCountsBits },
                new BitFieldArrayRecordsDescriptor { NumRecords = SubGridTree.SubGridTreeCellsPerSubgrid, BitsPerRecord = PassCountEncodedFieldDescriptor.RequiredBits }
            };

            BF_PassCounts.Initialise(recordDescriptors);
            BF_PassCounts.StreamWriteStart();
            try
            {
                // Write the column based first cell pass indexes into BF_PassCounts
                foreach (int firstPassIndex in ColFirstCellPassIndexes)
                {
                    BF_PassCounts.StreamWrite(firstPassIndex, EncodedColPassCountsBits);
                }

                // Write the cell pass count for each cell relative to the column based cell pass count
                SubGridUtilities.SubGridDimensionalIterator((col, row) =>
                {
                    BF_PassCounts.StreamWrite(PerCellColRelativeFirstCellPassIndexes[col, row], PassCountEncodedFieldDescriptor.RequiredBits);
                });
            }
            finally
            {
                BF_PassCounts.StreamWriteEnd();
            }

            // Copy the call passes themselves into BF
            recordDescriptors = new [] 
            {            
                new BitFieldArrayRecordsDescriptor { NumRecords = SegmentPassCount, BitsPerRecord = NumBitsPerCellPass }
            };

            BF_CellPasses.Initialise(recordDescriptors);
            BF_CellPasses.StreamWriteStart();
            try
            {
                foreach (CellPass pass in allCellPassesArray)
                {
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

            /*
            {$IFDEF DEBUG}
            // Read the values back again to check they were written as expected
            TestReadIndex:= 0;

            with BF_CellPasses do
              for I := 0 to SegmentPassCount - 1 do
                with CellPassesStorage[I] do
                  begin
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMachineIDIndex]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftTime]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftHeight]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftCCV]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftRMV]);
                    TestMDP:= ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMDP]);
                    if TestMDP <> MDP then
                      TestReadIndex := TestReadIndex;
          
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMaterialTemperature]);
                 end;
            {$ENDIF}
            */

            // if not VerifyCellPassEncoding then
            //   SIGLogMessage.PublishNoODS(Self, 'Segment VerifyCellPassEncoding failed', slmcMessage);
        }

        /// <summary>
        /// Takes a description of the cell passes to be placed into this segment and converts them into the
        /// internal compressed representation
        /// </summary>
        /// <param name="cellPasses"></param>
        public void SetState(CellPass[,][] cellPasses)
        {
            // Convert the supplied cell passes into the appropriate bit field arrays
            PerformEncodingStaticCompressedCache(cellPasses);
        }

        public CellPass[,][] GetState()
        {
            throw new NotImplementedException("Does not support GetState()");
        }

        /// <summary>
        /// Note that this information is immutable
        /// </summary>
        /// <returns></returns>
        public override bool IsImmutable() => true;

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this subgrid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        public void CalculateTotalPasses(out uint TotalPasses, out uint MaxPassCount)
        {
            SegmentTotalPassesCalculator.CalculateTotalPasses(this, out TotalPasses, out MaxPassCount);
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
        public void CalculatePassesBeforeTime(DateTime searchTime, out uint totalPasses, out uint maxPassCount)
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
      /// Sets the internal machine ID for the cell pass identifid by x & y spatial location and passNumber.
      /// </summary>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="passNumber"></param>
      /// <param name="internalMachineID"></param>
      public void SetInternalMachineID(uint X, uint Y, int passNumber, short internalMachineID)
      {
        throw new InvalidOperationException("Immutable cell pass segment.");
      }
    }
}
