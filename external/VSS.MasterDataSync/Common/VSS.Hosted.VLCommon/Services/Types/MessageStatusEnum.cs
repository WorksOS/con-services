using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  [DataContract] 
  public enum MessageStatusEnum
  {
    [EnumMember]
    Unknown = -1,
    [EnumMember]
    Pending = 0,
    [EnumMember]
    Sent = 1,
    [EnumMember]
    Acknowledged = 2,
    [EnumMember]
    Suppressed = 3,
  }
}
