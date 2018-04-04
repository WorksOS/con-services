using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.Filters
{
    /// <summary>
    /// Contains a filtered cell pass including the events and target values relevant to the 
    /// cell pass at the time it was measured
    /// </summary>
    public struct FilteredPassData
    {
        public byte MachineType;   // Derived from the machine ID in the FilteredPass record

        public CellPass FilteredPass;
        public CellTargets TargetValues;
        public CellEvents EventValues;

        /// <summary>
        /// Initialise all state to null
        /// </summary>
        public void Clear()
        {
            MachineType = 0;
            FilteredPass.Clear();
            TargetValues.Clear();
            EventValues.Clear();
        }

        /// <summary>
        /// Copy the state of another FilteredPassData instance to this one
        /// </summary>
        /// <param name="source"></param>
        public void Assign(FilteredPassData source)
        {
            MachineType = source.MachineType;
            FilteredPass.Assign(source.FilteredPass);
            TargetValues.Assign(source.TargetValues);
            EventValues.Assign(source.EventValues);
        }
    }
}
