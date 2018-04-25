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
    public class GetMachineDesignResult : RequestResult, IEquatable<GetMachineDesignResult>
    {
        #region Members
        public List<DesignNames> designs { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public GetMachineDesignResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(GetMachineDesignResult other)
        {
            if (other == null)
                return false;

            if (this.designs.Count != other.designs.Count)
                return false;

            for (int i = 0; i < this.designs.Count; ++i)
            {
                if (!other.designs.Exists(d =>
                    d.designId == this.designs[i].designId &&
                    d.designName == this.designs[i].designName))
                    return false;
            }

            return this.Code == other.Code && this.Message == other.Message;
        }

        public static bool operator ==(GetMachineDesignResult a, GetMachineDesignResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(GetMachineDesignResult a, GetMachineDesignResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GetMachineDesignResult && this == (GetMachineDesignResult)obj;
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
