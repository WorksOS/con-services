using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// Description to identify a file by its location in TCC.
    /// </summary>
    public class FileDescriptor : IEquatable<FileDescriptor>
    {
        #region Members
        /// <summary>
        /// The id of the filespace in TCC where the file is located.
        /// </summary>
        public string filespaceId { get; set; }

        /// <summary>
        /// The full path of the file.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string fileName { get; set; } 
        #endregion

        #region Equality test
        public bool Equals(FileDescriptor other)
        {
            if (other == null)
                return false;

            return this.filespaceId == other.filespaceId &&
                this.path == other.path &&
                this.fileName == other.fileName;
        }

        public static bool operator ==(FileDescriptor a, FileDescriptor b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(FileDescriptor a, FileDescriptor b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is FileDescriptor && this == (FileDescriptor)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}