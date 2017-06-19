using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Compression;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// A wrapper for a subgrid containing all information related to the 'latest kown' information related to each cell.
    /// This includes 'existence' information which indicates if the cell in question has any cell passes recorded for it.
    /// </summary>
    public class SubGridCellLatestPassDataWrapperBase 
    {
        /// <summary>
        /// The existence map detailed which cells have pass data recorded for them
        /// </summary>
        public SubGridTreeBitmapSubGridBits PassDataExistanceMap { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

        public SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

        public bool HasCCVData => true;
        public bool HasRMVData => true;
        public bool HasFrequencyData => true;
        public bool HasAmplitudeData => true;
        public bool HasGPSModeData => true;
        public bool HasTemperatureData => true;
        public bool HasMDPData => true;
        public bool HasCCAData => true;

        public SubGridCellLatestPassDataWrapperBase()
        {
            ClearPasses();
        }

        /// <summary>
        /// Clear all latest information for the subgrid
        /// </summary>
        public void Clear()
        {
            PassDataExistanceMap.Clear();

            CCVValuesAreFromLastPass.Clear();
            RMVValuesAreFromLastPass.Clear();
            FrequencyValuesAreFromLastPass.Clear();
            AmplitudeValuesAreFromLastPass.Clear();
            GPSModeValuesAreFromLatestCellPass.Clear();
            TemperatureValuesAreFromLastPass.Clear();
            MDPValuesAreFromLastPass.Clear();
            CCAValuesAreFromLastPass.Clear();

            ClearPasses();
        }

        /// <summary>
        /// Clear all latest information for the subgrid
        /// </summary>
        public virtual void ClearPasses()
        {
        }

        /// <summary>
        /// Copies the flags information from the source latest cell pass wrapper to this one
        /// </summary>
        /// <param name="Source"></param>
        public void AssignValuesFromLastPassFlags(SubGridCellLatestPassDataWrapperBase Source)
        {
            CCVValuesAreFromLastPass.Assign(Source.CCVValuesAreFromLastPass);
            RMVValuesAreFromLastPass.Assign(Source.RMVValuesAreFromLastPass);
            FrequencyValuesAreFromLastPass.Assign(Source.FrequencyValuesAreFromLastPass);
            AmplitudeValuesAreFromLastPass.Assign(Source.AmplitudeValuesAreFromLastPass);
            GPSModeValuesAreFromLatestCellPass.Assign(Source.GPSModeValuesAreFromLatestCellPass);
            TemperatureValuesAreFromLastPass.Assign(Source.TemperatureValuesAreFromLastPass);
            MDPValuesAreFromLastPass.Assign(Source.MDPValuesAreFromLastPass);
            CCAValuesAreFromLastPass.Assign(Source.CCAValuesAreFromLastPass);
        }
    }

    public class SubGridCellLatestPassDataWrapper_NonStatic : SubGridCellLatestPassDataWrapperBase
    {
        /// <summary>
        /// The array of 32x32 cells containing a cell pass representing the latest known values for a variety of cell attributes
        /// </summary>
        public CellPass[,] PassData = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        /// <summary>
        /// Provides the 'NonStatic' behaviour for clearing the passes in the latest pass information
        /// </summary>
        public override void ClearPasses()
        {
            base.ClearPasses();

            SubGridUtilities.SubGridDimensionalIterator((x, y) => PassData[x, y].Clear());
        }
    }

    public class SubGridCellLatestPassDataWrapper_NonStaticCompressed : SubGridCellLatestPassDataWrapperBase
    {
        DateTime FirstRealCellPassTime;

        // BF_CellPasses contains all the cell pass information for the segment (read in via
        // the transient CellPassesStorage reference and then encoded into the cache format)
        BitFieldArray BF_CellPasses;

        private struct EncodedFieldDescriptorsStruct
        {
            public EncodedBitFieldDescriptor Time;
            public EncodedBitFieldDescriptor Height;
            public EncodedBitFieldDescriptor CCV;
            public EncodedBitFieldDescriptor RMV;
            public EncodedBitFieldDescriptor MDP;
            public EncodedBitFieldDescriptor MaterialTemperature;
            public EncodedBitFieldDescriptor CCA;

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

            public void Write(BinaryWriter writer)
            {
                Time.Write(writer);
                Height.Write(writer);
                CCV.Write(writer);
                RMV.Write(writer);
                MDP.Write(writer);
                MaterialTemperature.Write(writer);
                CCA.Write(writer);
            }

            public void Read(BinaryReader reader)
            {
                Time.Read(reader);
                Height.Read(reader);
                CCV.Read(reader);
                RMV.Read(reader);
                MDP.Read(reader);
                MaterialTemperature.Read(reader);
                CCA.Read(reader);
            }

            public void CalculateTotalOffsetBits(ref int NumBitsPerCellPass)
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

        EncodedFieldDescriptorsStruct EncodedFieldDescriptors = new EncodedFieldDescriptorsStruct();

        int NumBitsPerCellPass;

        public SubGridCellLatestPassDataWrapper_NonStaticCompressed()
        {
            EncodedFieldDescriptors.Init();
        }

        // PerformEncodingForInternalCache converts the structure of the cell passes and
        // other information held into a more compact form to maximise
        // the amount of data that can be placed into the given cache memory limit.
        public void PerformEncodingForInternalCache(CellPass[,] cellPasses, long LatestCellPassDataSize, long CellPassStacksDataSize)
        {
            // Given the value range for each attribute, calculate the number of bits required to store the values.
            EncodedFieldDescriptors.Init();

            // Compute the time of the earliest real cell pass within the latest cell passes
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
            EncodedFieldDescriptors.CalculateTotalOffsetBits(ref NumBitsPerCellPass);

            // Create the bit field arrays to contain the segment call pass index & count plus passes.
            // Copy the call passes themselves into BF
            BitFieldArrayRecordsDescriptor[] recordDescriptors = new BitFieldArrayRecordsDescriptor[]
            {
                new BitFieldArrayRecordsDescriptor()
                {
                    NumRecords = (int)SubGridTree.SubGridTreeCellsPerSubgrid,
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
                };
            }
            finally
            {
                BF_CellPasses.StreamWriteEnd();
            }
        }

        // ReadHeight will read the height from the latest cell identified by the Row and Col
        public float ReadHeight(int Col, int Row)
        {
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.Height.OffsetBits;
            int IntegerHeight = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Height);
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
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.CCV.OffsetBits;
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
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.RMV.OffsetBits;
            return (short)BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.RMV);
        }

        // ReadMDP will read the MDP from the latest cell identified by the Row and Col
        public short ReadMDP(int Col, int Row)
        {
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.MDP.OffsetBits;
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
            int BitLocation = (((Col * SubGridTree.SubGridTreeDimension) + Row) * NumBitsPerCellPass) + EncodedFieldDescriptors.MaterialTemperature.OffsetBits;
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

        // ReadAmplitude will read the Amplitude from the latest cell identified by the Row and Col
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

            CellPass Result = new CellPass();

            Result.MachineID = -1; // No machine IDs supported in latest cell pass data.

            int IntegerTime = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Time);
            Result.Time = IntegerTime == EncodedFieldDescriptors.Time.NativeNullValue ? DateTime.MinValue : FirstRealCellPassTime.AddSeconds(IntegerTime);

            int IntegerHeight = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Height);
            Result.Height = IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : IntegerHeight / 1000;

            Result.CCV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCV));
            Result.RMV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RMV));
            Result.MDP = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MDP));
            Result.MaterialTemperature = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MaterialTemperature));
            Result.CCA = (byte)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCA));

            return Result;
        }

        public void Read(BinaryReader reader)
        {
            Clear();

            PassDataExistanceMap.Read(reader);
            CCVValuesAreFromLastPass.Read(reader);
            RMVValuesAreFromLastPass.Read(reader);
            FrequencyValuesAreFromLastPass.Read(reader);
            AmplitudeValuesAreFromLastPass.Read(reader);
            GPSModeValuesAreFromLatestCellPass.Read(reader);
            TemperatureValuesAreFromLastPass.Read(reader);
            MDPValuesAreFromLastPass.Read(reader);
            CCAValuesAreFromLastPass.Read(reader);

            FirstRealCellPassTime = DateTime.FromBinary(reader.ReadInt64());

            BF_CellPasses.Read(reader);
            EncodedFieldDescriptors.Read(reader);

            NumBitsPerCellPass = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            PassDataExistanceMap.Write(writer);
            CCVValuesAreFromLastPass.Write(writer);
            RMVValuesAreFromLastPass.Write(writer);
            FrequencyValuesAreFromLastPass.Write(writer);
            AmplitudeValuesAreFromLastPass.Write(writer);
            GPSModeValuesAreFromLatestCellPass.Write(writer);
            TemperatureValuesAreFromLastPass.Write(writer);
            MDPValuesAreFromLastPass.Write(writer);
            CCAValuesAreFromLastPass.Write(writer);

            writer.Write(FirstRealCellPassTime.ToBinary());

            BF_CellPasses.Write(writer);

            EncodedFieldDescriptors.Write(writer);

            writer.Write(NumBitsPerCellPass);
        }
    }
}
