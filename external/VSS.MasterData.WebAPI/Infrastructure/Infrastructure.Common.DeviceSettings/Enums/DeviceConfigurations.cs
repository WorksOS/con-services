namespace Infrastructure.Common.DeviceSettings.Enums
{
    public enum AssetSecurityStatus
    {
        NormalOperation = 0,
        Derated =1,
        Disable = 2
    }

    public enum ReportingScheduleHourMeterFuelReport
    {
        Fuel = 1,
        Runtime,
        Both,
        Off
    }

    public enum MovingThresholdSettings
    {
        UseOnRoadDefaults = 1,
        UseOffRoadDefaults = 2
    }

    public enum FaultCodeReportingEventSeverity
    {
        Immediately = 1,
        Next = 2,
        Never = 3
    }

    public enum ReportingScheduleDailyLocationReportingFrequency
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4
    }

    public enum SwitchActiveState
    {
        NormallyOpen = 1,
        NormallyClosed = 2,
        NotInstalled = 3,
        NotConfigured=4
    };

    public enum SwitchMonitoringStatus
    {
        KeyOffEngineOff = 1,
        KeyOnEngineOff = 2,
        KeyOnEngineOn = 3,
        Always=4
    }

    public enum SwitchCaption
    {
        Unconfigured=1,
        Configured,
        Open,
        Close
    }

    public enum MetersSmhOdometerConfig
    {
        RTERM_AC_50HZ = 0, // Alternate Sources  with R-term AC input  (using 50Hz as frequency threshold)
                           //  Note: (if it is selected, the SMH & Engine start/stop will be from be from R-term AC with 50Hz threshold as default for R-term, 
                           //  the source of ?Odometer/Distance? travel will be from GPS).
        J1939 = 1, // default setting for DataLink
        RTERM_DC_4S = 2, // Alternate source with R-term DC input (using 4 seconds denounce for the DC input)
                         // Note: if it is selected, the SMH & Engine start/stop will be from be from R-term DC, 
                         // the source of ?Odometer/Distance? travel will be coming from GPS).
        RTERM_DC_6S = 3, // Alternate source with R-term DC input (using 6s denounce)
        RTERM_AC_30HZ = 4, // Alternate source with R-term AC input (30Hz as frequency threshold)
        RTERM_AC_70HZ = 5
    }

    public enum SwitchType
    {
        SingleStateSwitch = 1,
        DualStateSwitch
    }

    public enum ParameterName
    {
        Status = 9,
        MaintenanceModeDuration = 11
    }
}
