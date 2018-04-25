using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// Coordinate conversion domain object.
    /// </summary>
    public class CoordinateConversionRequest
    {
        /// <summary>
        /// The project to request coordinate conversion for.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// 2D coordinate conversion: 
        ///   0 - from Latitude/Longitude to North/East.
        ///   1 - from North/East to Latitude/Longitude.
        /// </summary>
        public TwoDCoordinateConversionType conversionType { get; set; }

        /// <summary>
        /// The list of coordinates for conversion.
        /// </summary>
        /// 
        public TwoDConversionCoordinate[] conversionCoordinates { get; set; }
    }

    /// <summary>
    /// The defined types of 2D coordinate conversions.
    /// </summary>
    /// 
    public enum TwoDCoordinateConversionType
    {
        /// <summary>
        /// 2D coordinate conversion from Latitude/Longitude to North/East.
        /// </summary>
        /// 
        LatLonToNorthEast = 0,

        /// <summary>
        /// 2D coordinate conversion from North/East to Latitude/Longitude.
        /// </summary>
        /// 
        NorthEastToLatLon = 1
    }

    /// <summary>
    /// A point specified in WGS84 Latitude/Longitude or North/East geographic Cartesian coordinates.
    /// </summary>
    public class TwoDConversionCoordinate
    {
        /// <summary>
        /// Either the Easting or WGS84 Longitude of the position expressed in meters or in radians respectively.
        /// </summary>
        public double x { get; set; }

        /// <summary>
        /// Either the Northing or WGS84 Latitude of the position expressed in meters or in radians respectively.
        /// </summary>
        public double y { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// Coordinate conversion result class.
    /// </summary>
    ///
    public class CoordinateConversionResult : RequestResult, IEquatable<CoordinateConversionResult>
    {
        #region Members
        /// <summary>
        /// The list of converted coordinates.
        /// </summary>
        public TwoDConversionCoordinate[] conversionCoordinates { get; set; } 
        #endregion

        #region Constructor
        public CoordinateConversionResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(CoordinateConversionResult other)
        {
            if (other == null)
                return false;

            if (this.conversionCoordinates.Length != other.conversionCoordinates.Length)
                return false;

            for (int i = 0; i < this.conversionCoordinates.Length; ++i )
            {
                if (Math.Round(this.conversionCoordinates[i].x, 2) != Math.Round(other.conversionCoordinates[i].x, 2) ||
                    Math.Round(this.conversionCoordinates[i].y, 2) != Math.Round(other.conversionCoordinates[i].y, 2))
                    return false;
            }

            return this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CoordinateConversionResult a, CoordinateConversionResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CoordinateConversionResult a, CoordinateConversionResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateConversionResult && this == (CoordinateConversionResult)obj;
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