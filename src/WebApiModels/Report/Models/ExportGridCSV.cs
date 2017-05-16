using System;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Interfaces;

namespace VSS.Nighthawk.ReportSvc.WebApi.Models
{
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

    /// <summary>
    /// The representation of a pass counts request
    /// </summary>
    public class ExportGridCSV : ProjectID, IValidatable
    {
        /// <summary>
        /// An identifier from the caller. 
        /// </summary>
        //[Required]
        [JsonProperty(PropertyName = "callId", Required = Required.Default)]
        public Guid? callId { get; protected set; }

        /// <summary>
        /// Determines if the coordinates of the points in the emitted CSV file are in Northing and Easting coordinates or 
        /// in Station and Offset coordinates with respect to a road design centerline supplied as a part of the request.
        /// </summary>
        [JsonProperty(PropertyName = "reportType", Required = Required.Default)]
        public GriddedCSVReportType reportType { get; protected set; }

        /// <summary>
        /// Sets the custom caller identifier.
        /// </summary>
        [JsonProperty(PropertyName = "callerId", Required = Required.Default)]
        public string callerId { get; protected set; }

        /// <summary>
        /// Sets the design file to be used for cut/fill or station/offset calculations
        /// </summary>
        [JsonProperty(PropertyName = "designFile", Required = Required.Default)]
        public TVLPDDesignDescriptor designFile { get; protected set; }

        /// <summary>
        /// The filter instance to use in the request
        /// Value may be null.
        /// </summary>
        [JsonProperty(PropertyName = "filter", Required = Required.Default)]
        public Filter filter { get; protected set; }

        /// <summary>
        /// The filter ID to used in the request.
        /// May be null.
        /// </summary>
        [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
        public long filterID { get; protected set; }

        /// <summary>
        /// A collection of parameters and configuration information relating to analysis and determination of material layers.
        /// </summary>
        [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
        public LiftBuildSettings liftBuildSettings { get; protected set; }

        /// <summary>
        /// The spacing interval for the sampled points. Setting to 1.0m will cause points to be spaced 1.0 meters apart.
        /// </summary>
        [JsonProperty(PropertyName = "interval", Required = Required.Default)]
        public double interval { get; protected set; }

        /// <summary>
        /// Include the measured elevation at the sampled location
        /// </summary>
        [JsonProperty(PropertyName = "reportElevation", Required = Required.Default)]
        public bool reportElevation { get; protected set; }

        /// <summary>
        /// Include the calculated cut-fill between the elevation at the sampled location and the design elevation at the same location
        /// </summary>
        [JsonProperty(PropertyName = "reportCutFill", Required = Required.Default)]
        public bool reportCutFill { get; protected set; }

        /// <summary>
        /// Include the measured CMV at the sampled location
        /// </summary>
        [JsonProperty(PropertyName = "reportCMV", Required = Required.Default)]
        public bool reportCMV { get; protected set; }

        /// <summary>
        /// Include the measured MDP at the sampled location
        /// </summary>
        [JsonProperty(PropertyName = "reportMDP", Required = Required.Default)]
        public bool reportMDP { get; protected set; }

        /// <summary>
        /// Include the calculated pass count at the sampled location
        /// </summary>
        [JsonProperty(PropertyName = "reportPassCount", Required = Required.Default)]
        public bool reportPassCount { get; protected set; }

        /// <summary>
        /// Include the measured temperature at the sampled location
        /// </summary>
        [JsonProperty(PropertyName = "reportTemperature", Required = Required.Default)]
        public bool reportTemperature { get; protected set; }

        /// <summary>
        /// *** Currently unclear, related to alignment based gridded CSV exports
        /// </summary>
        [JsonProperty(PropertyName = "reportOption", Required = Required.Default)]
        public GriddedCSVReportOption reportOption { get; protected set; }

        /// <summary>
        /// The Northing ordinate of the location to start gridding from
        /// </summary>
        [JsonProperty(PropertyName = "startNorthing", Required = Required.Default)]
        public double startNorthing { get; protected set; }

        /// <summary>
        /// The Easting ordinate of the location to start gridding from
        /// </summary>
        [JsonProperty(PropertyName = "startEasting", Required = Required.Default)]
        public double startEasting { get; protected set; }

        /// <summary>
        /// The Northing ordinate of the location to end gridding at
        /// </summary>
        [JsonProperty(PropertyName = "endNorthing", Required = Required.Default)]
        public double endNorthing { get; protected set; }

        /// <summary>
        /// The Easting ordinate of the location to end gridding at
        /// </summary>
        [JsonProperty(PropertyName = "endEasting", Required = Required.Default)]
        public double endEasting { get; protected set; }

        /// <summary>
        /// The orientation of the grid, expressed in radians
        /// </summary>
        [JsonProperty(PropertyName = "direction", Required = Required.Default)]
        public double direction { get; protected set; }

        /// <summary>
        /// Compress the result
        /// </summary>
        [JsonProperty(PropertyName = "compress", Required = Required.Default)]
        public bool compress { get; protected set; }

        public TTranslation[] translations { get; private set; }

        protected ExportGridCSV()
        {
        }

        /*
    /// <summary>
    /// Create instance of ExportGridCSV
    /// </summary>
    public static ExportGridCSV CreateExportGridCSVRequest(long projectId, LiftBuildSettings liftBuildSettings,
        Filter filter, long filterID, Guid? callid, string callerID, 
        string FileName, GriddedCSVReportType ExportType)
    {
      return new ExportGridCSV
      {
                 projectId = projectId,
                 liftBuildSettings = liftBuildSettings,
                 filter = filter,
                 filterID = filterID,
                 callId = callid,
                 callerId = callerID,
                 filename = FileName
       };
    }
    */
    /*
        /// <summary>
        /// Create example instance of PassCounts to display in Help documentation.
        /// </summary>
        public static ExportGridCSV HelpSample
        {
            get
            {
                return new ExportGridCSV()
                {
                    projectId = 34,
                    liftBuildSettings = LiftBuildSettings.HelpSample,
                    filter = Filter.HelpSample,
                    filterID = 0,
                    callId = new Guid(),
                    callerId = "Myself"
                };
            }
        }
*/

        /// <summary>
        /// Validates all properties
        /// </summary>
        public override void Validate()
        {
            translations = new TTranslation[6];
            translations[0].ID = 0;
            translations[0].Translation = "Problem occured processing export.";
            translations[1].ID = 1;
            translations[1].Translation = "No data found";
            translations[2].ID = 2;
            translations[2].Translation = "Timed out";
            translations[3].ID = 3;
            translations[3].Translation = "Unexpected error";
            translations[4].ID = 4;
            translations[4].Translation = "Request Canceled";
            translations[5].ID = 5;
            translations[5].Translation = "Maxmium records reached";
        }
    }
}