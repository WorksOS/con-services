using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    class GetEditDataRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long projectId { get; set; }

        /// <summary>
        /// The id of the machine whose data is overridden. 
        /// If not provided then overridden data for all machines for the specified project is returned.
        /// </summary>
        public long? assetId { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// The represenation of the results of an edit data request.
    /// </summary>
    public class GetEditDataResult : RequestResult, IEquatable<GetEditDataResult>
    {
        #region Members
        /// <summary>
        /// The collection of data edits applied to the production data.
        /// </summary>
        public List<ProductionDataEdit> dataEdits { get; set; }
        #endregion

        #region Constructor
        public GetEditDataResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(GetEditDataResult other)
        {
            if (other == null)
                return false;

            if (this.dataEdits.Count == other.dataEdits.Count)
            {

                for (int i = 0; i < this.dataEdits.Count; ++i)
                {
                    if (!other.dataEdits.Exists(m =>
                        m.assetId == this.dataEdits[i].assetId &&
                        m.endUTC == this.dataEdits[i].endUTC &&
                        m.startUTC == this.dataEdits[i].startUTC &&
                        m.liftNumber == this.dataEdits[i].liftNumber &&
                        m.onMachineDesignName == this.dataEdits[i].onMachineDesignName
                        ))
                        return false;
                }

                return this.Code == other.Code &&
                    this.Message == other.Message;
            }

            return false;
        }

        public static bool operator ==(GetEditDataResult a, GetEditDataResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(GetEditDataResult a, GetEditDataResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GetEditDataResult && this == (GetEditDataResult)obj;
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
