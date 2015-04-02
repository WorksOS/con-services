using Newtonsoft.Json;
using System;

namespace LandfillService.WebApi.Models
{
    public class Credentials
    {
        public string userName { get; set; }
        
        //[JsonIgnore]
        public string password { get; set; }
    }

    public class User
    {
        public uint id { get; set; }
        public string name { get; set; }
    }

    public class Session
    {
        public string id { get; set; }
        public uint userId { get; set; }
        //public User user { get; set; }
    }

    public class Project
    {
        public uint id { get; set; }
        public string name { get; set; }
    }


    public class WeightEntry
    {
        public DateTime date { get; set; }
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

    public class DayEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }
        public int density { get; set; }
        public int weight { get; set; }
    }

    public class VolumeFilter
    {
        public DateTime startUTC;
        public DateTime endUTC;
        public bool returnEarliest;
        public int gpsAccuracy;

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume filter params</returns>
        public override string ToString()
        {
            return String.Format("startUTC:{0}, endUTC:{1}, returnEarliest:{2}, gpsAccuracy:{3}", startUTC, endUTC, returnEarliest, gpsAccuracy);
        }

    }

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
}