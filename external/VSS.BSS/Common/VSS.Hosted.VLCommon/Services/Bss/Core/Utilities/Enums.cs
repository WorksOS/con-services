namespace VSS.Hosted.VLCommon.Bss
{
  public enum ServiceViewActionEnum
  {
    Creation,
    Termination
  }

  public enum ResultType
  {
    Debug,
    Information,
    Warning,
    Error,      // use Error to roll back the transaction and email tier 3
    Exception,  // use Exception to roll back the transaction and email tier 3
    Notify      // use Notify to let the transaction complete and email tier 3
  }

  // Note: There is a 30 char limit
  // Do not exceed 30 chars
  public enum BssFailureCode
  {
    MessageInvalid,
    ParentInvalid,
    ParentDoesNotExist,
    RelationshipInvalid,
    RelationshipIdExists,
    CustomerExists,
    CustomerDoesNotExist,
    RelationshipIdDoesNotExist,
    ActionInvalid,
    ParentBssIdNotDefined,
    PrimaryContactInvalid,
    RelationshipIdNotDefined,
    SequenceNumberNotDefined,
    ControlNumberNotDefined,
    ActionUtcInvalid,
    CustomerNameNotDefined,
    BssIdNotDefined,
    CustomerTypeInvalid,
    HierarchyTypeInvalid,
    BssIdInvalid,
    ControlNumberInvalid,
    ParentBssIdInvalid,
    RelationshipIdInvalid,
    CustomerTypeChangeInvalid,
    IbKeyInvalid,
    OwnerBssIdInalid,
    PartNumberNotDefined,
    ModelyearInvalid,
    GpsDeviceIdNotDefined,
    EquipmentSNNotDefined,
    EquipmentVINInvalid,
    AssetExists,
    MakeCodeNotDefined,
    OwnerBssNotDefined,
    IbKeyExists,
    PartNumberDoesNotExist,
    GpsDeviceIdExists,
    OwnerBssIdDoesNotExist,
    DeviceReplaceAndOwnershipXfer,
    GpsDeviceIdDefined,
    DeviceOwnerTypeInvalid,
    IbKeyDoesNotExist,
    AssetDoesNotExist,
    GpsDeviceIdInvalid,
    DeviceXferAndOwnershipXfer,
    ActiveDeviceRegisteredDlrXfer,
    ActiveServiceExistsForDevice,
    ActiveCATDailyServiceExistsForDevice,
    ActiveVisionLinkDailyServiceExistsForDevice,
    DeviceOwnerChangedForOldAndNew,
    NewDeviceHasServices,
    DeviceReplaceNotValid,
    ServicePlanNameNotDefined,
    ServicePlanLineIdNotDefined,
    ServicePlanLineIdInvalid,
    OwnerVisibilityDateInvalid,
    OwnerVisibilityDateNotDefined,
    OwnerVisibilityDateDefined,
    ServiceCancelDateInvalid,
    ServiceCancelDateNotDefined,
    ServiceCancelDateDefined,
    ServiceExists,
    ServiceDoesNotExist,
    ServiceNotAssociatedWithDevice,
    ServiceTypeDoesNotExists,
    DeviceDoesNotSupportService,
    ServiceCancelDateBeforeActDate,
    ServceTerminationInvalid,
    ServiceTypesDoesNotMatch,
    OwnerVisDateBeforeActDate,
    SameServiceExists,
    MakeCodeInvalid,
    DeviceOwnerUnchanged,
    MergeXferDiffCustomerType,
    DeviceRegistrationStateInvalid,
    DeviceAlreadyDeRegistered,
    MergeXferDiffDealerNetwork,
    DeviceDeregNotSupported,
    DeviceRegistrationStatusInvald,
    DeviceTransferNotValid,
    DeviceRelatedToDifferentStore,
    AssetRelatedToDifferentStore
  }
}
