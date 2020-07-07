using System.Runtime.Serialization;

namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  /// <summary>
  /// Device type on the asset e.g. Tablet or EC520
  /// this list comes from cws /devices/deviceModelTypes endpoint as at 2020_07_06
  /// </summary>

  public enum CWSDeviceTypeEnum
  {
    [EnumMember(Value = null)] Unknown = 0, // null

    [EnumMember(Value = "EC520")] EC520,

    [EnumMember(Value = "EC520-W")] EC520W,

    [EnumMember(Value = "CB430")] CB430,

    [EnumMember(Value = "CB450")] CB450,

    [EnumMember(Value = "CB460")] CB460,

    [EnumMember(Value = "CD700")] CD700,

    [EnumMember(Value = "TSC3")] TSC3,

    [EnumMember(Value = "Tablet")] Tablet,

    [EnumMember(Value = "Mobile")] Mobile,

    [EnumMember(Value = "VERSO")] VERSO,

    [EnumMember(Value = "EM")] EM,
  }
}
