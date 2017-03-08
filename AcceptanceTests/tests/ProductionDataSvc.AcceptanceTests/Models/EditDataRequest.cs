using System;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    public class EditDataRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// Flag which determines if the edit is applied or undone. Required.
        /// </summary>
        public bool undo { get; set; }

        /// <summary>
        /// Details of the edit to apply or undo. Required for applying an edit and for a single undo.
        /// If null and undo is true then all edits to the production data for the project will be undone.
        /// </summary>
        public ProductionDataEdit dataEdit { get; set; }
    }
    #endregion
 
    #region Result
    public class EditDataResult : RequestResult, IEquatable<EditDataResult>
    {
        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public EditDataResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(EditDataResult other)
        {
            if (other == null)
                return false;

            return this.Code == other.Code && this.Message == other.Message;
        }

        public static bool operator ==(EditDataResult a, EditDataResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(EditDataResult a, EditDataResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is EditDataResult && this == (EditDataResult)obj;
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

    /// <summary>
    /// List of ProductionDataEdit used as Injected Scenario Context for the tests.
    /// </summary>
    public class DataEditContext
    {
        public List<ProductionDataEdit> DataEdits { get; set; }
        public DataEditContext()
        {
            DataEdits = new List<ProductionDataEdit>();
        }
    }

    /// <summary>
    /// A representation of an edit applied to production data.
    /// </summary>
    public class ProductionDataEdit
    {
        /// <summary>
        /// The id of the machine whose data is overridden. Required.
        /// </summary>
        public long assetId { get; set; }

        /// <summary>
        /// Start of the period with overridden data. Required.
        /// </summary>
        public DateTime startUTC { get; set; }

        /// <summary>
        /// End of the period with overridden data. Required.
        /// </summary>
        public DateTime endUTC { get; set; }

        /// <summary>
        /// The design name used for the specified override period. May be null.
        /// </summary>
        public string onMachineDesignName { get; set; }

        /// <summary>
        /// The lift number used for the specified override period. May be null.
        /// </summary>
        public int? liftNumber { get; set; } 
    } 
}