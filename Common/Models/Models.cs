using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LandfillService.WebApi.Models
{

  public class ForemanUserPreferences
  {
    private string locale = null;
    private string timeZoneName = null;
    private int unitsTypeID = -1;
    private int assetLabelType = -1;
    private string dateTimeFormat = null;
    private string dateSeparator = null;
    private string timeSeparator = null;
    private int clockIndicator = -1;
    private string currencySymbol = null;
    private string numberFormat = null;
    private string thousandsSeparator = null;
    private string decimalSeparator = null;
    private long userID = -1;

    public string Locale
    {
      get { return locale; }
      set { locale = value; }
    }

    public string TimeZoneName
    {
      get { return timeZoneName; }
      set { timeZoneName = value; }
    }

    public int UnitsTypeID
    {
      get { return unitsTypeID; }
      set { unitsTypeID = value; }
    }

    public int AssetLabelType
    {
      get { return assetLabelType; }
      set { assetLabelType = value; }
    }

    public string DateTimeFormat
    {
      get { return dateTimeFormat; }
      set { dateTimeFormat = value; }
    }

    public string DateSeparator
    {
      get { return dateSeparator; }
      set { dateSeparator = value; }
    }

    public string TimeSeparator
    {
      get { return timeSeparator; }
      set { timeSeparator = value; }
    }

    public int ClockIndicator
    {
      get { return clockIndicator; }
      set { clockIndicator = value; }
    }

    public string CurrencySymbol
    {
      get { return currencySymbol; }
      set { currencySymbol = value; }
    }

    public string NumberFormat
    {
      get { return numberFormat; }
      set { numberFormat = value; }
    }

    public string ThousandsSeparator
    {
      get { return thousandsSeparator; }
      set { thousandsSeparator = value; }
    }
    
    public string DecimalSeparator
    {
      get { return decimalSeparator; }
      set { decimalSeparator = value; }
    }

    public long UserID
    {
      get { return userID; }
      set { userID = value; }
    }
  }

    /// <summary>
    /// User credentials submitted for login
    /// </summary>
    public class Credentials
    {
        public string userName { get; set; }
        
        //[JsonIgnore]
        public string password { get; set; }
    }

    /// <summary>
    /// User credentials submitted for login via VisionLink link
    /// </summary>
    public class VlCredentials
    {
        public string userName { get; set; }

        //[JsonIgnore]
        public string key { get; set; }
    }

    /// <summary>
    /// User representation
    /// </summary>
    public class User
    {
        public uint id { get; set; }
        public string name { get; set; }
        public uint unitsId { get; set; }
    }

    /// <summary>
    /// Session representation
    /// </summary>
    public class Session
    {
        public string id { get; set; }
        public uint userId { get; set; }
    }

    /// <summary>
    /// Project representation
    /// </summary>
    public class Project
    {
        public uint id { get; set; }
        public string name { get; set; }
        public string timeZoneName { get; set; }      // project time zone name
        public bool retrievingVolumes { get; set; }   // is the service retrieving volumes for this project?
    }

    /// <summary>
    /// Weight entry submitted by the user
    /// </summary>
    public class WeightEntry
    {
        public DateTime date { get; set; }          // date of the entry; always in the project time zone
        public double weight { get; set; }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume filter params</returns>
        public override string ToString()
        {
            return String.Format("date:{0}, weight:{1}", date, weight);
        }
    }

    /// <summary>
    /// Data entry for a given date - part of project data sent to the client 
    /// </summary>
    public class DayEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }    // true if the entry has at least the weight value
        public double density { get; set; }       // weight / volume
        public double weight { get; set; }
    }


    /// <summary>
    /// Encapsulates project data sent to the client 
    /// </summary>
    public class ProjectData
    {
        public IEnumerable<DayEntry> entries { get; set; }
        public bool retrievingVolumes { get; set; }          // is the service currently retrieving volumes for this project?
    }

    /// <summary>
    /// Filter for volume summary requests sent to the Raptor API; see Raptor API documentation for details
    /// </summary>
    public class VolumeFilter
    {
        public DateTime startUTC;
        public DateTime endUTC;
        public bool returnEarliest;     

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume filter params</returns>
        public override string ToString()
        {
            return String.Format("startUTC:{0}, endUTC:{1}, returnEarliest:{2}", startUTC, endUTC, returnEarliest);
        }

    }

    /// <summary>
    /// Volume calculation parameters sent to the Raptor API; see Raptor API documentation for details
    /// </summary>
    public class VolumeParams
    {
        public long projectId;
        public int volumeCalcType;
        public VolumeFilter baseFilter;
        public VolumeFilter topFilter;

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume request params</returns>
        public override string ToString()
        {
            return String.Format("projectId:{0}, volumeCalcType:{1}, baseFilter:{2}, topFilter:{3}", projectId, volumeCalcType, baseFilter, topFilter);
        }

    }


    /// <summary>
    /// 3D bounding box - returned in volume summary results from the Raptor API
    /// </summary>
    public class BoundingBox3DGrid
    {
        /// <summary>
        /// Maximum X value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxX { get; set; }

        /// <summary>
        /// Maximum Y value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxY { get; set; }

        /// <summary>
        /// Maximum Z value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxZ { get; set; }

        /// <summary>
        /// Minimum X value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minX { get; set; }

        /// <summary>
        /// Minimum Y value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minY { get; set; }

        /// <summary>
        /// Minimum Z value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minZ { get; set; }
    }


    /// <summary>
    /// Volume summary entry returned from the Raptor API
    /// </summary>
    public class SummaryVolumesResult 
    {
        /// <summary>
        /// Zone boundaries
        /// </summary>
        public BoundingBox3DGrid BoundingExtents { get; set; }
        /// <summary>
        /// Cut volume in m3
        /// </summary>
        public double Cut { get; set; }
        /// <summary>
        /// Fill volume in m3
        /// </summary>
        public double Fill { get; set; }
        /// <summary>
        /// Cut area in m2
        /// </summary>
        public double CutArea { get; set; }
        /// <summary>
        /// Fill area in m2
        /// </summary>
        public double FillArea { get; set; }
        /// <summary>
        /// Total coverage area (cut + fill + no change) in m2. 
        /// </summary>
        public double TotalCoverageArea { get; set; }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume summary results</returns>
        public override string ToString()
        {
            return String.Format("cut:{0}, fill:{1}", Cut, Fill);
        }

    }


    public enum UnitsTypeEnum
    {
      None = -1,
      US = 0,
      Metric = 1,
      Imperial = 2
    }
}