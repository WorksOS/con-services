using System.Runtime.Serialization;

namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  /// <summary>
  /// Device type on the platform e.g. Tablet or EC520
  /// this list comes from cws /devices/deviceModelTypes endpoint as at 2020_07_06
  /// </summary>

  public enum CWSDeviceTypeEnum
  {
    [EnumMember(Value = "Unknown")] Unknown = 0, 

    [EnumMember(Value = "EC520")] EC520,     // Earthworks serialNumber ends YU
                                             // e.g. 1234J501YU = EC520 the digit after the J will be 5 or greater

    [EnumMember(Value = "EC520-W")] EC520W,  // Earthworks EC520-W (wi-fi version) serialNumber ends YU 
                                             // e.g. 1234J001YU = EC520-W the digit after the J will be <5

    [EnumMember(Value = "CB430")] CB430,     // GCS900 serialNumber ends SM

    [EnumMember(Value = "CB450")] CB450,     // GCS900 serialNumber ends SV

    [EnumMember(Value = "CB460")] CB460,     // GCS900 serialNumber ends SW

    [EnumMember(Value = "TMC")] TMC,         // Marine dredgers: CutterSuctionDredge = 70, BargeMountedExcavator = 71
                                             // serial number is a guid 

    [EnumMember(Value = "CD700")] CD700,

    [EnumMember(Value = "TSC3")] TSC3,

    [EnumMember(Value = "Tablet")] Tablet,

    [EnumMember(Value = "Mobile")] Mobile,

    [EnumMember(Value = "VERSO")] VERSO,

    [EnumMember(Value = "EM")] EM,
  }
}
