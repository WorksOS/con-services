using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// Common Interface for all OP objects carried through service bus
  /// </summary>
  public interface INHOPDataObject
  {
    [global::System.Runtime.Serialization.DataMemberAttribute()]
    long AssetID { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    string GPSDeviceID { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    DeviceTypeEnum DeviceType { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    long? SourceMsgID { get; set; }

  }
}
