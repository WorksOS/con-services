using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class FiltersResult : RequestResult, IEquatable<FiltersResult>
    {
        #region Members
        /// <summary>
        ///   Gets the filter identifier.
        /// </summary>
        /// <value>
        ///   The filter identifier.
        /// </value>
        public long FilterId { get; set; }

        /// <summary>
        ///   Represents array of filters in the result. Each element is a valid Raptor filter.
        /// </summary>
        /// <value>
        ///   The filters array.
        /// </value>
        public FilterResult[] FiltersArray { get; set; } 
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor: Success by default
        /// </summary>
        public FiltersResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(FiltersResult other)
        {
            if (other == null)
                return false;

            List<FilterResult> thisFilterList = this.FiltersArray.ToList();
            List<FilterResult> otherFilterList = other.FiltersArray.ToList();

            return this.FilterId == other.FilterId &&
                Common.ListsAreEqual(thisFilterList, otherFilterList) &&
                this.Code == other.Code && 
                this.Message == other.Message;
        }

        public static bool operator ==(FiltersResult a, FiltersResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(FiltersResult a, FiltersResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is FiltersResult && this == (FiltersResult)obj;
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
}
