using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// A representation of a machine in a Raptor project.
    /// </summary>
    public class MachineDetails : IEquatable<MachineDetails>
    {
        #region Members
        /// <summary>
        /// The ID of the machine/asset
        /// </summary>
        public long assetID { get; set; }

        /// <summary>
        /// The textual name of the machine
        /// </summary>
        public string machineName { get; set; }

        /// <summary>
        /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
        /// </summary>
        public bool isJohnDoe { get; set; } 
        #endregion

        #region Equality test
        public bool Equals(MachineDetails other)
        {
            if (other == null)
                return false;

            return this.assetID == other.assetID &&
                this.machineName == other.machineName &&
                this.isJohnDoe == other.isJohnDoe;
        }

        public static bool operator ==(MachineDetails a, MachineDetails b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(MachineDetails a, MachineDetails b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is MachineDetails && this == (MachineDetails)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
