using System;
using System.Linq;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.Text;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The representation of a pass counts request
    /// </summary>
    public class ExportReportRequest
    {
        /// <summary>
        /// The project to process the CS definition file into.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// An identifier from the caller. 
        /// </summary>
        public Guid? callId { get; set; }

        public ExportTypes exportType { get; set; }

        /// <summary>
        /// Sets the custom caller identifier.
        /// </summary>
        public string callerId { get; set; }

        /// <summary>
        /// The filter instance to use in the request
        /// Value may be null.
        /// </summary>
        public FilterResult filter { get; set; }

        /// <summary>
        /// The filter ID to used in the request.
        /// May be null.
        /// </summary>
        public long filterID { get; set; }

        /// <summary>
        /// A collection of parameters and configuration information relating to analysis and determination of material layers.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        public bool timeStampRequired { get; set; }

        public bool cellSizeRequired { get; set; }

        public bool rawData { get; set; }

        public bool restrictSize { get; set; }

        public double tolerance { get; set; }

        public bool includeSurveydSurface { get; set; }

        public bool precheckonly { get; set; }

        public string filename { get; set; }

        public TMachine[] machineList { get; set; }

        public CoordTypes coordType { get; set; }

        public OutputTypes outputType { get; set; }

        public DateTime dateFromUTC { get; set; }

        public DateTime dateToUTC { get; set; }

        //public T3DBoundingWorldExtent projectExtents { get; protected set; }

        //public TTranslation[] translations { get; private set; }

        //public TASNodeUserPreferences userPrefs { get; private set; } 
    }

    public enum ExportTypes
    {
        kSurfaceExport = 1,
        kPassCountExport = 2,
        kVedaExport = 3
    }
    public enum CoordTypes
    {
        ptNORTHEAST = 0,
        ptLATLONG = 1
    }
    public enum OutputTypes
    {
        etPassCountLastPass,
        etPassCountAllPasses,
        etVedaFinalPass,
        etVedaAllPasses
    }
    public struct TMachine
    {
        public long AssetID;
        public string MachineName;
        public string SerialNo;
    }
    #endregion

    #region Result
    public class ExportReportResult : RequestResult, IEquatable<ExportReportResult>
    {
        #region Members
        public byte[] ExportData { get; set; }
        public short ResultCode { get; set; }
        #endregion

        #region Constructor
        public ExportReportResult() :
            base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(ExportReportResult other)
        {
            if (other == null)
                return false;

          // Check all the entries in the file are the same.
          // Note: The order of the result is not deterministics so the CSV contents need to be compared against
          // each other in light of that.
          string[] thisData = Encoding.Default.GetString(Common.Decompress(this.ExportData)).Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
          string[] otherData = Encoding.Default.GetString(Common.Decompress(other.ExportData)).Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

          return thisData.All(x => otherData.Contains(x)) &&
                 this.ResultCode == other.ResultCode &&
                 this.Code == other.Code &&
                 this.Message == other.Message;
        }

        public static bool operator ==(ExportReportResult a, ExportReportResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ExportReportResult a, ExportReportResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ExportReportResult && this == (ExportReportResult)obj;
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