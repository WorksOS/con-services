using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProjectSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation used to request a local copy of a file from TCC.
    /// </summary>
    public class FileAccessRequest
    {
        /// <summary>
        /// The description of where the file is located in TCC.
        /// </summary>
        public FileDescriptor file { get; set; }

        /// <summary>
        /// The description of where to put the copy of the file.
        /// </summary>
        public string localPath { get; set; }
    } 
    #endregion
}
