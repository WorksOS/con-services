using System;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using Newtonsoft.Json;
using OnlineHelp;
using VLPDDecls;
using VSS.Nighthawk.RaptorServicesCommon.Interfaces;
using VSS.Nighthawk.RaptorServicesCommon.Models;

namespace VSS.Raptor.Service.WebApiModels.Report.Models
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
    public class ExportGridCSV : ProjectID, IValidatable, IHelpSample
    {

        /// <summary>
        /// An identifier from the caller. 
        /// </summary>
        //[Required]
        [JsonProperty(PropertyName = "callId", Required = Required.Default)]
        public Guid? callId { get; protected set; }

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

        [JsonProperty(PropertyName = "interval", Required = Required.Default)]
        public double interval { get; protected set; }


        [JsonProperty(PropertyName = "reportElevation", Required = Required.Default)]
        public bool reportElevation { get; protected set; }

        [JsonProperty(PropertyName = "reportCutFill", Required = Required.Default)]
        public bool reportCutFill { get; protected set; }

        [JsonProperty(PropertyName = "reportCMV", Required = Required.Default)]
        public bool reportCMV { get; protected set; }

        [JsonProperty(PropertyName = "reportMDP", Required = Required.Default)]
        public bool reportMDP { get; protected set; }

        [JsonProperty(PropertyName = "reportPassCount", Required = Required.Default)]
        public bool reportPassCount { get; protected set; }

        [JsonProperty(PropertyName = "reportTemperature", Required = Required.Default)]
        public bool reportTemperature { get; protected set; }

        [JsonProperty(PropertyName = "reportOption", Required = Required.Default)]
        public GriddedCSVReportOption reportOption { get; protected set; }

        [JsonProperty(PropertyName = "startNorthing", Required = Required.Default)]
        public double startNorthing { get; protected set; }

        [JsonProperty(PropertyName = "startEasting", Required = Required.Default)]
        public double startEasting { get; protected set; }

        [JsonProperty(PropertyName = "endNorthing", Required = Required.Default)]
        public double endNorthing { get; protected set; }

        [JsonProperty(PropertyName = "endEasting", Required = Required.Default)]
        public double endEasting { get; protected set; }

        [JsonProperty(PropertyName = "direction", Required = Required.Default)]
        public double direction { get; protected set; }

        //        [JsonProperty(PropertyName = "zipFile", Required = Required.Default)]
        //        public bool zipFile { get; protected set; }

        [JsonProperty(PropertyName = "filename", Required = Required.Default)]
        public string filename { get; protected set; }

        public TTranslation[] translations { get; private set; }

        public TASNodeUserPreferences userPrefs { get; private set; }

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
                    filename = "GoToSomwhere.csv",
                    callId = new Guid(),
                    callerId = "Myself"
                };
            }
        }


        /// <summary>
        /// Validates all properties
        /// </summary>
        public void Validate()
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

            userPrefs =
                   ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences("NZ", "/", ":", ",", ".", 0.0, 0, 1, 0, 0, 1, 3);
        }
    }
}