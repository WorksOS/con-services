using System.ComponentModel;

namespace VSS.Common.Abstractions.Clients.ProfileX.Enums
{
  public enum LifeState
  {
    [Description("ACTIVE")]
    Active,
    [Description("INACTIVE")]
    Inactive,
    [Description("PENDING")]
    Pending,
    [Description("DELETED")]
    Deleted
  }
}