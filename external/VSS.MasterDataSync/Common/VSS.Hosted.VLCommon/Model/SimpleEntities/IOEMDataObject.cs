using System;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace VSS.Hosted.VLCommon
{
  public interface INHOEMDataObject
  {
    #region Common NH_OEMData.CAT_XXX fields
    [global::System.Runtime.Serialization.DataMemberAttribute()]
    long MessageID { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    long MasterMsgID { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    DateTime EventUTC { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    string MakeCode { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    string SerialNumber { get; set; }
    
    [global::System.Runtime.Serialization.DataMemberAttribute()]
    string GpsDeviceID { get; set; }

    [global::System.Runtime.Serialization.DataMemberAttribute()]
    int DeviceTypeID { get; set; }

    #endregion

    /// <summary>
    /// Each Data type must provide an implementation for persisting itself to a SqlDataRecord to support
    /// bulk inserts.
    /// </summary>
    /// <returns></returns>
    SqlDataRecord ToSqlDataRecord();

    XElement ToXElement(string oemName);
  }
}
