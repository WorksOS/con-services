using System.Runtime.Serialization;

namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  /// <summary>
  /// Device type on the platform e.g. Tablet or EC520
  /// this list comes from cws /devices/deviceModelTypes endpoint as at 2020_07_06
  /// </summary>

  public enum CWSDeviceTypeEnum
  {
    [EnumMember(Value = null)] Unknown = 0, // null

    [EnumMember(Value = "EC520")] EC520,     // Earthworks serialNumber ends YU
                                             // e.g. 1234J501YU = EC520 (Non Wi-Fi version)

    [EnumMember(Value = "EC520-W")] EC520W,  // Earthworks EC520-W serialNumber ends YU
                                             // the digit after the J will be 5 or greater for a non wi-fi version
                                             // e.g. 1234J001YU = EC520-W (Wi-Fi version)

    [EnumMember(Value = "CB430")] CB430,     // GCS900 serialNumber ends SM

    [EnumMember(Value = "CB450")] CB450,     // GCS900 serialNumber ends SV

    [EnumMember(Value = "CB460")] CB460,     // GCS900 serialNumber ends SW

    [EnumMember(Value = "CD700")] CD700,

    [EnumMember(Value = "TSC3")] TSC3,

    [EnumMember(Value = "Tablet")] Tablet,

    [EnumMember(Value = "Mobile")] Mobile,

    [EnumMember(Value = "VERSO")] VERSO,

    [EnumMember(Value = "EM")] EM,
  }
}
