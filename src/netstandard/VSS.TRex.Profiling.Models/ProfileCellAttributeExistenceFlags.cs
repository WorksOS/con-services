using System;

namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// Flags determining which attributes are present with non null values in profile cell
  /// </summary>
  [Flags]
    public enum ProfileCellAttributeExistenceFlags
    {
      None = 0x0,
      HasCCVData = 0x1,
      HasRMVData = 0x2,
      HasFrequencyData = 0x4,
      HasAmplitudeData = 0x8,
      HasGPSModeData = 0x10,
      HasTemperatureData = 0x20,
      HasMDPData = 0x40,
      HasCCAData = 0x80
    }
}
