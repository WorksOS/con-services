using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// Description to identify a design file either by id or by its location in TCC.
    /// </summary>
    public class DesignDescriptor : IEquatable<DesignDescriptor>
    {
        #region Members
        /// <summary>
        /// The id of the design file
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// The description of where the file is located.
        /// </summary>
        public FileDescriptor file { get; set; }

        /// <summary>
        /// The offset in meters to use for a reference surface. The surface in the file will be offset by this amount.
        /// Only applicable when the file is a surface design file.
        /// </summary>
        public double offset { get; set; } 
        #endregion

        #region Equality test
        public bool Equals(DesignDescriptor other)
        {
            if (other == null)
                return false;

            return this.id == other.id &&
                this.file == other.file &&
                this.offset == other.offset;
        }

        public static bool operator ==(DesignDescriptor a, DesignDescriptor b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(DesignDescriptor a, DesignDescriptor b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is DesignDescriptor && this == (DesignDescriptor)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}