namespace VSS.Productivity3D.Models.Enums
{
    /// <summary>
    /// The list of 'display modes' that Raptor understans in the context of rendering WMS tiles and other operations
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// The elevation above the grid coordinate datum of the project coordinate system.
        /// </summary>
        Height,

        /// <summary>
        /// Raw CCV/CMV (Caterpillar Compaction Value/Compaction Meter Value) values recorded by compaction machine systems.PFor this request special custom palette handling is required. 
        /// The last range interval for the values in color palatte is handled by the the last record in color palette (not the previous one). The last value in color palette overrides previous value. So you should specify 
        /// 5 colors for the request i.e.: 
        ///  {
        ///"color": 16711680, //will be applied for the values between 0 and 60
        ///"value": 0
      ///},
      ///{
        ///"color": 65280, //will be applied for the values between 60 and 80
        ///"value": 60
      ///},
      ///{
        ///"color": 155, //will be applied for the values between 80 and 110
        ///"value": 80
      ///},
      ///{
        ///"color": 255, //will be applied for the values equal 110 
        ///"value": 110
      ///},
      ///{
        ///"color": 0, //will be applied for the values above 111. This is required for correct handling ABOVE condition by Raptor
        ///"value": 111
        ///},
        /// </summary>
        CCV,

        /// <summary>
        /// CCV values expressed as a percentage between the raw measured CCV value and either the configured target CCV on the machine or a global override target CCV value.
        /// For the palette please refer CCV sample.
        /// </summary>
        CCVPercent,

        /// <summary>
        /// Radio latency reported by the machine systems, where the latency refers to the age of the RTK corrections induced by the radio network transmission latency between the RTK base station and the machine.
        /// </summary>
        Latency,

        /// <summary>
        /// The number of passes measured within the top most layer of material identified by layer analysis.
        /// </summary>
        PassCount,

        /// <summary>
        /// Resonance meter value indicating how close the reactive force of the ground against the compactive energy being directed into it by the offset-mass vibrating drum is to causing the drum to bounce.
        /// </summary>
        RMV,

        /// <summary>
        /// The reported vibratory drum frequency on a compactor
        /// </summary>
        Frequency,

        /// <summary>
        /// The reported vibratory drum amplitude on a compactor
        /// </summary>
        Amplitude,

        /// <summary>
        /// The cut or fill calculated from the comparison of two surfaces which may be a mixture of filtered machine originated production data and design surfaces. Design surface must be specified for the request.
        /// </summary>
        CutFill,

        /// <summary>
        /// The reported soil moisture content from a moisture sensor on a soil compactor
        /// </summary>
        Moisture,

        /// <summary>
        /// Analysed summary temperature information from recorded temperatures values from asphalt compactors.
        /// </summary>
        TemperatureSummary,

        /// <summary>
        /// The reported GPSMode values from a machine
        /// </summary>
        GPSMode,

        /// <summary>
        /// Analysed raw CCV summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        CCVSummary,

        /// <summary>
        /// Analysed raw CCV percentage summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        CCVPercentSummary, // This is a synthetic display mode for CCV summary

        /// <summary>
        /// Analysed passcount summary information from asphalt and soil compactors.
        /// </summary>
        PassCountSummary, // This is a synthetic display mode for Pass Count summary

        /// <summary>
        /// Information indication only where data exists within a project.
        /// </summary>
        CompactionCoverage, // This ia a synthetic display mode for Compaction Coverage

        /// <summary>
        /// Information indicating where in the project volume calculations occurred and in which areas there was no volumetric difference between the comparative surfaces.
        /// </summary>
        VolumeCoverage, // This is a synthetic display mode for Volumes Coverage

        /// <summary>
        /// Raw Machine Drive Power values recorded by compaction machine systems
        /// For the palette please refer CCV sample.
        /// </summary>
        MDP,

        /// <summary>
        /// MDP values expressed as a percentage between the raw measured MDP value and either the configured target MDP on the machine or a global override target CCV value.
        /// For the palette please refer CCV sample.
        /// </summary>
        MDPSummary,

        /// <summary>
        /// Analysed raw MDP summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        MDPPercent,

        /// <summary>
        /// Analysed raw MDP percentage summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        MDPPercentSummary, // This is a synthetic display mode for MDP summary

        /// <summary>
        /// An analysis of a cell in terms of the layers derived from profile analysis of information within it
        /// </summary>
        CellProfile,

        /// <summary>
        /// An analysis of a cell in terms of the layers derived from profile analysis of information within it, and the cell passes contained in the analysed layers
        /// </summary>
        CellPasses,

        /// <summary>
        /// Machine Speed valus recorded by compaction machine systems
        /// For the palette please refer CCV sample.
        /// </summary>
        MachineSpeed,
        /// <summary>
        /// The CCV percent change calculates change of the CCV in % between current and previous CCV % over target. Normal filtering rules are applied.
        /// </summary>
        CCVPercentChange,
        /// <summary>
        /// Target thickness summary overlay. Renders cells with three colors - above target, within target, below target. Target value shall be specified in the request.
        /// </summary>
        TargetThicknessSummary,
        /// <summary>
        /// Target speed summary overlay. Renders cells with three colors - over target range, within target range, lower target range. Target range values shall be specified in the request.
        /// </summary>
        TargetSpeedSummary,
        /// <summary>
        /// The CCV change calculates change of the CCV in % between current and previous CCV absolute values. Normal filtering rules are applied.
        /// </summary>
        CMVChange,
        /// <summary>
        /// Raw CCA (Caterpillar Compaction Algorithm) values recorded by Landfill compaction machine systems. 
        /// The proprietary Cat Compaction Algorithm is a dynamic pass counting and lift thickness detection feature available in GCS900 for Cat 8x6 K series Landfill Compactors only.
        /// </summary>
        CCA,
        /// <summary>
        /// Analysed raw CCA summary information from recorded CCA values Landfill compactors.
        /// </summary>
        CCASummary
    }
}