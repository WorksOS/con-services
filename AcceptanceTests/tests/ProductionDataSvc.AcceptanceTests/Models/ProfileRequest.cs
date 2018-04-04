using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SVOICDecls;
using SVOSiteVisionDecls;
using SVOICProfileCell;
using SVOICGridCell;
using SVOICFiltersDecls;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
    /// </summary>
    public class ProfileRequest
    {
        /// <summary>
        /// The project to perform the request against
        /// </summary>
        public long? projectID { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// The type of profile to be generated.
        /// </summary>
        public ProductionDataType profileType { get; set; }

        /// <summary>
        /// The filter instance to use in the request
        /// Value may be null.
        /// </summary>
        public FilterResult filter { get; set; }

        /// <summary>
        /// The filter ID to used in the request.
        /// Value may be null.
        /// </summary>
        public long? filterID { get; set; }

        /// <summary>
        /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
        /// Value may be null.
        /// </summary>
        public DesignDescriptor alignmentDesign { get; set; }

        /// <summary>
        /// A series of points along which to generate the profile. Coorinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
        /// Value may be null.
        /// </summary>
        public ProfileGridPoints gridPoints { get; set; }

        /// <summary>
        /// A series of points along which to generate the profile. Coorinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
        /// Value may be null.
        /// </summary>
        public ProfileLLPoints wgs84Points { get; set; }

        /// <summary>
        /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
        /// </summary>
        public double? startStation { get; set; }

        /// <summary>
        /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
        /// </summary>
        public double? endStation { get; set; }

        /// <summary>
        /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        /// <summary>
        /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
        /// </summary>
        public bool returnAllPassesAndLayers { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// Base class containing common information relevant to linear and alignment based profile calculations
    /// </summary>
    public class BaseProfile : RequestResult, IEquatable<BaseProfile>
    {
        #region Members
        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        /// 
        public Guid callId;

        /// <summary>
        /// Was the profile calculation successful?
        /// </summary>
        /// 
        public bool success;

        /// <summary>
        /// The minimum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
        /// the first position of the profile line or alignment.
        /// </summary>
        /// 
        public double minStation;

        /// <summary>
        /// The maximum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
        /// the first position of the profile line or alignment.
        /// </summary>
        /// 
        public double maxStation;

        /// <summary>
        /// The minimum elevation across all cells processed in the profile result
        /// </summary>
        /// 
        public double minHeight;

        /// <summary>
        /// The maximum elevation across all cells processed in the profile result
        /// </summary>
        /// 
        public double maxHeight;

        /// <summary>
        /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
        /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
        /// </summary>
        /// 
        public double gridDistanceBetweenProfilePoints; 
        #endregion

        #region Constructor
        protected BaseProfile() :
            base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(BaseProfile other)
        {
            if (other == null)
                return false;

            return this.callId == other.callId &&
                this.success == other.success &&
                Math.Round(this.minStation, 3) == Math.Round(other.minStation, 3) &&
                Math.Round(this.maxStation, 3) == Math.Round(other.maxStation, 3) &&
                Math.Round(this.minHeight, 3) == Math.Round(other.minHeight, 3) &&
                Math.Round(this.maxHeight, 3) == Math.Round(other.maxHeight, 3) &&
                Math.Round(this.gridDistanceBetweenProfilePoints, 3) == Math.Round(other.gridDistanceBetweenProfilePoints, 3) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(BaseProfile a, BaseProfile b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(BaseProfile a, BaseProfile b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is BaseProfile && this == (BaseProfile)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// The collection of information for a single cell in the computed profile (which may contain many cells) as defined by the collection of cell passes that comprise it.
    /// Values are revelant to the cell as a whole. In the case of cell attributes, event or target information they represent the latest known values
    /// for those items as at the time of the last contributory cell pass in the cell that contained a known-value for the attribute in question.
    /// Composite elevations are elevations that are calculated from a combination of elevation information from production data sourced from TAG files produced
    /// by machine control systems, the elevation information obtained from dated topological surveys (surveyed surfaces).
    /// </summary>
    /// 
    public class ProfileCell
    {
        #region Members
        /// <summary>
        /// The station value, or distance from start of the profile line at which the profile line intersects this cell.
        /// </summary>
        /// 
        public double station { get; set; }

        /// <summary>
        /// Elevation of first cell pass in the profile cell.
        /// </summary>
        /// 
        public float firstPassHeight { get; set; }

        /// <summary>
        /// Highest elevation recoreded for all cell passes involved in computation of this profile cell.
        /// </summary>
        /// 
        public float highestPassHeight { get; set; }

        /// <summary>
        /// The elevation of the last (in time) cell pass involved in computation of this profile cell.
        /// </summary>
        /// 
        public float lastPassHeight { get; set; }

        /// <summary>
        /// Lowest elevation recoreded for all cell passes involved in computation of this profile cell.
        /// </summary>
        /// 
        public float lowestPassHeight { get; set; }

        /// <summary>
        /// First (in time) composite elevation recorded in the cell
        /// </summary>
        /// 
        public float firstCompositeHeight { get; set; }

        /// <summary>
        /// Highest composite elevation recorded in the cell
        /// </summary>
        /// 
        public float highestCompositeHeight { get; set; }

        /// <summary>
        /// Last (in time) composite elevation recorded in the cell
        /// </summary>
        /// 
        public float lastCompositeHeight { get; set; }

        /// <summary>
        /// Lowest composite elevation recorded in the cell
        /// </summary>
        /// 
        public float lowestCompositeHeight { get; set; }

        /// <summary>
        /// Elevation of the design at the location of the center point of the cell.
        /// </summary>
        /// 
        public float designHeight { get; set; }

        /// <summary>
        ///  CMV value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the CMV value was recorded.
        /// </summary>
        /// 
        public float cmvPercent { get; set; }

        /// <summary>
        /// Elevation of the cell pass that contributed the CMV value.
        /// </summary>
        /// 
        public float cmvHeight { get; set; }

        /// <summary>
        /// The previous valid CMV value
        /// </summary>
        public float previousCmvPercent { get; set; }

        /// <summary>
        /// MDP value expressed as a percentage of the Target CMV applicable at the time the cell pass that contributed the MDP value was recorded.
        /// </summary>
        /// 
        public float mdpPercent { get; set; }

        /// <summary>
        /// Elevation of the cell pass that contributed the MDP value.
        /// </summary>
        /// 
        public float mdpHeight { get; set; }

        /// <summary>
        /// Temperature value. Value expressed in Celcius.
        /// </summary>
        /// 
        public float temperature { get; set; }

        /// <summary>
        /// Elevation of the cell pass that contributed the temperature value.
        /// </summary>
        /// 
        public float temperatureHeight { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// 
        public int temperatureLevel { get; set; }

        /// <summary>
        /// Number of passes contained in the top most layer analysed from the cell passes
        /// </summary>
        /// 
        public int topLayerPassCount { get; set; }

        /// <summary>
        /// Pass count target application at the time the last cell pass that contributed to the top most layer was recorded
        /// </summary>
        /// 
        public TargetPassCountRange topLayerPassCountTargetRange { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        /// 
        public int passCountIndex { get; set; }

        /// <summary>
        /// The thickness of the top most layer analysed from the cell passes. Value is expressed in meters.
        /// </summary>
        /// 
        public float topLayerThickness { get; set; }

        /// <summary>
        /// Determine the minimum elevation of the measured production data elevation or composite elevation
        /// </summary>
        /// 
        public float minHeight
        {
            get
            {
                return lowestPassHeight < lowestCompositeHeight ? lowestPassHeight : lowestCompositeHeight;
            }
        }

        /// <summary>
        /// Determine the maximum elevation of the measured production data elevation or composite elevation
        /// </summary>
        /// 
        public float maxHeight
        {
            get
            {
                return highestPassHeight > highestCompositeHeight ? highestPassHeight : highestCompositeHeight;
            }
        } 
        #endregion

        #region Equality test
        public static bool operator ==(ProfileCell a, ProfileCell b)
        {
            PropertyInfo[] allProperties = typeof(ProfileCell).GetProperties();
            foreach (var property in allProperties)
            {
                if (property.PropertyType == typeof(TargetPassCountRange))
                {
                    if ((TargetPassCountRange)property.GetValue(a) != (TargetPassCountRange)property.GetValue(b))
                        return false;
                }                
                else if (Double.IsNaN(Convert.ToDouble(property.GetValue(a))) || Double.IsNaN(Convert.ToDouble(property.GetValue(b))))
                {
                    if (!(Double.IsNaN(Convert.ToDouble(property.GetValue(a))) && Double.IsNaN(Convert.ToDouble(property.GetValue(b)))))
                        return false;
                }
                else 
                {
                    if(Math.Round(Convert.ToDouble(property.GetValue(a)), 2) != Math.Round(Convert.ToDouble(property.GetValue(b)), 2))
                        return false;
                }
            }

            return true;
        }

        public static bool operator !=(ProfileCell a, ProfileCell b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ProfileCell && this == (ProfileCell)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        } 
        #endregion
    }

    /// <summary>
    /// A point that combines WGS85 latitude and longitude with a station value. This is used as a part of the alignment based profile response 
    /// representation to define the geometry along which the profile computation as made.
    /// </summary>
    /// 
    public class StationLLPoint
    {
        #region Members
        /// <summary>
        /// Station of point. Value is expressed in meters.
        /// </summary>
        /// 
        public double station;

        /// <summary>
        /// Latitude of point. Value is expressed in radians.
        /// </summary>
        /// 
        public double lat;

        /// <summary>
        /// Latitude of point. Value is expressed in radians.
        /// </summary>
        /// 
        public double lng; 
        #endregion

        #region Equality test
        public static bool operator ==(StationLLPoint a, StationLLPoint b)
        {
            return Math.Round(a.station, 2) == Math.Round(b.station, 2) &&
                Math.Round(a.lat, 2) == Math.Round(b.lat, 2) &&
                Math.Round(a.lng, 2) == Math.Round(b.lng, 2);
        }

        public static bool operator !=(StationLLPoint a, StationLLPoint b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is StationLLPoint && this == (StationLLPoint)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// The representation of a profile computed as a straight line between two points in the cartesian grid coordinate system of the project or
    /// by following a section of an alignment centerline.
    /// </summary>
    /// 
    public class ProfileResult : BaseProfile
    {
        #region Members
        /// <summary>
        /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
        /// </summary>
        /// 
        public List<ProfileCell> cells;

        /// <summary>
        /// A geometrical representation of the profile which defines the actual portion of the line or alignment used for the profile.
        /// </summary>
        /// 
        public List<StationLLPoint> alignmentPoints; 
        #endregion

        #region Constructor
        public ProfileResult()
            : base()
        { } 
        #endregion

        #region Equality test
        public bool Equals(ProfileResult other)
        {
            if (other == null)
                return false;

            if (this.cells.Count != other.cells.Count)
                return false;
            if (this.alignmentPoints.Count != other.alignmentPoints.Count)
                return false;

            for (int i = 0; i < this.cells.Count; ++i)
            {
                if (this.cells[i] != other.cells[i])
                    return false;
            }

            for (int i = 0; i < this.alignmentPoints.Count; ++i)
            {
                if (this.alignmentPoints[i] != other.alignmentPoints[i])
                    return false;
            }

            return (BaseProfile)this == (BaseProfile)other;
        }

        public static bool operator ==(ProfileResult a, ProfileResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ProfileResult a, ProfileResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ProfileResult && this == (ProfileResult)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        #endregion
    } 
    #endregion
}
