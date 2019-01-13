export class DataRequestType {
}
// The list of 'display modes' that Raptor understands in the context of rendering WMS tiles and other operations
export var DisplayModeType;
(function (DisplayModeType) {
    // The elevation above the grid coordinate datum of the project coordinate system.
    DisplayModeType[DisplayModeType["Height"] = 0] = "Height";
    // Raw CCV/CMV (Caterpillar Compaction Value/Compaction Meter Value) values recorded by compaction machine systems.PFor this request special custom palette handling is required.
    DisplayModeType[DisplayModeType["CCV"] = 1] = "CCV";
    // CCV values expressed as a percentage between the raw measured CCV value and either the configured target CCV on the machine or a global override target CCV value.
    // For the palette please refer CCV sample.
    DisplayModeType[DisplayModeType["CCVPercent"] = 2] = "CCVPercent";
    // Radio latency reported by the machine systems, where the latency refers to the age of the RTK corrections induced by the radio network transmission latency between the RTK base station and the machine.
    DisplayModeType[DisplayModeType["Latency"] = 3] = "Latency";
    // The number of passes measured within the top most layer of material identified by layer analysis.
    DisplayModeType[DisplayModeType["PassCount"] = 4] = "PassCount";
    // Resonance meter value indicating how close the reactive force of the ground against the compactive energy being directed into it by the offset-mass vibrating drum is to causing the drum to bounce.
    DisplayModeType[DisplayModeType["RMV"] = 5] = "RMV";
    // The reported vibratory drum frequency on a compactor
    DisplayModeType[DisplayModeType["Frequency"] = 6] = "Frequency";
    // The reported vibratory drum amplitude on a compactor
    DisplayModeType[DisplayModeType["Amplitude"] = 7] = "Amplitude";
    // The cut or fill calculated from the comparison of two surfaces which may be a mixture of filtered machine originated production data and design surfaces. Design surface must be specified for the request.
    DisplayModeType[DisplayModeType["CutFill"] = 8] = "CutFill";
    // The reported soil moisture content from a moisture sensor on a soil compactor
    DisplayModeType[DisplayModeType["Moisture"] = 9] = "Moisture";
    // Analysed summary temperature information from recorded temperatures values from asphalt compactors.
    DisplayModeType[DisplayModeType["TemperatureSummary"] = 10] = "TemperatureSummary";
    // The reported GPSMode values from a machine
    DisplayModeType[DisplayModeType["GPSMode"] = 11] = "GPSMode";
    // Analysed raw CCV summary information from recorded compaction values from asphalt and soil compactors.
    DisplayModeType[DisplayModeType["CCVSummary"] = 12] = "CCVSummary";
    // Analysed raw CCV percentage summary information from recorded compaction values from asphalt and soil compactors.
    DisplayModeType[DisplayModeType["CCVPercentSummary"] = 13] = "CCVPercentSummary";
    // Analysed passcount summary information from asphalt and soil compactors.
    DisplayModeType[DisplayModeType["PassCountSummary"] = 14] = "PassCountSummary";
    // Information indication only where data exists within a project.
    DisplayModeType[DisplayModeType["CompactionCoverage"] = 15] = "CompactionCoverage";
    // Information indicating where in the project volume calculations occurred and in which areas there was no volumetric difference between the comparative surfaces.
    DisplayModeType[DisplayModeType["VolumeCoverage"] = 16] = "VolumeCoverage";
    // Raw Machine Drive Power values recorded by compaction machine systems
    // For the palette please refer CCV sample.
    DisplayModeType[DisplayModeType["MDP"] = 17] = "MDP";
    // MDP values expressed as a percentage between the raw measured MDP value and either the configured target MDP on the machine or a global override target CCV value.
    // For the palette please refer CCV sample.
    DisplayModeType[DisplayModeType["MDPSummary"] = 18] = "MDPSummary";
    // Analysed raw MDP summary information from recorded compaction values from asphalt and soil compactors.
    DisplayModeType[DisplayModeType["MDPPercent"] = 19] = "MDPPercent";
    // Analysed raw MDP percentage summary information from recorded compaction values from asphalt and soil compactors.
    DisplayModeType[DisplayModeType["MDPPercentSummary"] = 20] = "MDPPercentSummary";
    // An analysis of a cell in terms of the layers derived from profile analysis of information within it
    DisplayModeType[DisplayModeType["CellProfile"] = 21] = "CellProfile";
    // An analysis of a cell in terms of the layers derived from profile analysis of information within it, and the cell passes contained in the analysed layers
    DisplayModeType[DisplayModeType["CellPasses"] = 22] = "CellPasses";
    // Machine Speed valus recorded by compaction machine systems
    // For the palette please refer CCV sample.
    DisplayModeType[DisplayModeType["MachineSpeed"] = 23] = "MachineSpeed";
    // The CCV percent change calculates change of the CCV in % between current and previous CCV % over target. Normal filtering rules are applied.
    DisplayModeType[DisplayModeType["CCVPercentChange"] = 24] = "CCVPercentChange";
    // Target thickness summary overlay. Renders cells with three colors - above target, within target, below target. Target value shall be specified in the request.
    DisplayModeType[DisplayModeType["TargetThicknessSummary"] = 25] = "TargetThicknessSummary";
    // Target speed summary overlay. Renders cells with three colors - over target range, within target range, lower target range. Target range values shall be specified in the request.
    DisplayModeType[DisplayModeType["TargetSpeedSummary"] = 26] = "TargetSpeedSummary";
    /// The CCV change calculates change of the CCV in % between current and previous CCV absolute values. Normal filtering rules are applied.
    DisplayModeType[DisplayModeType["CMVChange"] = 27] = "CMVChange";
    // Raw CCA (Caterpillar Compaction Algorithm) values recorded by Landfill compaction machine systems. 
    // The proprietary Cat Compaction Algorithm is a dynamic pass counting and lift thickness detection feature available in GCS900 for Cat 8x6 K series Landfill Compactors only.
    DisplayModeType[DisplayModeType["CCA"] = 28] = "CCA";
    // Analysed raw CCA summary information from recorded CCA values Landfill compactors.
    DisplayModeType[DisplayModeType["CCASummary"] = 29] = "CCASummary";
    // Raw temperature information from recorded temperatures values from asphalt compactors.
    DisplayModeType[DisplayModeType["TemperatureDetail"] = 30] = "TemperatureDetail";
    // 3D terrain for map
    DisplayModeType[DisplayModeType["Terrain3D"] = 31] = "Terrain3D";
    // 3D design for map
    DisplayModeType[DisplayModeType["Design3D"] = 32] = "Design3D";
})(DisplayModeType || (DisplayModeType = {}));
//# sourceMappingURL=fetch-data-model.js.map