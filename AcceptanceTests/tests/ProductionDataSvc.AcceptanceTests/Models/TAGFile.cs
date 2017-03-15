using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// TAG file domain object
    /// This is copied from ...\TagFileProcessing\Models\TAGFile.cs
    /// </summary>
    public class TagFilePostParameter
    {
        /// <summary>
        /// The name of the TAG file.
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// The content of the TAG file as an array of bytes.
        /// </summary>
        public byte[] data { get; set; }

        /// <summary>
        /// The project to process the TAG file into. This is optional. If not set, Raptor will determine automatically which project the TAG file should be processed into.
        /// </summary>
        public long? projectId { get; set; }

        /// <summary>
        /// The boundary of the project to process the TAG file into. If the location of the data in the TAG file is outside of this boundary it will not be processed into the project.
        /// May be null.
        /// </summary>
        public WGS84Fence boundary { get; set; }

        /// <summary>
        /// The machine (asset) ID to process the TAG file as.
        /// May be null.
        /// </summary>
        public long? machineId { get; set; }

        ///// <summary>
        ///// A flag to indicate if the TAG file should also be converted into a CSV file. Not currently available.
        ///// </summary>
        //public bool convertToCSV { get; set; }

        ///// <summary>
        ///// A flag to indicate if the TAG file should also be converted into a DXF file. Not currently available.
        ///// </summary>
        //public bool convertToDXF { get; set; } 
    } 
    #endregion

    #region Result
    /// <summary>
    /// Represents response from the service after TAG file POST request
    /// </summary>
    public class TagFilePostResult : RequestResult, IEquatable<TagFilePostResult>
    {
        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public TagFilePostResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(TagFilePostResult other)
        {
            if (other == null)
                return false;

            return this.Code == other.Code && this.Message == other.Message;
        }

        public static bool operator ==(TagFilePostResult a, TagFilePostResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(TagFilePostResult a, TagFilePostResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is TagFilePostResult && this == (TagFilePostResult)obj;
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