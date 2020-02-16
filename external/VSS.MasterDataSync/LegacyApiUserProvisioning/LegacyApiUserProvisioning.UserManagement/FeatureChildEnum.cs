using System.Runtime.Serialization;

namespace LegacyApiUserProvisioning.UserManagement
{
    [DataContract]
    public enum FeatureChildEnum
    {
        [EnumMember]
        Alerts = 2201,
        [EnumMember]
        Groups = 2202,
        [EnumMember]
        Sites = 2203,
        [EnumMember]
        FleetSummary = 2204,
        [EnumMember]
        Imports = 2205,
        [EnumMember]
        Projects = 2206,
        [EnumMember]
        TwoDProjectMonitoring = 2207,
        [EnumMember]
        Reports = 2208,
        [EnumMember]
        EditAssetID = 2209,
        [EnumMember]
        ModifyAssetIcon = 2210,
        [EnumMember]
        MixedFleet = 2212,
        [EnumMember]
        AssetUtilization = 2501,
        [EnumMember]
        FuelUtilization = 2502,
        [EnumMember]
        DeviceConfig = 2701,
        [EnumMember]
        CATSiSApplication = 2901,
        [EnumMember]
        CATSoSApplication = 2902,
        [EnumMember]
        StartStopService = 3301,
        [EnumMember]
        FenceAlertService = 3302,
        [EnumMember]
        FuelService = 3303,
        [EnumMember]
        EventService = 3304,
        [EnumMember]
        DiagnosticService = 3305,
        [EnumMember]
        EngineParametersService = 3306,
        [EnumMember]
        DigitalSwitchStatusService = 3307,
        [EnumMember]
        SecurityService = 3308,
        [EnumMember]
        FleetSummaryService = 3309,
        [EnumMember]
        SMULocationService = 3310,
        [EnumMember]
        Provisioning = 6003,
        [EnumMember]
        BillableProvisioning = 6005

        //Changing this? The client also needs to be updated
    }
}