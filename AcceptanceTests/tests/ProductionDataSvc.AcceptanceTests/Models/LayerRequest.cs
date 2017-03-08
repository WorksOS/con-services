using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Result
    public class LayerIdsExecutionResult : RequestResult, IEquatable<LayerIdsExecutionResult>
    {
        #region Members
        public LayerIdDetails[] LayerIdDetailsArray { get; set; }
        #endregion

        #region Constructor
        public LayerIdsExecutionResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(LayerIdsExecutionResult other)
        {
            if (other == null)
                return false;

            if (this.LayerIdDetailsArray.Length != other.LayerIdDetailsArray.Length)
                return false;

            for (int i = 0; i < this.LayerIdDetailsArray.Length; ++i)
            {
                if (this.LayerIdDetailsArray[i].AssetId != other.LayerIdDetailsArray[i].AssetId ||
                    this.LayerIdDetailsArray[i].DesignId != other.LayerIdDetailsArray[i].DesignId ||
                    this.LayerIdDetailsArray[i].LayerId != other.LayerIdDetailsArray[i].LayerId ||
                    this.LayerIdDetailsArray[i].StartDate != other.LayerIdDetailsArray[i].StartDate ||
                    this.LayerIdDetailsArray[i].EndDate != other.LayerIdDetailsArray[i].EndDate)
                    return false;
            }

            return this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(LayerIdsExecutionResult a, LayerIdsExecutionResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(LayerIdsExecutionResult a, LayerIdsExecutionResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is LayerIdsExecutionResult && this == (LayerIdsExecutionResult)obj;
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

    public class LayerIdDetails
    {
        public long AssetId { get; set; }
        public long DesignId { get; set; }
        public long LayerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    } 
    #endregion
}
