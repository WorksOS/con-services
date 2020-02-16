using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon
{
  public enum AssetSecurityEventStatus
  {
    [EnumMember]
    Unknown = -1,
    [EnumMember]
    Sent = 1,
    [EnumMember]
    StartModeConfigured = 2,
    [EnumMember]
    Suppressed = 3,
    [EnumMember]
    LeaseOwershipClaim = 4,//LeaseOwnerSet ref:AssetSecurityEventLog
    [EnumMember]
    LeaseOwershipRelease = 5, //LeaseOwnerCleared ref:AssetSecurityEventLog
    [EnumMember]
    TamperLevelPending = 6,
    [EnumMember]
    StartModeTamperLevel = 7,
    [EnumMember]
    TamperLevel = 8,
    [EnumMember]
    StartModePending = 9,
    [EnumMember]
    StartMode = 10,
    [EnumMember]
    TamperAppliedStartModeConfigured = 11, //when device sends both start mode configured and tamper level
    [EnumMember]
    StartModeTamperLevelPending = 12
  }
}
