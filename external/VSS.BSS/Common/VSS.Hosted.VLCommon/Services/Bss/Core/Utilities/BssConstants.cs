namespace VSS.Hosted.VLCommon.Bss
{
  public class BssConstants
  {  
    public const string SEQUENCE_NUMBER_NOT_DEFINED = "SequenceNumber is not defined.";
    public const string CONTROL_NUMBER_NOT_DEFINED = "ControlNumber is not defined.";
    public const string BSSID_NOT_DEFINED = "BSSID is not defined.";
    
    public const string ACTION_UTC_TOO_OLD = "ActionUTC is greater than 13 months.";
    public const string ACTION_UTC_IN_FUTURE = "ActionUTC is in the future.";
    public const string ACTION_INVALID_FOR_MESSAGE = @"Action: {0} is invalid for {1}.";

    public const string CONTROL_NUMBER_NOT_VALID = @"ControlNumber not valid.";
    public const string ACTION_NOT_VALID = @"Action not valid.";
    public const string ACTION_UTC_NOT_VALID = @"Action UTC not valid.";

    public const string IBKEY_NOT_VALID = "IBKey not valid.";
    public const string IBKEY_DOES_NOT_EXISTS = @"IBKey: {0} does not exists.";
    public const string IBKEY_NOT_DEFINED = "IBKey is not defined.";
    public const string ASSET_NOT_ASSOCIATED_WITH_DEVICE = @"No Asset is associated with {0} IBKey: {1}.";
    public const string DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER = @"Device: {0} is not associated to a valid customer.";
    public const string ACTIVE_SERVICE_EXISTS_FOR_DEVICE = @"Active Service Exists For IBKey: {0}";
    public const string ACTIVE_CATDaily_SERVICE_EXISTS_FOR_DEVICE = @"Active CATDaily Service Exists For IBKey: {0}";
    public const string ACTIVE_VisionLinkDaily_SERVICE_EXISTS_FOR_DEVICE = @"Active VisionLink Daily Service Exists For IBKey: {0}";
    public class Hierarchy
    {
      
      public const string BSSID_NOT_VALID = @"BSSID not valid.";
      public const string PARENT_BSSID_NOT_VALID = @"Parent BSSID not valid.";
      public const string RELATIONSHIPID_NOT_VALID = @"Relationship ID not valid.";
      public const string CUSTOMER_TYPE_NOT_VALID = @"Customer Type not valid.";

      public const string HIERARCHY_TYPE_INVALID = @"HierarchyType: {0} is invalid.";
      public const string CUSTOMER_TYPE_INVALID = @"CustomerType: {0} is invalid.";
      public const string RELATIONSHIP_INVALID = @"CustomerType: {0} cannot have Parent CustomerType: {1}.";
      public const string RELATIONSHIP_TO_SELF_INVALID = @"Parent and child are same organization.";
      public const string CUSTOMER_WITH_PARENT_CUSTOMER = @"Customer with ParentCustomer.";
      public const string CUSTOMER_IS_INACTIVE = @"Customer with BSSID: {0} is inactive.";
      public const string CUSTOMER_IS_ACTIVE = @"Customer with BSSID: {0} is active.";
      public const string DEALERNETWORK_INVALID = @"DealerNetwork: {0} is invalid for CustomerType: {1}.";
      public const string CUSTOMER_TYPE_CHANGE_INVALID = @"CustomerType change from {0} to {1} invalid as {2} exists.";

      public const string CUSTOMER_NAME_NOT_DEFINED = "CustomerName is not defined.";
      public const string PARENT_BSSID_NOT_DEFINED = "ParentBSSID is not defined.";
      public const string RELATIONSHIPID_NOT_DEFINED = "RelationshipID is not defined.";
      public const string DEALER_NETWORK_NOT_DEFINED = "DealerNetwork is not defined.";
      public const string NETWORK_DEALER_CODE_NOT_DEFINED = "NetworkDealerCode is not defined.";
      public const string NETWORK_CUSTOMER_CODE_NOT_DEFINED = "NetworkCustomerCode is not defined.";
      public const string DEALER_ACCOUNT_CODE_NOT_DEFINED = "DealerAccountCode is not defined.";
      public const string PRIMARY_CONTACT_NOT_DEFINED = "PrimaryContact is not defined.";
      public const string PRIMARY_CONTACT_FIRST_NAME_NOT_DEFINED = "PrimaryContact FirstName is not defined.";
      public const string PRIMARY_CONTACT_LAST_NAME_NOT_DEFINED = "PrimaryContact LastName is not defined.";
      public const string PRIMARY_CONTACT_EMAIL_NOT_DEFINED = "PrimaryContact Email is not defined.";
      public const string PRIMARY_CONTACT_EMAIL_INVALID = "Email: {0} is not valid.";
      public const string PRIMARY_CONTACT_EMAIL_DUPLICATE = "User with email: {0} already exists.";

      public const string PARENT_BSSID_DEFINED = "ParentBSSID is defined.";
      public const string DEALER_NETWORK_DEFINED = "DealerNetwork is defined.";
      public const string NETWORK_DEALER_CODE_DEFINED = "NetworkDealerCode is defined.";
      public const string NETWORK_CUSTOMER_CODE_DEFINED = "NetworkCustomerCode is defined.";
      public const string DEALER_ACCOUNT_CODE_DEFINED = "DealerAccountCode is defined.";
      public const string PRIMARY_CONTACT_DEFINED = "PrimaryContact is defined.";
      public const string RELATIONSHIPID_DEFINED = "RelationshipID is defined.";

      public const string BSSID_EXISTS = @"{0} exists for BSSID: {1}";
      public const string RELATIONSHIPID_EXISTS = @"CustomerRelationship exists with RelationshipID: {0}.";
      public const string RELATIONSHIP_TYPE_EXISTS = @"CustomerRelationship exists with CustomerType: {0}.";

      public const string BSSID_DOES_NOT_EXIST = @"{0} does not exist with BSSID: {1}";
      public const string PARENT_BSSID_DOES_NOT_EXIST = @"Parent{0} does not exist with BSSID: {1}.";
      public const string RELATIONSHIPID_DOES_NOT_EXIST = @"CustomerRelationship does not exist with RelationshipID: {0}.";
      public const string RELATIONSHIP_TYPE_DOES_NOT_EXISTS = @"CustomerRelationship does not exist with CustomerType: {0}.";
    }

    public class InstallBase
    {
      public const string IMPLIED_ACTION_IS_DEVICE_REPLACEMENT = "The implied action from BSS is Device Replacement.";
      public const string IMPLIED_ACTION_IS_DEVICE_TRANSFER = "The implied action from BSS is Device Transfer.";
      public const string OWNERSHIP_XFER_DUE_TO_MERGE_TO_DIFFERENT_DEALER_NETWORK = "Ownership Transfer initiated by Merge action is invalid to a different Dealer Network.";
      public const string OWNERSHIP_XFER_DUE_TO_MERGE_TO_DIFFERENT_CUSTOMER_TYPE = "Ownership Transfer initiated by Merge action is invalid to a different Customer Type.";


      public const string DEVICE_OWNER_TYPE_INVALID = "CustomerType: {0} cannot own devices.";
      public const string GPS_DEVICEID_DEFINED_MANUAL_DEVICE = "GPSDeviceID is defined for Manual Device type.";
      public const string DEVICE_REPLACEMENT_AND_OWNERSHIP_TRANSFER = "Attempting a Device Replacement and an Ownership Transfer in the same message.";
      public const string DEVICE_TRANSFER_AND_OWNSERSHIP_TRANSFER = "Attempting a Device Transfer and an Ownership Transfer in the same message.";
      public const string DEVICE_WITH_ACTIVE_SERVICE_TRANSFER_TO_DIFFERENT_REGISTERED_DEALER = "Device with active Service attempted an Ownership Transfer to a different registered dealer.";
      public const string DEVICE_OWNER_NOT_UPDATED_DURING_MERGE = "Device OwnerBssID must be updated during a merge operation.";
      public const string OWNER_BSSID_NOT_VALID = "OwnerBssId not valid.";
      public const string FIRMWARE_VERSION_ID_NOT_VALID = "FirmwareVersionID not valid.";
      public const string MODEL_YEAR_NOT_VALID = "ModelYear not valid.";
      public const string MAKE_CODE_NOT_VALID = "MakeCode: {0} is invalid.";

      public const string GPS_DEVICEID_NOT_DEFINED = "GPSDeviceID is not defined.";
      public const string EQUIPMENTSN_NOT_DEFINED = "Equipment Serial Number is not defined.";
      public const string EQUIPMENTVIN_TOO_LONG = "Equipment VIN exceeds 50 characters";
      public const string MAKE_CODE_NOT_DEFINED = "MakeCode is not defined.";
      public const string OWNER_BSSID_NOT_DEFINED = "OwnerBSSID is not defined.";
      public const string PART_NUMBER_NOT_DEFINED = "PartNumber is not defined.";

      public const string IBKEY_EXISTS = @"IBKey: {0} already exists.";
      public const string GPS_DEVICEID_EXISTS = @"GPSDeviceID: {0} exists.";
      public const string ASSET_EXISTS = @"Asset with SerialNumber: {0} and MakeCode: {1} already exists.";

      public const string ASSET_DOES_NOT_EXISTS = @"Asset: {0} does not exists.";

      public const string PART_NUMBER_DOES_NOT_EXIST = @"PartNumber: {0} does not exist.";
      public const string OWNER_BSSID_DOES_NOT_EXIST = @"Device owner with BSSID: {0} does not exist.";

      public const string GPS_DEVICEIDS_DO_NOT_MATCH = @"The existing GpsDeviceId: {0} and the GpsDeviceId: {1} defined in the InstallBase message don't match.";
      public const string ACTIVE_SERVICE_EXISTS_FOR_DEVICE_ACTION_NOT_VALID = @"Attempting a Device {0} for IBKey: {1}. Device has one or more active services.";
      public const string BSS_DEVICE_UNAUTHORIZED = @"BSS Unauthorized for IBKey: {0}";
      public const string BSS_ASSET_UNAUTHORIZED = @"BSS Unauthorized for Asset with SerialNumber: {0} and MakeCode {1}";
    }

    public class DeviceReplacement
    {
      public const string OLD_DEVICE_DOES_NOT_HAVE_ACTIVE_SERVICE = @"The Old Device does not have an active core Service.";
      public const string NEW_DEVICE_NOT_INSTALLED_OR_OLD_DEVICE_NOT_REMOVED = @"The New Device has not been installed on the Asset or the Old Device has not been removed. This could occur if the DeviceReplacement is not preceeded by an InstallBase.";
      public const string OLD_IBKEY_AND_NEW_IBKEY_ARE_EQUAL = @"The Old IBKey: {0} and the New IBKey: {1} are same and hence the Device Replacement can't be performed.";
      public const string IBKEY_DOES_NOT_EXISTS = @"{1} IBKey: {0} does not exists.";
      public const string OWNER_BSSID_DIFFERENT_FOR_OLDIBKEY_AND_NEWIBKEY = @"OwnerBssID is different for Old IBKey: {0} and new IBKey: {1}";
      public const string NEW_DEVICE_HAS_ACTIVE_SERVICES = @"New IB Key: {0} already has services";
      public const string NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES = @"Device replace not valid as the New IBKey: {0} doesn't support the services held by the Old IBKey: {1}";
      public const string DEVICE_SWAP_NOT_VALID = @"Device swap not valid as the old Device Type: {0} is not same as the new Device Type: {1}.";
    }

    public class ServicePlan
    {
      public const string SERVICE_PLAN_NAME_NOT_DEFINED = @"Service Plan Name Defined.";
      public const string SERVICE_TERMINATION_DATE_INVALID = @"Service Termination Date not valid.";
      public const string SERVICE_PLAN_LINE_ID_NOT_DEFINED = @"Service Plan Line ID not Defined.";
      public const string SERVICE_PLAN_LINE_ID_INVALID = @"Service Plan Line ID not valid.";
      public const string OWNER_VISIBILITY_DATE_INVALID = @"Owner Visibility Date not valid.";
      public const string OWNER_VISIBILITY_DATE = @"Owner Visibility Date {0} Defined for Action {1}.";
      public const string SERVICE_TERMINATION_DATE = @"Service Termination Date {0} Defined for Action {1}.";
      public const string SERVICE_EXISTS = @"Service: {0} already exists";
      public const string SERVICE_DOES_NOT_EXISTS = @"Service: {0} does not exists";
      public const string SERVICE_NOT_ASSOCIATED_WITH_DEVICE = @"Service Plan Line ID {0} belongs to device SN {1} with IBKey {2}";
      public const string SERVICE_TYPE_DOES_NOT_EXISTS = "No Service Type Exists for Service Name: {0}.";
      public const string SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE = @"The Service Type: {0} is not supported for the Device Type: {1}.";
      public const string SERVICE_TERMINATION_DATE_IS_PRIOR_TO_ACTIVATION_DATE = @"Service termination date: {0} is prior to activation date:{1}.";
      public const string SERVICE_TERMINATION_NOT_VALID = @"Service termination on: {0} is not valid. The service is already terminated on: {1}";
      public const string SERVICE_TYPES_ARE_NOT_EQUAL = @"Service Type: {0} of the Service Plan Line ID: {1} is not same as the Service Type: {2} exists on the device.";
      public const string OWNER_VISIBILITY_DATE_IS_PRIOR_TO_ACTIVATION_DATE = @"Owner visibility date: {0} is prior to service activation date: {1}.";
      public const string DEVICE_NOT_ASSOCIATED_WITH_ASSET = @"Device: {0} is not associated with any asset.";
      public const string DEVICE_HAS_SAME_ACTIVE_SERVICE = @"The Service: {0} is already activated with ServicePlanLineID: {2} on this Device: {1} and it is still active.";
      public const string ACTION_NOTALLOWED_ON_CANCELLEDSERVICE = @"Action not allowed on cancelled services. Service Plan ID : {0}";
    }

    public class DeviceRegistration
    {
      public const string DEVICE_REGISTRATION_STATUS_NOT_VALID = @"Device Registration Status: {0} not valid.";
      public const string DEVICE_ALREADY_DEREGISTERED = @"The Request is invalid as the Device: {0} already {1}.";
      public const string DEVICE_REGISTRATION_NOT_SUPPORTED = @"Device type: {0} does not support the device (de)registration.";
      public const string DEVICE_STATUS_NOT_VALID = @"Status: {0} not valid for device {1} action.";
    }
  }
}
