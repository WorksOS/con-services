using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class AssetECMInfo
    {
        /// <summary>
        /// ECM S/N 
        /// </summary>
        public string ECMSerialNumber { get; set; }

        /// <summary>
        /// Firmware P/N
        /// </summary>
        public string FirmwarePartNumber { get; set; }

        /// <summary>
        /// ECM Description
        /// </summary>
        public string ECMDescription { get; set; }

        /// <summary>
        /// Sync Clock Enabled 
        /// </summary>
        public string SyncClockEnabled { get; set; }

        /// <summary>
        /// Sync Clock Level 
        /// </summary>
        public string SyncClockLevel { get; set; }
    }
}
