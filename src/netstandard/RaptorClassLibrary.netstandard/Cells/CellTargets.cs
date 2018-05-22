using VSS.TRex.Common;

namespace VSS.TRex.Cells
{
    /// <summary>
    /// Stores the values of various 'target' values that were active on the machine at the time the pass information was captured.
    /// </summary>
    public struct CellTargets
    {
        /// <summary>
        /// Null value for the CCA target configured at the time of cell pass measurement
        /// </summary>
        public const short NullCCATarget = short.MaxValue;

        /// <summary>
        /// Null value for the pass count target configured at the time of cell pass measurement
        /// </summary>
        public const ushort NullPassCountTarget = 0;

        /// <summary>
        /// Null value for the override target lift thickness value to be used in place of the target lift thickness value
        /// configured at the time of cell pass measurement
        /// </summary>
        public const float NullOverridingTargetLiftThicknessValue = Consts.NullSingle;

        /// <summary>
        /// Target Compaction Meter Value at the time a cell pass was recorded
        /// </summary>
        public short TargetCCV { get; set; }

        /// <summary>
        ///  Target Machien Drive Power at the time a cell pass was recorded
        /// </summary>
        public short TargetMDP { get; set; }

        /// <summary>
        /// Target material layer thickness at the time a cell pass was recorded
        /// </summary>
        public float TargetThickness { get; set; }

        /// <summary>
        /// Target machine pass count at the time a cell pass was recorded
        /// </summary>
        public ushort TargetPassCount { get; set; }

        /// <summary>
        /// Target minimum temperature sensor warning level at the time a cell pass was recorded
        /// </summary>
        public ushort TempWarningLevelMin { get; set; }

        /// <summary>
        /// Target maximum temperature sensor warning level at the time a cell pass was recorded
        /// </summary>
        public ushort TempWarningLevelMax { get; set; }

        /// <summary>
        /// Target Caterpillar Compaction algorithm value at the time a cell pass was recorded
        /// </summary>
        public short TargetCCA { get; set; }

        /// <summary>
        /// Set all state in this structure to null values
        /// </summary>
        public void Clear()
        {
            TargetCCV = CellPass.NullCCV;
            TargetMDP = CellPass.NullMDP;
            TargetThickness = NullOverridingTargetLiftThicknessValue;
            TargetPassCount = NullPassCountTarget;
            TempWarningLevelMin = CellPass.NullMaterialTemperatureValue;
            TempWarningLevelMax = CellPass.NullMaterialTemperatureValue;
            TargetCCA = NullCCATarget;
        }

        /// <summary>
        /// Assigns the contents of a CellTargets to this CellTargets
        /// </summary>
        /// <param name="source"></param>
        public void Assign(CellTargets source)
        {
            TargetCCA = source.TargetCCA;
            TargetCCV = source.TargetCCV;
            TargetMDP = source.TargetMDP;
            TargetPassCount = source.TargetPassCount;
            TargetThickness = source.TargetThickness;
            TempWarningLevelMax = source.TempWarningLevelMax;
            TempWarningLevelMin = source.TempWarningLevelMin;
        }

        //Procedure ReadFromStream(const Stream : TStream);
        //Procedure WriteToStream(const Stream : TStream);
    }
}
