using System.ComponentModel;

namespace VSS.Common.Abstractions.ServiceDiscovery.Enums
{
  public enum ApiVersion
  {
    V1,

    // We can't have a dot in the name, so use this
    [Description("v1.1")]
    V1_1,

    V2,
    V3,
    V4,
    V5,
    V6

    // Add more as needed
  }
}
