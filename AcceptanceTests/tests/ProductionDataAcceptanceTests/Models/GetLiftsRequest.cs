using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    public class LiftIdsRequest
    {
        /// <summary>
        /// Sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public long projectId { get; set; }

        /// <summary>
        /// Sets the design identifier.
        /// </summary>
        /// <value>
        /// The design identifier.
        /// </value>
        public long designId { get; set; }

        /// <summary>
        /// Sets the startdate.
        /// </summary>
        /// <value>
        /// The startdate.
        /// </value>
        public DateTime startdate { get; set; }

        /// <summary>
        /// Sets the enddate.
        /// </summary>
        /// <value>
        /// The enddate.
        /// </value>
        public DateTime enddate { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// The represenation of the results of an edit data request.
    /// </summary>
    public class LiftIdsResponse : RequestResult, IEquatable<LiftIdsResponse>
    {
        #region Members
        /// <summary>
        /// The collection of data edits applied to the production data.
        /// </summary>
        public List<LayerDescriptor> LayerIdDetailsArray { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor - success by default
        /// </summary>
        protected LiftIdsResponse()
            : base("success")
        {
        }
        #endregion

        #region Equality test
        public bool Equals(LiftIdsResponse other)
        {
            if (other == null)
                return false;

            if (this.LayerIdDetailsArray.Count == other.LayerIdDetailsArray.Count)
            {
                for (int i = 0; i < this.LayerIdDetailsArray.Count; ++i)
                {
                    if (!other.LayerIdDetailsArray.Exists(m =>
                        m.AssetId == this.LayerIdDetailsArray[i].AssetId &&
                        m.DesignId == this.LayerIdDetailsArray[i].DesignId &&
                        m.LayerId == this.LayerIdDetailsArray[i].LayerId &&
                        m.EndDate == this.LayerIdDetailsArray[i].EndDate &&
                        m.StartDate == this.LayerIdDetailsArray[i].StartDate
                        ))
                        return false;
                }

                return this.Code == other.Code &&
                    this.Message == other.Message;
            }

            return false;
        }

        public static bool operator ==(LiftIdsResponse a, LiftIdsResponse b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(LiftIdsResponse a, LiftIdsResponse b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is LiftIdsResponse && this == (LiftIdsResponse)obj;
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
        /// <returns>A string representation of the array of passcount percentages</returns>
        public override string ToString()
        {
            return String.Format("LayerIdDetailsArray: [{0}]", string.Join(",", LayerIdDetailsArray));
        }
        #endregion
    }

    public class LayerDescriptor
    {
        #region Members
        public long AssetId { get; set; }
        public long DesignId { get; set; }
        public long LayerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of the LayerDescriptor</returns>
        public override string ToString()
        {
            return "{" + String.Format("AssetId:{0}, DesignId:{1}, LayerId:{2}, StartDate:{3}, EndDate:{4}",
                                  AssetId, DesignId, LayerId, StartDate, EndDate) + "}";
        }
        #endregion
    } 
    #endregion
}
