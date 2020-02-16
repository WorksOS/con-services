using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  partial class User
  {
    public string Account;
    public int NHWebFeatureAccess;
    public DateTime ? lastLoginUTC;
    public  bool IsSSOUser;
  }

  [Serializable()]
  partial class BookmarkManager
  {
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
    public static TimeSpan TenMinuteReportingInterval { get { return TimeSpan.FromMinutes(10); }}
    public static TimeSpan OneMinuteReportingInterval { get { return TimeSpan.FromMinutes(1); } }

    //Apply to CatDaily plan requires least reporting
    public static TimeSpan LeastSamplingInterval { get { return TimeSpan.FromHours(8); } }
    public static TimeSpan LeastReportingInterval { get { return TimeSpan.FromHours(8); } }
    }



  partial class Site : IEquatable<Site>
  {
    private Polygon sitePolygon;

    public Site()
    {
      InitSitePolygon();
    }

    private void InitSitePolygon()
    {
      if (sitePolygon == null)
      {
        sitePolygon = new Polygon();
      }
      sitePolygon.Xml = Polygon;
      sitePolygon.MinLat = MinLat;
      sitePolygon.MaxLat = MaxLat;
      sitePolygon.MinLon = MinLon;
      sitePolygon.MaxLon = MaxLon;
    }

    public IEnumerable<Point> PolygonPoints
    {
      get
      {
        if (sitePolygon == null || sitePolygon.PolygonPoints == null)
        {
          InitSitePolygon();
        }
        return sitePolygon.PolygonPoints;
      }
    }

    public bool PolygonAproximatesRectangle
    {
      get
      {
        if (sitePolygon == null || sitePolygon.PolygonPoints == null)
        {
          InitSitePolygon();
        }
        return sitePolygon.PolygonAproximatesRectangle;
      }
    }

    public bool Inside(double latitude, double longitude)
    {
      if (sitePolygon == null || sitePolygon.PolygonPoints == null)
      {
        InitSitePolygon();
      }
      return sitePolygon.Inside(latitude, longitude);
    }

    public void AddPoint(double latitude, double longitude)
    {
      sitePolygon.AddPoint(latitude, longitude);
      Polygon = sitePolygon.Xml;
    }

    public void InsertPoint(double latitude, double longitude, int atIndex)
    {
      sitePolygon.InsertPoint(latitude, longitude, atIndex);
      Polygon = sitePolygon.Xml;
    }

    public void RemovePoint(int atIndex)
    {
      sitePolygon.RemovePoint(atIndex);
      Polygon = sitePolygon.Xml;
    }

    public void Clear()
    {
      sitePolygon.ClearPoints();
      Polygon = sitePolygon.Xml;
    }

    public bool Equals(Site other)
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

  
   public partial class AssetMonitoring
  {
    public static AssetMonitoring Default(long assetID)
    {
      return new AssetMonitoring
      {
        fk_AssetID = assetID,
        RadiusMeters = 0,
        fk_MonitoringMachineTypeID = (int)MonitoringMachineTypeEnum.Truck,//make sure same as default in UI
        MaxInSiteHours = (double?)null,        
        DebounceSeconds = 30,
        UpdateUTC = DateTime.UtcNow
      };
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

  public partial class TTOut
  {
    public static TTOut CreateTTOut(long id, global::System.DateTime insertUTC, short status, string payload, string unitID)
    {
      TTOut tTOut = new TTOut();
      tTOut.ID = id;
      tTOut.InsertUTC = insertUTC;
      tTOut.Status = status;
      tTOut.Payload = payload;
      tTOut.UnitID = unitID;
      return tTOut;
    }
  }
  partial class PLMessage
  {
    public DateTime MessageUTC { get; set; }
  }

}
