using System.IO;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
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
        public SubGridTreeBitmapSubGridBits PassDataExistanceMap { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        public SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

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
        private void AssignValuesFromLastPassFlags(ISubGridCellLatestPassDataWrapper Source)
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

        public void Assign(ISubGridCellLatestPassDataWrapper Source)
        {
            PassDataExistanceMap.Assign(Source.PassDataExistanceMap);

            AssignValuesFromLastPassFlags(Source);
        }

        public virtual void Read(BinaryReader reader, byte[] buffer)
        {
            // Read in the latest call pass flags
            PassDataExistanceMap.Read(reader, buffer);
            CCVValuesAreFromLastPass.Read(reader, buffer);
            RMVValuesAreFromLastPass.Read(reader, buffer);
            FrequencyValuesAreFromLastPass.Read(reader, buffer);
            GPSModeValuesAreFromLatestCellPass.Read(reader, buffer);
            AmplitudeValuesAreFromLastPass.Read(reader, buffer);
            TemperatureValuesAreFromLastPass.Read(reader, buffer);
            MDPValuesAreFromLastPass.Read(reader, buffer);
            CCAValuesAreFromLastPass.Read(reader, buffer);
        }

        public virtual void Write(BinaryWriter writer, byte [] buffer)
        {
            // Write out the latest call pass flags
            PassDataExistanceMap.Write(writer, buffer);
            CCVValuesAreFromLastPass.Write(writer, buffer);
            RMVValuesAreFromLastPass.Write(writer, buffer);
            FrequencyValuesAreFromLastPass.Write(writer, buffer);
            GPSModeValuesAreFromLatestCellPass.Write(writer, buffer);
            AmplitudeValuesAreFromLastPass.Write(writer, buffer);
            TemperatureValuesAreFromLastPass.Write(writer, buffer);
            MDPValuesAreFromLastPass.Write(writer, buffer);
            CCAValuesAreFromLastPass.Write(writer, buffer);
        }

        /// <summary>
        /// Is the information contained in this wrapper mutable, or has is been converted to an immutable form from the base mutable data
        /// </summary>
        /// <returns></returns>
        public virtual bool IsImmutable() => false;
    }
}
