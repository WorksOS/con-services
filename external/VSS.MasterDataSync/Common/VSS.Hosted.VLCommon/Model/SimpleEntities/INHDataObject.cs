using Microsoft.SqlServer.Server;
using System;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{

    /// <summary>
    /// Common interface for all "Data" types from the NH_DATA model.
    /// 
    /// The Data can be identified by Asset ID or if unavailable by setting gpsdeviceID/deviceType. 
    /// </summary>
    public interface INHDataObject
    {
        #region Alternative identifiers
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        string GPSDeviceID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        DeviceTypeEnum DeviceType { get; set; }
        #endregion

        #region Common NH_DATA.DataXXX fields
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        long? DebugRefID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        long? SourceMsgID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        int fk_DimSourceID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        DateTime EventUTC { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        long AssetID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        DateTime? NHReceivedUTC { get; set; }

        #endregion

        /// <summary>
        /// Each Data type must provide an implementation for persisting itself to a SqlDataRecord to support
        /// bulk inserts.
        /// </summary>
        /// <returns></returns>
        SqlDataRecord ToSqlDataRecord();

        /// <summary>
        /// Prior to use SqlDataRecord, xml was used to serialize the objects for bulk storage. This method
        /// remains on the interface so that the Data types support serialization to a human readable form.
        /// </summary>
        /// <returns></returns>
        XElement ToXElement();
    }

    /// <summary>
    /// Common interface for all Data types that can be sourced from CAT data and therefore
    /// are identifiable using CAT's Identity scheme.
    /// </summary>
    public interface ICATIdentity
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        string MakeCode { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        string SerialNumberVIN { get; set; }
    }
}
