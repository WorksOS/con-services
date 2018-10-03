using System;

namespace VSS.TRex.Types
{
    /// <summary>
    /// DisplayMode represents the displayable rendering modes supported by the TRex tile rendering engine
    /// </summary>
    [Serializable]
    public enum DisplayMode
    {
        Height,
        CCV,
        CCVPercent,
        Latency,
        PassCount,
        RMV,
        Frequency,
        Amplitude,
        CutFill,
        Moisture,
        TemperatureSummary,
        GPSMode,
        CCVSummary,
        CCVPercentSummary,   // This is a synthetic display mode for CCV summary
        PassCountSummary,    // This is a synthetic display mode for Pass Count summary
        CompactionCoverage,  // This ia a synthetic display mode for Compaction Coverage
        VolumeCoverage,      // This is a synthetic display mode for Volumes Coverage
        MDP,
        MDPSummary,
        MDPPercent,
        MDPPercentSummary,   // This is a synthetic display mode for MDP summary
        CellProfile,
        CellPasses,
        MachineSpeed,
        CCVPercentChange,    //This calculates Percent of Percent change of CCV over Target
        TargetThicknessSummary, //Renders tiles with thickness between lifts above, below or equal target values
        TargetSpeedSummary, //Renders SPeed summary looking through all cell passes for a cell
        CCVChange, //Renders CCV change in absolute values (compared to CCV percent change)
        CCA,
        CCASummary,
        TemperatureDetail,
        ThreeDTerrain,
        ThreeDDesign
  }
}
