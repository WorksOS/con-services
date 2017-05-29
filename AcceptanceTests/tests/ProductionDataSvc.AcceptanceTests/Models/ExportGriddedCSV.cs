using System;
using System.Linq;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The representation of a pass counts request
    /// </summary>
    public class ExportGriddedCSVRequest
    {
        /// <summary>
        /// The project to process the CS definition file into.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// An identifier from the caller. 
        /// </summary>
        public Guid? callId { get; protected set; }

        /// <summary>
        /// Determines if the coordinates of the points in the emitted CSV file are in Northing and Easting coordinates or 
        /// in Station and Offset coordinates with respect to a road design centerline supplied as a part of the request.
        /// </summary>
        public GriddedCSVReportType reportType { get; protected set; }

        /// <summary>
        /// Sets the custom caller identifier.
        /// </summary>
        public string callerId { get; protected set; }

        /// <summary>
        /// Sets the design file to be used for cut/fill or station/offset calculations
        /// </summary>
        public DesignDescriptor designFile { get; protected set; }

        /// <summary>
        /// The filter instance to use in the request
        /// Value may be null.
        /// </summary>
        public Filter filter { get; protected set; }

        /// <summary>
        /// The filter ID to be used in the request.
        /// May be null.
        /// </summary>
        public long filterID { get; protected set; }

        /// <summary>
        /// A collection of parameters and configuration information relating to analysis and determination of material layers.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; protected set; }

        /// <summary>
        /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
        /// </summary>
        public double interval { get; protected set; }

        /// <summary>
        /// Include the measured elevation at the sampled location
        /// </summary>
        public bool reportElevation { get; protected set; }

        /// <summary>
        /// Include the calculated cut-fill between the elevation at the sampled location and the design elevation at the same location
        /// </summary>
        public bool reportCutFill { get; protected set; }

        /// <summary>
        /// Include the measured CMV at the sampled location
        /// </summary>
        public bool reportCMV { get; protected set; }

        /// <summary>
        /// Include the measured MDP at the sampled location
        /// </summary>
        public bool reportMDP { get; protected set; }

        /// <summary>
        /// Include the calculated pass count at the sampled location
        /// </summary>
        public bool reportPassCount { get; protected set; }

        /// <summary>
        /// Include the measured temperature at the sampled location
        /// </summary>
        public bool reportTemperature { get; protected set; }

        /// <summary>
        /// *** Currently unclear, related to alignment based gridded CSV exports
        /// </summary>
        public GriddedCSVReportOption reportOption { get; protected set; }

        /// <summary>
        /// The Northing ordinate of the location to start gridding from
        /// </summary>
        public double startNorthing { get; protected set; }

        /// <summary>
        /// The Easting ordinate of the location to start gridding from
        /// </summary>
        public double startEasting { get; protected set; }

        /// <summary>
        /// The Northing ordinate of the location to end gridding at
        /// </summary>
        public double endNorthing { get; protected set; }

        /// <summary>
        /// The Easting ordinate of the location to end gridding at
        /// </summary>
        public double endEasting { get; protected set; }

        /// <summary>
        /// The orientation of the grid, expressed in radians
        /// </summary>
        public double direction { get; protected set; }

        /// <summary>
        /// If set to false, the returned file content is an ANSI text representation of the CSV file. If set to true, the returned file content is a ZIP conpressed archive containing a single file with the name 'asbuilt.csv'.
        /// </summary>
        public bool compress { get; protected set; }
    }

    public enum GriddedCSVReportType
    {
        Gridded = 1,
        Alignment = 2,
    }

    public enum GriddedCSVReportOption
    {
        Direction = 0,
        EndPoint = 1,
        Automatic = 2
    }
    #endregion

    #region Result
    public class ExportGriddedCSVResult : RequestResult, IEquatable<ExportReportResult>
    {
        #region Members
        public byte[] ExportData { get; set; }
        public short ResultCode { get; set; }
        #endregion

        #region Constructor
        public ExportGriddedCSVResult() :
            base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(ExportReportResult other)
        {
            if (other == null)
                return false;

            return Common.Decompress(this.ExportData).SequenceEqual(Common.Decompress(other.ExportData)) &&
                this.ResultCode == other.ResultCode &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(ExportGriddedCSVResult a, ExportGriddedCSVResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ExportGriddedCSVResult a, ExportGriddedCSVResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ExportGriddedCSVResult && this == (ExportGriddedCSVResult)obj;
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