
namespace VSS.Hosted.VLCommon.ServiceContracts
{
  public sealed class ContractConstants
  {
    internal const string Namespace = "http://www.nighthawk.com/nighthawk/2008/09";
    internal const string DataNS = "http://www.nighthawk.com/nighthawk/data/2008/09";

    public const string OEMDATAFEEDNS = "http://www.trimble.com/MachineData";

    public const string NHOPNS = "http://www.nighthawk.com/nighthawk/service/NHOP/2009/10";
    public const string NHAlertNS = "http://www.nighthawk.com/nighthawk/service/NHAlert/2009/10";
    public const string NHDataNS = "http://www.nighthawk.com/nighthawk/service/NHData/2009/10";
    public const string NHBssSvc = "http://www.trimble.com/BusinessSupport";    
    public const string CatFeedbackSvcNs = "http://www.cat.com/ConnectedWorksite/11/2009";
    public const string IntegrationNS = "http://www.nighthawk.com/nighthawk/service/Integration/2009/10";
    public const string NSAccountHierarchy = NHBssSvc + "/v1/AccountHierarchy";
    public const string NSInstallBase = NHBssSvc + "/v1/InstallBase";
    public const string NSEventDetail = CatFeedbackSvcNs;
    public const string NSDeviceRegistration = NHBssSvc + "/v1/DeviceRegistration";
    public const string NSDeviceTransfer = NHBssSvc + "/v1/DeviceTransfer";
    public const string NSFirstReport = NHBssSvc + "/v1/FirstReport";
    public const string NSFirmwareVersion = NHBssSvc + "/v1/FirmwareVersion";

    public const string NSServicePlan = NHBssSvc + "/v1/ServicePlan";
    public const string NSAccountHierarchyResult = NHBssSvc + "/v1/AccountHierarchyResult";
    public const string NSInstallBaseResult = NHBssSvc + "/v1/InstallBaseResult";
    public const string NSServicePlanResult = NHBssSvc + "/v1/ServicePlanResult";
    public const string NSDeviceTransferResult = NHBssSvc + "/v1/DeviceTransferResult";

    public const string NHAPIService = "http://www.trimble.com/APIService";

    public const string NHFarmWorksSvc = NHAPIService + "/FarmWorks";
    public const string NSFarmWorksAssetV1 = NHFarmWorksSvc + "/v1/Assets";
    public const string NSFarmWorksContactV1 = NHFarmWorksSvc + "/v1/Contacts";
    public const string NSFarmWorksSiteV1 = NHFarmWorksSvc + "/v1/Sites";
    public const string NSFarmWorksAlertV1 = NHFarmWorksSvc + "/v1/Alerts";

    public const string NHAEMPSvc = NHAPIService + "/AEMP";
    public const string NSAEMPFleetV1 = NHAEMPSvc + "/v1/Fleet";

    public const string NHCATDataTopicsSvc = NHAPIService + "CATDataTopics";
    public const string NSCATDataTopicsSMULocV1 = NHCATDataTopicsSvc + "/v1/SMULoc";
    public const string NSCATDataTopicsSMULocV2 = NHCATDataTopicsSvc + "/v2/SMULoc";
    public const string NSCATDataTopicsSMULocV3 = NHCATDataTopicsSvc + "/v3/SMULoc";
    public const string NSCATDataTopicsSMULocV4 = NHCATDataTopicsSvc + "/v4/SMULoc";

    public const string NSCATDataTopicsFenceV2 = NHCATDataTopicsSvc + "/v2/FenceAlert";
    public const string NSCATDataTopicsFenceV3 = NHCATDataTopicsSvc + "/v3/FenceAlert";
    public const string NSCATDataTopicsFenceV5 = NHCATDataTopicsSvc + "/v5/Fence";
    public const string NSCATDataTopicsFenceV6 = NHCATDataTopicsSvc + "/v6/Fence";

    public const string NSCATDataTopicsEventV2 = NHCATDataTopicsSvc + "/v2/Event";
    public const string NSCATDataTopicsEventV4 = NHCATDataTopicsSvc + "/v4/Event";
    public const string NSCATDataTopicsEventV5 = NHCATDataTopicsSvc + "/v5/Event";

    public const string NSCATDataTopicsDiagnosticV2 = NHCATDataTopicsSvc + "/v2/Diagnostic";
    public const string NSCATDataTopicsDiagnosticV3 = NHCATDataTopicsSvc + "/v3/Diagnostic";
    public const string NSCATDataTopicsDiagnosticV4 = NHCATDataTopicsSvc + "/v4/Diagnostic";

    public const string NHSecuritySvc = NHAPIService + "/SecuritySvcAPI";
    public const string NSVLAPISecurityStatusV1 = NHSecuritySvc + "/v1/AssetSecurityStatus";

    public enum BSSFailureCodes
    {
      // General Failures
      ActionInvalid,
      UnexpectedException,
      DatabaseFailure,
      RecordInvalid,
      // v1 
      OmittedRelationship,
      MissingParent,
      MissingPrimaryContact,
      PrimaryLoginFailed,
      MissingOwner,
      InvalidMakeCode,
      DeviceTypeInvalid,
      ServicePlanLineIDInvalid,
      IBKeyInvalid,
      MissingAsset,
      DeviceConfigurationFailed,
      ServicePlanInvalid,
      // v1++
      InvalidParentRelationship,
      NoRelationshipFound,
      OwnershipChangeInvalid,
      TerminationDateInvalid,
      MissingOldDevice,
      MissingNewDevice,
      NewDeviceHasServices,
      ServiceNotSupported,
      MultipleOperations,
    }
  }
}
