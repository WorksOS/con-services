using System.IO;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// A wrapper for a sub grid containing all information related to the 'latest known' information related to each cell.
    /// This includes 'existence' information which indicates if the cell in question has any cell passes recorded for it.
    /// </summary>
    public abstract class SubGridCellLatestPassDataWrapperBase
    {
        /// <summary>
        /// The existence map detailed which cells have pass data recorded for them
        /// </summary>
        public SubGridTreeBitmapSubGridBits PassDataExistenceMap { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        public SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      /*
        public bool HasCCVData() => true;
        public bool HasRMVData() => true;
        public bool HasFrequencyData() => true;
        public bool HasAmplitudeData() => true;
        public bool HasGPSModeData() => true;
        public bool HasTemperatureData() => true;
        public bool HasMDPData() => true;
        public bool HasCCAData() => true;
*/
        public SubGridCellLatestPassDataWrapperBase()
        {
        }

        /// <summary>
        /// Clear all latest information for the sub grid
        /// </summary>
        public void Clear()
        {
            PassDataExistenceMap.Clear();

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
        /// Clear all latest information for the sub grid
        /// </summary>
        public virtual void ClearPasses()
        {
        }

        /// <summary>
        /// Copies the flags information from the source latest cell pass wrapper to this one
        /// </summary>
        /// <param name="Source"></param>
        public void AssignValuesFromLastPassFlags(ISubGridCellLatestPassDataWrapper Source)
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
            PassDataExistenceMap.Assign(Source.PassDataExistenceMap);

            AssignValuesFromLastPassFlags(Source);
        }

        public virtual void Read(BinaryReader reader)
        {
            // Read in the latest call pass flags
            PassDataExistenceMap.Read(reader);
            CCVValuesAreFromLastPass.Read(reader);
            RMVValuesAreFromLastPass.Read(reader);
            FrequencyValuesAreFromLastPass.Read(reader);
            GPSModeValuesAreFromLatestCellPass.Read(reader);
            AmplitudeValuesAreFromLastPass.Read(reader);
            TemperatureValuesAreFromLastPass.Read(reader);
            MDPValuesAreFromLastPass.Read(reader);
            CCAValuesAreFromLastPass.Read(reader);
        }

        public virtual void Write(BinaryWriter writer)
        {
            // Write out the latest call pass flags
            PassDataExistenceMap.Write(writer);
            CCVValuesAreFromLastPass.Write(writer);
            RMVValuesAreFromLastPass.Write(writer);
            FrequencyValuesAreFromLastPass.Write(writer);
            GPSModeValuesAreFromLatestCellPass.Write(writer);
            AmplitudeValuesAreFromLastPass.Write(writer);
            TemperatureValuesAreFromLastPass.Write(writer);
            MDPValuesAreFromLastPass.Write(writer);
            CCAValuesAreFromLastPass.Write(writer);
        }

        /// <summary>
        /// Is the information contained in this wrapper mutable, or has is been converted to an immutable form from the base mutable data
        /// </summary>
        /// <returns></returns>
        public virtual bool IsImmutable() => false;
    }
}
