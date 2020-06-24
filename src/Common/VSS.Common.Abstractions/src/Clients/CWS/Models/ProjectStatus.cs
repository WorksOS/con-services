using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public enum ProjectStatus
  {
    [EnumMember(Value="ACTIVE")]
    Active = 0,
    [EnumMember(Value = "ARCHIVED")]
    Archived = 1,
  }
}
