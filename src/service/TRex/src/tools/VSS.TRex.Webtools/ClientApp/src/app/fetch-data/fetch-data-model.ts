export class DataRequestType {
  item1: number;
  item2: string;
}

// The list of 'display modes' that Raptor understands in the context of rendering WMS tiles and other operations
export enum DisplayModeType {
  // The elevation above the grid coordinate datum of the project coordinate system.
  Height,

  // Raw CCV/CMV (Caterpillar Compaction Value/Compaction Meter Value) values recorded by compaction machine systems.PFor this request special custom palette handling is required.
  CCV,

  // CCV values expressed as a percentage between the raw measured CCV value and either the configured target CCV on the machine or a global override target CCV value.
  // For the palette please refer CCV sample.
  CCVPercent,

  // Radio latency reported by the machine systems, where the latency refers to the age of the RTK corrections induced by the radio network transmission latency between the RTK base station and the machine.
  Latency,

  // The number of passes measured within the top most layer of material identified by layer analysis.
  PassCount,

  // Resonance meter value indicating how close the reactive force of the ground against the compactive energy being directed into it by the offset-mass vibrating drum is to causing the drum to bounce.
  RMV,

  // The reported vibratory drum frequency on a compactor
  Frequency,

  // The reported vibratory drum amplitude on a compactor
  Amplitude,

  // The cut or fill calculated from the comparison of two surfaces which may be a mixture of filtered machine originated production data and design surfaces. Design surface must be specified for the request.
  CutFill,

  // The reported soil moisture content from a moisture sensor on a soil compactor
  Moisture,

  // Analysed summary temperature information from recorded temperatures values from asphalt compactors.
  TemperatureSummary,

  // The reported GPSMode values from a machine
  GPSMode,

  // Analysed raw CCV summary information from recorded compaction values from asphalt and soil compactors.
  CCVSummary,

  // Analysed raw CCV percentage summary information from recorded compaction values from asphalt and soil compactors.
  CCVPercentSummary, // This is a synthetic display mode for CCV summary

  // Analysed passcount summary information from asphalt and soil compactors.
  PassCountSummary, // This is a synthetic display mode for Pass Count summary

  // Information indication only where data exists within a project.
  CompactionCoverage, // This ia a synthetic display mode for Compaction Coverage

  // Information indicating where in the project volume calculations occurred and in which areas there was no volumetric difference between the comparative surfaces.
  VolumeCoverage, // This is a synthetic display mode for Volumes Coverage

  // Raw Machine Drive Power values recorded by compaction machine systems
  // For the palette please refer CCV sample.
  MDP,

  // MDP values expressed as a percentage between the raw measured MDP value and either the configured target MDP on the machine or a global override target CCV value.
  // For the palette please refer CCV sample.
  MDPSummary,

  // Analysed raw MDP summary information from recorded compaction values from asphalt and soil compactors.
  MDPPercent,

  // Analysed raw MDP percentage summary information from recorded compaction values from asphalt and soil compactors.
  MDPPercentSummary, // This is a synthetic display mode for MDP summary

  // An analysis of a cell in terms of the layers derived from profile analysis of information within it
  CellProfile,

  // An analysis of a cell in terms of the layers derived from profile analysis of information within it, and the cell passes contained in the analysed layers
  CellPasses,

  // Machine Speed valus recorded by compaction machine systems
  // For the palette please refer CCV sample.
  MachineSpeed,

  // The CCV percent change calculates change of the CCV in % between current and previous CCV % over target. Normal filtering rules are applied.
  CCVPercentChange,

  // Target thickness summary overlay. Renders cells with three colors - above target, within target, below target. Target value shall be specified in the request.
  TargetThicknessSummary,

  // Target speed summary overlay. Renders cells with three colors - over target range, within target range, lower target range. Target range values shall be specified in the request.
  TargetSpeedSummary,

  /// The CCV change calculates change of the CCV in % between current and previous CCV absolute values. Normal filtering rules are applied.
  CMVChange,

  // Raw CCA (Caterpillar Compaction Algorithm) values recorded by Landfill compaction machine systems. 
  // The proprietary Cat Compaction Algorithm is a dynamic pass counting and lift thickness detection feature available in GCS900 for Cat 8x6 K series Landfill Compactors only.
  CCA,

  // Analysed raw CCA summary information from recorded CCA values Landfill compactors.
  CCASummary,

  // Raw temperature information from recorded temperatures values from asphalt compactors.
  TemperatureDetail,

  // 3D terrain for map
  Terrain3D,

  // 3D design for map
  Design3D
}
