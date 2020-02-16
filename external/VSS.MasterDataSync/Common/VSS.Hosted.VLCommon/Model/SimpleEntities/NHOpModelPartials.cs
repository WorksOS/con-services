using System;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
    partial class User
    {
        public string Account;
        public int NHWebFeatureAccess;
        public DateTime? lastLoginUTC;
        public bool IsSSOUser;
    }

    [Serializable()]
    partial class BookmarkManager
    {
    }

    partial class Fault : IEquatable<Fault>
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as Fault);
        }

        public bool Equals(Fault other)
        {
            return (other != null
              && (this.ID.Equals(other.ID)
              || this.CodedDescription.Equals(other.CodedDescription)));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    partial class ServiceType
    {
        //Apply to VLCore and CATCore Plans
        public static TimeSpan DefaultSamplingInterval { get { return TimeSpan.FromHours(6); } }
        public static TimeSpan DefaultReportingInterval { get { return TimeSpan.FromHours(6); } }

        //Internal use Only
        public static TimeSpan DefaultLowPowerInterval { get { return TimeSpan.FromHours(6); } }
        public static TimeSpan DefaultBitPacketInterval { get { return TimeSpan.FromHours(8); } }

        //Apply to CatUTIL plan requires hourly reporting of fuel
        public static TimeSpan PerformanceSamplingInterval { get { return TimeSpan.FromHours(1); } }
        public static TimeSpan PerformanceReportingInterval { get { return TimeSpan.FromHours(1); } }

        //One Minute Plan requires one minute positions
        public static TimeSpan OneMinuteSamplingInterval { get { return TimeSpan.FromMinutes(1); } }
        public static TimeSpan TenMinuteReportingInterval { get { return TimeSpan.FromMinutes(10); } }
        public static TimeSpan OneMinuteReportingInterval { get { return TimeSpan.FromMinutes(1); } }

    }

    partial class Sensor : IEquatable<Sensor>
    {
        public bool Equals(Sensor other)
        {
            return (other != null
              && (this.ID.Equals(other.ID)));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    public partial class Asset
    {
        public static long ComputeAssetID(string makeCode, string serialNumberVIN)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] hashKey = new Byte[makeCode.Length];
            hashKey = encoding.GetBytes(makeCode);
            HMACMD5 myhmacMD5 = new HMACMD5(hashKey);
            byte[] hash = myhmacMD5.ComputeHash(encoding.GetBytes(serialNumberVIN));
            // JBP 12/20/2010 Remove highest order 12 bits so no numbers with more than 52 bit mantissa are allowed.
            // This is used to prevent conflict that can occur with numbers that are too large for the flourine client conversion software.
            hash[6] &= 0x0F;
            hash[7] &= 0x00;
            return BitConverter.ToInt64(hash, 0);
        }
    }

    public partial class DevicePersonality : INHOPDataObject, ICATIdentity
    {

        public long AssetID { get; set; }
        public string MakeCode { get; set; }
        public string SerialNumberVIN { get; set; }
        public string GPSDeviceID { get; set; }
        public DeviceTypeEnum DeviceType { get; set; }
        public long? SourceMsgID { get; set; }

        public XElement ToXElement()
        {
            XElement itemXML = new XElement("DevicePersonality");
            itemXML.SetAttributeValue("AssetID", AssetID);
            itemXML.SetAttributeValue("MakeCode", MakeCode);
            itemXML.SetAttributeValue("SerialNumberVIN", SerialNumberVIN);
            itemXML.SetAttributeValue("GPSDeviceID", GPSDeviceID);
            itemXML.SetAttributeValue("DeviceType", DeviceType);
            //itemXML.SetAttributeValue("SourceMsgID", SourceMsgID);
            return itemXML;
        }
    }
    public partial class NH_OP
    {
        [EdmFunction("VSS.Hosted.VLCommon.Store", "fn_GetOwnership")]
        public long fn_GetOwnership(long assetID)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
    }

}
