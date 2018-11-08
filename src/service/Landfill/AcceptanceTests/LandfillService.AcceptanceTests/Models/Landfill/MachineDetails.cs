using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    public class MachineDetails
    {
        public int id;
        public long assetId;
        public string machineName;
        public bool isJohnDoe;

        #region Equality test
        public bool Equals(MachineDetails other)
        {
            if (other == null)
                return false;

            return this.id == other.id &&
                this.assetId == other.assetId &&
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

    /// <summary>
    /// Machine Lift/Layer Details returned from the Raptor API
    /// </summary>
    public class MachineLiftDetails : MachineDetails
    {
        public LiftDetails[] lifts { get; set; }

        #region Equality test
        public bool Equals(MachineLiftDetails other)
        {
            if (other == null)
                return false;

            if (!((this as MachineDetails) == (other as MachineDetails) &&
                this.lifts.Length == other.lifts.Length))
                return false;

            for(int i = 0; i < this.lifts.Length; ++i)
            {
                if(this.lifts[i] != other.lifts[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(MachineLiftDetails a, MachineLiftDetails b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(MachineLiftDetails a, MachineLiftDetails b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is MachineLiftDetails && this == (MachineLiftDetails)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Lift details returned from the Raptor API
    /// </summary>
    public class LiftDetails
    {
        public int layerId { get; set; }
        public DateTime endUtc { get; set; }

        #region Equality test
        public bool Equals(LiftDetails other)
        {
            if (other == null)
                return false;

            return this.layerId == other.layerId && this.endUtc == other.endUtc;
        }

        public static bool operator ==(LiftDetails a, LiftDetails b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(LiftDetails a, LiftDetails b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is LiftDetails && this == (LiftDetails)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
