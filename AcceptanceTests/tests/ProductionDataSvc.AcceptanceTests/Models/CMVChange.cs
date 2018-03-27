using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// Represents speed summary request.
    /// </summary>
    public class CMVChangeSummaryRequest
    {
        /// <summary>
        /// The project to process the CS definition file into.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// The filter to be used 
        /// </summary>
        public FilterResult filter { get; set; }

        /// <summary>
        /// Gets or sets the filter identifier.
        /// </summary>
        /// <value>
        /// The filter identifier.
        /// </value>
        public int filterId { get; set; }

        /// <summary>
        /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        /// <summary>
        /// Sets the CMV change summary values to compare against.
        /// </summary>
        public double[] CMVChangeSummaryValues { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// Represents result returned by Summary CMV Change request
    /// </summary>
    public class CMVChangeSummaryResult : RequestResult, IEquatable<CMVChangeSummaryResult>
    {
        #region Members
        /// <summary>
        /// Percent of the cells meeting values request conditions
        /// </summary>
        public double[] Values { get; set; }

        /// <summary>
        /// Gets the coverage area where we have not null measured CCV
        /// </summary>
        public double CoverageArea { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public CMVChangeSummaryResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(CMVChangeSummaryResult other)
        {
            if (other == null)
                return false;

            if (this.Values != null && other.Values != null)
            {
                if (this.Values.Length != other.Values.Length)
                    return false;

                for (int i = 0; i < this.Values.Length; ++i)
                {
                    if (Math.Round(this.Values[i],4) != Math.Round(other.Values[i],4) )
                        return false;
                }
            }
            else if (this.Values == null || other.Values == null)
                return false;

            return Math.Round(this.CoverageArea, 2) == Math.Round(other.CoverageArea, 2) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CMVChangeSummaryResult a, CMVChangeSummaryResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CMVChangeSummaryResult a, CMVChangeSummaryResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CMVChangeSummaryResult && this == (CMVChangeSummaryResult)obj;
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
