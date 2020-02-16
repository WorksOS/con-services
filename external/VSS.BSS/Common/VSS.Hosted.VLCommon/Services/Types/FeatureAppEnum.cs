using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  [DataContract]
  public enum FeatureAppEnum
  {
    [EnumMember]
    NHAdmin = 1000,
    [EnumMember]
    NHWeb = 2000,
    [EnumMember]
    DataServices = 3000,
    [EnumMember]
    VLAdmin = 6000

    //Changing this? The client also needs to be updated
  }
}
