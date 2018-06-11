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
    /// <summary>
    /// The GET response body: Represent that have contributed TAG file information into a project.
    /// This is copied from ...\ProductionDataSvc.WebAPI\ResultHandling
    /// </summary>
    public class GetMachinesResult : RequestResult, IEquatable<GetMachinesResult>
    {
        #region Members
        public MachineStatus[] MachineStatuses { get; set; }
        #endregion

        #region Constructors
        public GetMachinesResult()
            : base("success")
        { }

        public GetMachinesResult(MachineStatus[] statuses, int code = 0, string message = "success")
            : base(code, message)
        {
            this.MachineStatuses = statuses;
        }
        #endregion

        #region Equality test
        public bool Equals(GetMachinesResult other)
        {
            if (other == null)
                return false;

            if (this.MachineStatuses.Length == other.MachineStatuses.Length)
            {
                List<MachineStatus> thisList = this.MachineStatuses.ToList();
                List<MachineStatus> otherList = other.MachineStatuses.ToList();

                for (int i = 0; i < thisList.Count; ++i)
                {
                    if (!otherList.Exists(m =>
                        m.AssetId == thisList[i].AssetId &&
                        m.MachineName == thisList[i].MachineName &&
                        m.IsJohnDoe == thisList[i].IsJohnDoe &&
                        m.lastKnownDesignName == thisList[i].lastKnownDesignName &&
                        Math.Round((double)m.lastKnownLatitude, 6) == Math.Round((double)thisList[i].lastKnownLatitude, 6) &&
                        Math.Round((double)m.lastKnownLongitude, 6) == Math.Round((double)thisList[i].lastKnownLongitude, 6) &&
                        Math.Round((double)m.lastKnownX, 6) == Math.Round((double)thisList[i].lastKnownX, 6) &&
                        Math.Round((double)m.lastKnownY, 6) == Math.Round((double)thisList[i].lastKnownY, 6) &&
                        m.lastKnownLayerId == thisList[i].lastKnownLayerId &&
                        m.lastKnownTimeStamp == thisList[i].lastKnownTimeStamp
                        ))
                        return false;
                }

                return this.Code == other.Code && 
                    this.Message == other.Message;
            }

            return false;
        }

        public static bool operator ==(GetMachinesResult a, GetMachinesResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(GetMachinesResult a, GetMachinesResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GetMachinesResult && this == (GetMachinesResult)obj;
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

    public class MachineStatus : MachineDetails
    {
        /// <summary>
        /// The design currently loaded on the machine.
        /// </summary>
        public string lastKnownDesignName { get; set; }

        /// <summary>
        /// The layer number currently loaded on the machine.
        /// </summary>
        public ushort? lastKnownLayerId { get; set; }

        /// <summary>
        /// The time the machine last reported.
        /// </summary>
        public DateTime? lastKnownTimeStamp { get; set; }

        /// <summary>
        /// The last reported position of the machine in radians.
        /// </summary>
        public double? lastKnownLatitude { get; set; }

        /// <summary>
        /// The last reported position of the machine in radians.
        /// </summary>
        public double? lastKnownLongitude { get; set; }

        /// The last reported position of the machine in grid coordinates.
        /// </summary>
        public double? lastKnownX { get; set; }

        /// <summary>
        /// The last reported position of the machine in grid coordinates.
        /// </summary>
        public double? lastKnownY { get; set; }
    }
    #endregion
}
