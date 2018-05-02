using System;
using System.IO;
using System.Linq;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Compression;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellLatestPassDataWrapper_StaticCompressed : SubGridCellLatestPassDataWrapperBase, ISubGridCellLatestPassDataWrapper
    {
        private DateTime FirstRealCellPassTime;

        // BF_CellPasses contains all the cell pass information for the segment (read in via
        // the transient CellPassesStorage reference and then encoded into the cache format)
        private BitFieldArray BF_CellPasses;

        /// <summary>
        /// The set of field descriptors for the attribute being stored in the bit field array compressed form
        /// </summary>
        private struct EncodedFieldDescriptorsStruct
        {
            public EncodedBitFieldDescriptor Time;
            public EncodedBitFieldDescriptor Height;
            public EncodedBitFieldDescriptor CCV;
            public EncodedBitFieldDescriptor RMV;
            public EncodedBitFieldDescriptor MDP;
            public EncodedBitFieldDescriptor MaterialTemperature;
            public EncodedBitFieldDescriptor CCA;

            /// <summary>
            /// Initialise all descriptors
            /// </summary>
            public void Init()
            {
                Time.Init();
                Height.Init();
                CCV.Init();
                RMV.Init();
                MDP.Init();
                MaterialTemperature.Init();
                CCA.Init();
            }

            /// <summary>
            /// Serialise all descriptors to the supplied writer
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="buffer"></param>
            public void Write(BinaryWriter writer, byte [] buffer)
            {
                Time.Write(writer);
                Height.Write(writer);
                CCV.Write(writer);
                RMV.Write(writer);
                MDP.Write(writer);
                MaterialTemperature.Write(writer);
                CCA.Write(writer);
            }

            /// <summary>
            /// Deserialise all descriptors using the supplied reader
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="buffer"></param>
            public void Read(BinaryReader reader, byte [] buffer)
            {
                Time.Read(reader);
                Height.Read(reader);
                CCV.Read(reader);
                RMV.Read(reader);
                MDP.Read(reader);
                MaterialTemperature.Read(reader);
                CCA.Read(reader);
            }

            /// <summary>
            /// Calculate the chained offsets and numbers of requiredbits for each attribute being stored
            /// </summary>
            /// <param name="NumBitsPerCellPass"></param>
            public void CalculateTotalOffsetBits(out int NumBitsPerCellPass)
            {
                Time.OffsetBits = 0;
                Height.OffsetBits = (byte)(Time.OffsetBits + Time.RequiredBits);
                CCV.OffsetBits = (byte)(Height.OffsetBits + Height.RequiredBits);
                RMV.OffsetBits = (byte)(CCV.OffsetBits + CCV.RequiredBits);
                MDP.OffsetBits = (byte)(RMV.OffsetBits + RMV.RequiredBits);
                MaterialTemperature.OffsetBits = (byte)(MDP.OffsetBits + MDP.RequiredBits);
                CCA.OffsetBits = (byte)(MaterialTemperature.OffsetBits + MaterialTemperature.RequiredBits);

                // Calculate the total number of bits required and pass back
                NumBitsPerCellPass = CCA.OffsetBits + CCA.RequiredBits;
            }
        }

        /// <summary>
        /// Implement the last pass indexer from the interface.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellPass this[int x, int y]
        {
            get
            {
                return CellPass(x, y);
            }
            set
            {
                throw new NotSupportedException("Writing to individual last pass information is not supported in immutable representations");
            }
        }

        private EncodedFieldDescriptorsStruct EncodedFieldDescriptors; // = new EncodedFieldDescriptorsStruct();

        private int NumBitsPerCellPass;

        public SubGridCellLatestPassDataWrapper_StaticCompressed()
        {
            BF_CellPasses = new BitFieldArray();
            
            EncodedFieldDescriptors.Init();
        }

        // PerformEncodingForInternalCache converts the structure of the cell passes and
        // other information held into a more compact form to maximise
        // the amount of data that can be placed into the given cache memory limit.
        public void PerformEncodingForInternalCache(CellPass[,] cellPasses /*, long LatestCellPassDataSize, long CellPassStacksDataSize*/)
        {
            // Given the value range for each attribute, calculate the number of bits required to store the values.
            EncodedFieldDescriptors.Init();

            // Compute the time of the earliest real cell pass within the latest cell passes, and the elevation of the lowest recorded cell
            // passes in the latest cell passes
            FirstRealCellPassTime = DateTime.MaxValue;

            SubGridUtilities.SubGridDimensionalIterator((col, row) =>
            {
                DateTime time = cellPasses[col, row].Time;
                FirstRealCellPassTime = time != Cells.CellPass.NullTime && time < FirstRealCellPassTime ? time : FirstRealCellPassTime;
            });

            // For ease of management convert all the cell passes into a single list for the following operations
            CellPass[] allCellPassesArray = new CellPass[SubGridTree.SubGridTreeCellsPerSubgrid];
            int cellPassIndex = 0;

            SubGridUtilities.SubGridDimensionalIterator((col, row) => allCellPassesArray[cellPassIndex++] = cellPasses[col, row]);

            // Work out the value ranges of all the attributes and given the value range
            // for each attribute, calculate the number of bits required to store the values.

            // Note:
            // Time - based on the longword, second accurate times overriding the TDateTime times
            // Height - based on the longword, millimeter accurate elevations overriding the IEEE double elevations
            // GPSMode - take the least significant 4 bits of the GPSModeStore

            // Convert time and elevation value to offset values in the appropriate units
            // from the lowest values of those attributes. Reuse the existing fields in the
            // cell passes list to avoid having to allocate an extra memory block

            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedTime(x.Time, FirstRealCellPassTime)).ToArray(), 0xffffffff, 0, false, ref EncodedFieldDescriptors.Time);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedHeight(x.Height)).ToArray(), 0xffffffff, 0x7fffffff, true, ref EncodedFieldDescriptors.Height);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCV).ToArray(), 0xffffffff, Cells.CellPass.NullCCV, true, ref EncodedFieldDescriptors.CCV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.RMV).ToArray(), 0xffffffff, Cells.CellPass.NullRMV, true, ref EncodedFieldDescriptors.RMV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MDP).ToArray(), 0xffffffff, Cells.CellPass.NullMDP, true, ref EncodedFieldDescriptors.MDP);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MaterialTemperature).ToArray(), 0xffffffff, Cells.CellPass.NullMaterialTemp, true, ref EncodedFieldDescriptors.MaterialTemperature);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCA).ToArray(), 0xff, Cells.CellPass.NullCCA, true, ref EncodedFieldDescriptors.CCA);

            // Calculate the offset bit locations for the cell pass attributes
            EncodedFieldDescriptors.CalculateTotalOffsetBits(out NumBitsPerCellPass);

            // Create the bit field arrays to contain the segment call pass index & count plus passes.
            // Copy the call passes themselves into BF
            BitFieldArrayRecordsDescriptor[] recordDescriptors = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor()
                {
                    NumRecords = SubGridTree.SubGridTreeCellsPerSubgrid,
                    BitsPerRecord = NumBitsPerCellPass
                }
            };

            BF_CellPasses.Initialise(recordDescriptors);
            BF_CellPasses.StreamWriteStart();
            try
            {
                foreach (CellPass pass in allCellPassesArray)
                {
                    BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedTime(pass.Time, FirstRealCellPassTime), EncodedFieldDescriptors.Time);
                    BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedHeight(pass.Height), EncodedFieldDescriptors.Height);
                    BF_CellPasses.StreamWrite(pass.CCV, EncodedFieldDescriptors.CCV);
                    BF_CellPasses.StreamWrite(pass.RMV, EncodedFieldDescriptors.RMV);
                    BF_CellPasses.StreamWrite(pass.MDP, EncodedFieldDescriptors.MDP);
                    BF_CellPasses.StreamWrite(pass.MaterialTemperature, EncodedFieldDescriptors.MaterialTemperature);
                    BF_CellPasses.StreamWrite(pass.CCA, EncodedFieldDescriptors.CCA);
                }
            }
            finally
            {
                BF_CellPasses.StreamWriteEnd();
            }
        }

        /// <summary>
        /// ReadTime will read the time from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public DateTime ReadTime(int Col, int Row)
        {
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.Height.OffsetBits;

            long IntegerTime = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Time);
            return IntegerTime == EncodedFieldDescriptors.Time.NativeNullValue ? DateTime.MinValue : FirstRealCellPassTime.AddSeconds(IntegerTime);
        }

        /// <summary>
        /// ReadHeight will read the height from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public float ReadHeight(int Col, int Row)
        {
            int BitLocation = (Col * SubGridTree.SubGridTreeDimension + Row) * NumBitsPerCellPass + EncodedFieldDescriptors.Height.OffsetBits;
            float IntegerHeight = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Height);
            return IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : IntegerHeight / 1000;
        }

        /// <summary>
        /// ReadCCV will read the CCV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadCCV(int Col, int Row)
        {
            int BitLocation = (Col * SubGridTree.SubGridTreeDimension + Row) * NumBitsPerCellPass + EncodedFieldDescriptors.CCV.OffsetBits;
            return (short)BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.CCV);
        }

        /// <summary>
        /// ReadRMV will read the RMV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadRMV(int Col, int Row)
        {
            int BitLocation = (Col * SubGridTree.SubGridTreeDimension + Row) * NumBitsPerCellPass + EncodedFieldDescriptors.RMV.OffsetBits;
            return (short)BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.RMV);
        }

        /// <summary>
        /// ReadMDP will read the MDP from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadMDP(int Col, int Row)
        {
            int BitLocation = (Col * SubGridTree.SubGridTreeDimension + Row) * NumBitsPerCellPass + EncodedFieldDescriptors.MDP.OffsetBits;
            return (short)BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.MDP);
        }

        /// <summary>
        /// ReadTemperature will read the Temperature from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public ushort ReadTemperature(int Col, int Row)
        {
            int BitLocation = (Col * SubGridTree.SubGridTreeDimension + Row) * NumBitsPerCellPass + EncodedFieldDescriptors.MaterialTemperature.OffsetBits;
            return (ushort)BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.MaterialTemperature);
        }

        /// <summary>
        /// ReadFrequency will read the Frequency from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public ushort ReadFrequency(int Col, int Row)
        {
            return Cells.CellPass.NullFrequency;
        }

        /// <summary>
        /// ReadAmplitude will read the Amplitude from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public ushort ReadAmplitude(int Col, int Row)
        {
            return Cells.CellPass.NullAmplitude;
        }

        /// <summary>
        /// ReadGPSMode will read the GPSMode from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public GPSMode ReadGPSMode(int Col, int Row)
        {
            return GPSMode.NoGPS;
        }

        /// <summary>
        /// ReadCCA will read the CCA from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public byte ReadCCA(int Col, int Row)
        {
            return Cells.CellPass.NullCCA;
        }

        /// <summary>
        /// Returns the latest cell pass stored at the location given by Row & Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public CellPass CellPass(int Col, int Row)
        {
            return GetCellPass(Col, Row);
        }

        public CellPass GetCellPass(int Col, int Row)
        {
            // IMPORTANT: The fields read in this method must be read in the same order as they were written during encoding

            int CellPassBitLocation = ((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass;

            CellPass Result = new CellPass()
            {
                //MachineID = Cells.CellPass.NullMachineID // No machine IDs supported in latest cell pass data.
                InternalSiteModelMachineIndex = Cells.CellPass.NullInternalSiteModelMachineIndex // No machine IDs supported in latest cell pass data.
            };

            long IntegerTime = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Time);
            Result.Time = IntegerTime == EncodedFieldDescriptors.Time.NativeNullValue ? DateTime.MinValue : FirstRealCellPassTime.AddSeconds(IntegerTime);

            long IntegerHeight = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Height);
            Result.Height = IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : (float)(IntegerHeight / 1000.0);

            Result.CCV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCV));
            Result.RMV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RMV));
            Result.MDP = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MDP));
            Result.MaterialTemperature = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MaterialTemperature));
            Result.CCA = (byte)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCA));

            return Result;
        }

        public override void Read(BinaryReader reader, byte[] buffer)
        {
            Clear();

            base.Read(reader, buffer);

            FirstRealCellPassTime = DateTime.FromBinary(reader.ReadInt64());

            BF_CellPasses.Read(reader);

            EncodedFieldDescriptors.Read(reader, buffer);

            NumBitsPerCellPass = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            base.Write(writer, buffer);

            writer.Write(FirstRealCellPassTime.ToBinary());

            BF_CellPasses.Write(writer);

            EncodedFieldDescriptors.Write(writer, buffer);

            writer.Write(NumBitsPerCellPass);
        }

        /// <summary>
        /// Note that this information is immutable
        /// </summary>
        /// <returns></returns>
        public override bool IsImmutable() => true;
    }
}
