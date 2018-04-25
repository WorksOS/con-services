using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// Request to do operations on design file in design cache
    /// </summary>
    class DesignNameRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Description to identify a design file in DesignCache.
        /// </summary>
        public string DesignFilename { get; set; }
    } 
    #endregion
}
