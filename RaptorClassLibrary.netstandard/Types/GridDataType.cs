using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// Types of grid data that client leaf subgrids may represent. These largely map to the 
    /// attribute and processing vectors supported by the Raptor processing engine with th eexception 
    /// of All which represents fully attributed call passes. This type is only used on server side
    /// full stack cell pass subgrids.
    /// </summary>
    [Serializable]
    public enum GridDataType
    {
        All = 0x00000000, // Could possilbly remove this from the enumeration
        CCV = 0x00000001,
        Height = 0x00000002,
        Latency = 0x00000003,
        PassCount = 0x00000004,
        Frequency = 0x00000005,
        Amplitude = 0x00000006,
        Moisture = 0x00000007,
        Temperature = 0x00000008,
        RMV = 0x00000009,
        CCVPercent = 0x0000000B,
        GPSMode = 0x0000000A,
        SimpleVolumeOverlay = 0x0000000C,
        HeightAndTime = 0x0000000D,
        CompositeHeights = 0x0000000E,
        MDP = 0x0000000F,
        MDPPercent = 0x00000010,
        CellProfile = 0x00000011,
        CellPasses = 0x00000012,
        MachineSpeed = 0x00000013,
        CCVPercentChange = 0x00000014,
        MachineSpeedTarget = 0x00000015,
        CCVPercentChangeIgnoredTopNullValue = 0x00000016,
        CCA = 0x00000017,
        CCAPercent = 0x00000018,
        TemperatureDetail = 0x00000019,
        CutFill = 0x00000020
  }

    public static class GridDataFromModeConverter
    {
       public static GridDataType Convert(DisplayMode Mode)
        {
            switch (Mode)
            {
                case DisplayMode.Height: return GridDataType.Height;
                case DisplayMode.CCV: return GridDataType.CCV;
                case DisplayMode.CCVPercent: return GridDataType.CCV;
                case DisplayMode.CCVSummary: return GridDataType.CCV;
                case DisplayMode.CCVPercentSummary: return GridDataType.CCV;
                case DisplayMode.Latency: return GridDataType.Latency;
                case DisplayMode.PassCount: return GridDataType.PassCount;
                case DisplayMode.PassCountSummary: return GridDataType.PassCount;
                case DisplayMode.Frequency: return GridDataType.Frequency;
                case DisplayMode.Amplitude: return GridDataType.Amplitude;
                case DisplayMode.RMV: return GridDataType.RMV;
                case DisplayMode.Moisture: return GridDataType.Moisture;
                case DisplayMode.TemperatureSummary: return GridDataType.Temperature;
                case DisplayMode.CutFill: return GridDataType.Height;
                case DisplayMode.GPSMode: return GridDataType.GPSMode;
                case DisplayMode.CompactionCoverage: return GridDataType.Height;
                case DisplayMode.VolumeCoverage: return GridDataType.SimpleVolumeOverlay;
                case DisplayMode.MDP: return GridDataType.MDP;
                case DisplayMode.MDPSummary: return GridDataType.MDP;
                case DisplayMode.MDPPercent: return GridDataType.MDP;
                case DisplayMode.MDPPercentSummary: return GridDataType.MDP;
                case DisplayMode.CellProfile: return GridDataType.CellProfile;
                case DisplayMode.CellPasses: return GridDataType.CellPasses;
                case DisplayMode.MachineSpeed: return GridDataType.MachineSpeed;
                case DisplayMode.CCVPercentChange: return GridDataType.CCVPercentChange;
                case DisplayMode.TargetThicknessSummary: return GridDataType.SimpleVolumeOverlay;
                case DisplayMode.TargetSpeedSummary: return GridDataType.MachineSpeedTarget;
                case DisplayMode.CCVChange: return GridDataType.CCVPercentChangeIgnoredTopNullValue;
                case DisplayMode.CCA: return GridDataType.CCA;
                case DisplayMode.CCASummary: return GridDataType.CCA;
                default:
                    Debug.Assert(false, string.Format("Unknown mode ({0}) in ICGridDataTypeForDisplayMode", Mode));
                    return GridDataType.Height;  // For modes that are not supported yet, we will use heights
            }
        }
    }
}
