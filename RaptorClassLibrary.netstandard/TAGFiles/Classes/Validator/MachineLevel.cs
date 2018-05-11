using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    /// <summary>
    /// Enums for different licensing types
    /// </summary>
    public enum MachineLevel
    {
        Unknown = 0, // (No machine control subscription at level 3 or 4 or expired)
        Essentials = 1,
        ManualMaintenanceLog = 2,
        CATHealth = 3,
        StandardHealth = 4,
        CATUtilization = 5,
        StandardUtilization = 6,
        CATMAINT = 7,
        VLMAINT = 8,
        RealTimeDigitalSwitchAlerts = 9,
        e1minuteUpdateRateUpgrade = 10,
        Unused1 = 11, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
        Unused2 = 12, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
        Unused3 = 13, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
        ConnectedSiteGateway = 14,
        BasicProduction = 15, // Basic Production (Level 3)
        ProductionAndCompaction = 16, // Production and Compaction (Level 4)
        Unused4 = 17, // Introduced to ensure continuity of Delphi enum thereby enabling RTTI generation for improved logging
        Manual3DProjectMonitoring = 18 // Manual 3DPM import only subscription

    }
}
