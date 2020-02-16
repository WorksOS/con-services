using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  public class BSS
  {

    #region AccountHierarchy

    public static AccountHierarchyBuilder AH(ActionEnum actionEnum)
    {
      return new AccountHierarchyBuilder(actionEnum);
    }
    public static AccountHierarchyBuilder AHCreated
    {
      get { return AH(ActionEnum.Created); }
    }
    public static AccountHierarchyBuilder AHUpdated
    {
      get { return AH(ActionEnum.Updated); }
    }
    public static AccountHierarchyBuilder AHDeleted
    {
      get { return AH(ActionEnum.Deleted); }
    }
    public static AccountHierarchyBuilder AHDeactivated
    {
      get { return AH(ActionEnum.Deactivated); }
    }
    public static AccountHierarchyBuilder AHReactivated
    {
      get { return AH(ActionEnum.Reactivated); }
    }

    #endregion

    #region InstallBase

    public static InstallBaseBuilder IB(ActionEnum actionEnum)
    {
      return new InstallBaseBuilder(actionEnum);
    }

    public static InstallBaseBuilder IBCreated
    {
      get { return IB(ActionEnum.Created); }
    }

    public static InstallBaseBuilder IBUpdated
    {
      get { return IB(ActionEnum.Updated); }
    }

    public static InstallBaseBuilder IBUpdatedMerge
    {
      get { return IB(ActionEnum.UpdatedMerge); }
    }

    #endregion

    #region DeviceReplacement

    public static DeviceReplacementBuilder DR(ActionEnum actionEnum)
    {
      return new DeviceReplacementBuilder(actionEnum);
    }

    public static DeviceReplacementBuilder DRReplaced
    {
      get { return DR(ActionEnum.Replaced); }
    }

    public static DeviceReplacementBuilder DRSwapped
    {
      get { return DR(ActionEnum.Swapped); }
    }

    #endregion

    #region Service Plan

    public static ServicePlanBuilder SP(ActionEnum actionEnum)
    {
      return new ServicePlanBuilder(actionEnum);
    }
    public static ServicePlanBuilder SPActivated
    {
      get { return SP(ActionEnum.Activated); }
    }
    public static ServicePlanBuilder SPCancelled
    {
      get { return SP(ActionEnum.Cancelled); }
    }
    public static ServicePlanBuilder SPUpdated
    {
      get { return SP(ActionEnum.Updated); }

    }
    #endregion

    #region Device Registration

    public static DeviceRegistrationBuilder DRB(ActionEnum actionEnum)
    {
      return new DeviceRegistrationBuilder(actionEnum);
    }

    public static DeviceRegistrationBuilder DRBRegistered
    {
      get { return DRB(ActionEnum.Registered); }
    }

    public static DeviceRegistrationBuilder DRBDeRegistered
    {
      get { return DRB(ActionEnum.Deregistered); }
    }
    #endregion
  }

  public abstract class BssBuilder<TBuilder>
  {
    protected TBuilder Builder;
    protected ActionEnum _action;
    protected string _targetStack = "TestStack01";
    protected long _sequenceNumber = IdGen.GetId();
    protected string _controlNumber = IdGen.GetId().ToString();
    protected DateTime _actionUTC = DateTime.UtcNow;

    protected BssBuilder(ActionEnum action)
    {
      _action = action;
    }
    public TBuilder TargetStack(string targetStack)
    {
      _targetStack = targetStack;
      return Builder;
    }
    public TBuilder SequenceNumber(long sequenceNumber)
    {
      _sequenceNumber = sequenceNumber;
      return Builder;
    }
    public TBuilder ControlNumber(string controlNumber)
    {
      _controlNumber = controlNumber;
      return Builder;
    }
    public TBuilder ActionUtc(DateTime actionUtc)
    {
      _actionUTC = actionUtc;
      return Builder;
    }
  }

  public class AccountHierarchyBuilder : BssBuilder<AccountHierarchyBuilder>
  {
    private AccountHierarchy.BSSCustomerTypeEnum _customerType = AccountHierarchy.BSSCustomerTypeEnum.DEALER;

    private string _customerName = "CUSTOMER_NAME";
    private string _bssId = IdGen.GetId().ToString();
    private string _parentBssId;
    private string _relationshipId;
    private string _dealerNetwork = "NONE";
    private string _networkCustomerCode = "NETWORK_CUSTOMER_CODE";
    private string _networkDealerCode = "NETWORK_DEALER_CODE";
    private string _dealerAccountCode = "DEALER_ACCOUNT_CODE";
    private string _hierarchyType = "TCS Dealer";
    private PrimaryContact _contact = new PrimaryContact();

    public AccountHierarchyBuilder(ActionEnum action)
      : base(action)
    {
      Builder = this;
    }
    public AccountHierarchyBuilder CustomerType(AccountHierarchy.BSSCustomerTypeEnum bssCustomerType)
    {
      _customerType = bssCustomerType;
      return this;
    }
    public AccountHierarchyBuilder ForDealer()
    {
      _customerType = AccountHierarchy.BSSCustomerTypeEnum.DEALER;
      _networkCustomerCode = null;
      _dealerAccountCode = null;

      return this;
    }
    public AccountHierarchyBuilder ForCustomer()
    {
      _customerType = AccountHierarchy.BSSCustomerTypeEnum.CUSTOMER;
      _dealerNetwork = null;
      _networkCustomerCode = null;
      _networkDealerCode = null;
      _dealerAccountCode = null;
      _hierarchyType = "TCS Customer";

      return this;
    }
    public AccountHierarchyBuilder ForAccount()
    {
      _customerType = AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT;
      _dealerNetwork = null;
      _networkDealerCode = null;

      _parentBssId = IdGen.GetId().ToString();
      _relationshipId = IdGen.GetId().ToString();

      return this;
    }
    public AccountHierarchyBuilder Name(string customerName)
    {
      _customerName = customerName;
      return this;
    }
    public AccountHierarchyBuilder BssId(string bssId)
    {
      _bssId = bssId;
      return this;
    }
    public AccountHierarchyBuilder ParentDefined()
    {
      ParentBssId(IdGen.GetId().ToString());
      RelationshipId(IdGen.GetId().ToString());
      return this;
    }
    public AccountHierarchyBuilder ParentBssId(string parentBssId)
    {
      _parentBssId = parentBssId;
      return this;
    }
    public AccountHierarchyBuilder RelationshipId(string relationshipId)
    {
      _relationshipId = relationshipId;
      return this;
    }
    public AccountHierarchyBuilder DealerNetwork(string dealerNetwork)
    {
      _dealerNetwork = dealerNetwork;
      return this;
    }
    public AccountHierarchyBuilder NetworkDealerCode(string networkDealerCode)
    {
      _networkDealerCode = networkDealerCode;
      return this;
    }
    public AccountHierarchyBuilder NetworkCustomerCode(string networkCustomerCode)
    {
      _networkCustomerCode = networkCustomerCode;
      return this;
    }
    public AccountHierarchyBuilder DealerAccountCode(string dealerAccountCode)
    {
      _dealerAccountCode = dealerAccountCode;
      return this;
    }
    public AccountHierarchyBuilder HierarchyType(string hierarchyType)
    {
      _hierarchyType = hierarchyType;
      return this;
    }
    public AccountHierarchyBuilder ContactDefined()
    {
      _contact = new PrimaryContact { FirstName = "FirstName", LastName = "LastName", Email = "email@domain.com" };
      return this;
    }
    public AccountHierarchyBuilder ContactNotDefined()
    {
        _contact = null;
        return this;
    }
    public AccountHierarchyBuilder FirstName(string firstName)
    {
      (_contact ?? new PrimaryContact()).FirstName = firstName;
      return this;
    }
    public AccountHierarchyBuilder LastName(string lastName)
    {
      (_contact ?? new PrimaryContact()).LastName = lastName;
      return this;
    }
    public AccountHierarchyBuilder Email(string email)
    {
      (_contact ?? new PrimaryContact()).Email = email;
      return this;
    }

    public AccountHierarchy Build()
    {
      return new AccountHierarchy
      {
        TargetStack = _targetStack,
        SequenceNumber = _sequenceNumber,
        ControlNumber = _controlNumber,
        Action = _action.ToString(),
        ActionUTC = _actionUTC.ToString(),
        CustomerName = _customerName,
        BSSID = _bssId,
        ParentBSSID = _parentBssId,
        RelationshipID = _relationshipId,
        CustomerType = _customerType.ToString(),
        DealerNetwork = _dealerNetwork,
        NetworkCustomerCode = _networkCustomerCode,
        NetworkDealerCode = _networkDealerCode,
        DealerAccountCode = _dealerAccountCode,
        HierarchyType = _hierarchyType,
        contact = _contact,
      };
    }
  }

  public class InstallBaseBuilder : BssBuilder<InstallBaseBuilder>
  {
    private string _ownerBssId = IdGen.StringId();
    private string _partNumber = IdGen.StringId();
    private string _equipmentSn = IdGen.StringId();
    private string _equipmentVin = IdGen.StringId();
    private string _simSerialNumber = IdGen.StringId();
    private string _simState = "SIM_STATE";
    private string _cellularModemIMEA = IdGen.StringId();
    private string _equipmentLabel = "NAME_" + IdGen.StringId();
    private string _firmwareVersionId = IdGen.StringId();
    private string _gpsDeviceId = IdGen.StringId();
    private string _ibkey = IdGen.StringId();
    private string _makeCode = "CAT";
    private string _model = "MODEL";
    private string _modelyear = DateTime.Now.Year.ToString();
    private string _deviceActive;
    private string _previousDeviceActive;
    private string _previousEquipmentSn;
    private string _previousMakeCode;

    public InstallBaseBuilder(ActionEnum action)
      : base(action)
    {
      Builder = this;
    }

    public InstallBaseBuilder OwnerBssId(string ownerBssId)
    {
      _ownerBssId = ownerBssId;
      return this;
    }

    public InstallBaseBuilder PartNumber(string partNumber)
    {
      _partNumber = partNumber;
      return this;
    }

    public InstallBaseBuilder EquipmentSN(string equipmentSn)
    {
      _equipmentSn = equipmentSn;
      return this;
    }

    public InstallBaseBuilder EquipmentVIN(string equipmentVin)
    {
      _equipmentVin = equipmentVin;
      return this;
    }

    public InstallBaseBuilder SimSerialNumber(string simSerialNumber)
    {
      _simSerialNumber = simSerialNumber;
      return this;
    }

    public InstallBaseBuilder SimState(string simState)
    {
      _simState = simState;
      return this;
    }

    public InstallBaseBuilder CellularModemIMEA(string cellularModemIMEA)
    {
      _cellularModemIMEA = cellularModemIMEA;
      return this;
    }

    public InstallBaseBuilder EquipmentLabel(string equipmentLabel)
    {
      _equipmentLabel = equipmentLabel;
      return this;
    }

    public InstallBaseBuilder FirmwareVersionId(string firmwareVersionId)
    {
      _firmwareVersionId = firmwareVersionId;
      return this;
    }

    public InstallBaseBuilder GpsDeviceId(string gpsDeviceId)
    {
      _gpsDeviceId = gpsDeviceId;
      return this;
    }

    public InstallBaseBuilder IBKey(string ibkey)
    {
      _ibkey = ibkey;
      return this;
    }

    public InstallBaseBuilder MakeCode(string makeCode)
    {
      _makeCode = makeCode;
      return this;
    }

    public InstallBaseBuilder Model(string model)
    {
      _model = model;
      return this;
    }

    public InstallBaseBuilder ModelYear(string modelyear)
    {
      _modelyear = modelyear;
      return this;
    }

    public InstallBaseBuilder ImplyDeviceTransfer()
    {
      return ImplyDeviceTransfer(IdGen.StringId(), "CAT");
    }

    public InstallBaseBuilder ImplyDeviceTransfer(string prevEquipmentSn, string prevMakeCode)
    {
      _deviceActive = "NOTACTIVE";
      _previousDeviceActive = "NOTACTIVE";
      _previousEquipmentSn = prevEquipmentSn;
      _previousMakeCode = prevMakeCode;
      return this;
    }

    public InstallBaseBuilder ImplyDeviceReplacement()
    {
      return ImplyDeviceReplacement(IdGen.StringId(), "CAT");
    }

    public InstallBaseBuilder ImplyDeviceReplacement(string prevEquipmentSn, string prevMakeCode)
    {
      _deviceActive = "ACTIVE";
      _previousDeviceActive = "NOTACTIVE";
      _previousEquipmentSn = prevEquipmentSn;
      _previousMakeCode = prevMakeCode;
      return this;
    }

    public InstallBaseBuilder DeviceActive()
    {
      _deviceActive = "ACTIVE";
      return this;
    }

    public InstallBaseBuilder DeviceNotActive()
    {
      _deviceActive = "NOTACTIVE";
      return this;
    }

    public InstallBaseBuilder PrevEquipmentSN(string prevEquipmentSn)
    {
      _previousEquipmentSn = prevEquipmentSn;
      return this;
    }

    public InstallBaseBuilder PrevMakeCode(string prevMakeCode)
    {
      _previousMakeCode = prevMakeCode;
      return this;
    }

    public InstallBase Build()
    {
      return new InstallBase
      {
        TargetStack = _targetStack,
        Action = _action.ToString(),
        ActionUTC = _actionUTC.ToString(),
        ControlNumber = _controlNumber,
        SequenceNumber = _sequenceNumber,
        OwnerBSSID = _ownerBssId,
        PartNumber = _partNumber,
        EquipmentSN = _equipmentSn,
        EquipmentVIN = _equipmentVin,
        SIMSerialNumber = _simSerialNumber,
        SIMState = _simState,
        CellularModemIMEA = _cellularModemIMEA,
        EquipmentLabel = _equipmentLabel,
        FirmwareVersionID = _firmwareVersionId,
        GPSDeviceID = _gpsDeviceId,
        IBKey = _ibkey,
        MakeCode = _makeCode,
        Model = _model,
        ModelYear = _modelyear,
        DeviceState = _deviceActive,
        PreviousDeviceState = _previousDeviceActive,
        PreviousEquipmentSN = _previousEquipmentSn,
        PreviousMakeCode = _previousMakeCode
      };
    }
  }

  public class DeviceReplacementBuilder : BssBuilder<DeviceReplacementBuilder>
  {
    private string _oldIBKey = IdGen.GetId().ToString();
    private string _newIBKey = IdGen.GetId().ToString();

    public DeviceReplacementBuilder(ActionEnum action)
      : base(action)
    {
      Builder = this;
    }

    public DeviceReplacementBuilder OldIBKey(string oldIBKey)
    {
      _oldIBKey = oldIBKey;
      return this;
    }

    public DeviceReplacementBuilder NewIBKey(string newIBKey)
    {
      _newIBKey = newIBKey;
      return this;
    }

    public DeviceReplacement Build()
    {
      return new DeviceReplacement
      {
        Action = _action.ToString(),
        ActionUTC = _actionUTC.ToString(),
        ControlNumber = _controlNumber,
        NewIBKey = _newIBKey,
        OldIBKey = _oldIBKey,
        SequenceNumber = _sequenceNumber,
        TargetStack = _targetStack
      };
    }
  }

  public class ServicePlanBuilder : BssBuilder<ServicePlanBuilder>
  {
    private string _servicePlanName = IdGen.GetId().ToString();
    private DateTime? _serviceTerminationDate = DateTime.UtcNow;
    private string _servicePlanlineID = IdGen.GetId().ToString();
    private string _ibKey = IdGen.GetId().ToString();
    private DateTime? _ownerVisibilityDate = DateTime.UtcNow;

    public ServicePlanBuilder(ActionEnum action)
      : base(action)
    {
      if (action != ActionEnum.Cancelled)
        _serviceTerminationDate = null;

      if (action == ActionEnum.Cancelled)
        _ownerVisibilityDate = null;

      Builder = this;
    }

    public ServicePlanBuilder ServicePlanName(string servicePlanName)
    {
      _servicePlanName = servicePlanName;
      return this;
    }

    public ServicePlanBuilder ServiceTerminationDate(DateTime? serviceTerminationDate)
    {
      _serviceTerminationDate = serviceTerminationDate;
      return this;
    }

    public ServicePlanBuilder ServicePlanlineID(string servicePlanlineID)
    {
      _servicePlanlineID = servicePlanlineID;
      return this;
    }

    public ServicePlanBuilder IBKey(string ibkey)
    {
      _ibKey = ibkey;
      return this;
    }

    public ServicePlanBuilder OwnerVisibilityDate(DateTime? ownerVisibilityDate)
    {
      _ownerVisibilityDate = ownerVisibilityDate;
      return this;
    }

    public ServicePlan Build()
    {
      return new ServicePlan
      {
        Action = _action.ToString(),
        ActionUTC = _actionUTC.ToString(),
        ControlNumber = _controlNumber,
        SequenceNumber = _sequenceNumber,
        TargetStack = _targetStack,
        IBKey = _ibKey,
        ServicePlanName = _servicePlanName,
        ServiceTerminationDate = _serviceTerminationDate.ToString(),
        ServicePlanlineID = _servicePlanlineID,
        OwnerVisibilityDate = _ownerVisibilityDate.ToString()
      };
    }
  }

  public class DeviceRegistrationBuilder : BssBuilder<DeviceRegistrationBuilder>
  {
    private string _ibkey = IdGen.StringId();
    private DeviceRegistrationStatusEnum _status = DeviceRegistrationStatusEnum.DEREG_STORE;

    public DeviceRegistrationBuilder(ActionEnum actionEnum) : base(actionEnum)
    {
      if (actionEnum == ActionEnum.Registered)
        _status = DeviceRegistrationStatusEnum.REG;
      else
        _status = DeviceRegistrationStatusEnum.DEREG_STORE;
      Builder = this;
    }

    public DeviceRegistrationBuilder IBKey(string ibkey)
    {
      _ibkey = ibkey;
      return this;
    }

    public DeviceRegistrationBuilder Status(DeviceRegistrationStatusEnum status)
    {
      _status = status;
      return this;
    }

    public DeviceRegistration Build()
    {
      return new DeviceRegistration
      {
        Action = _action.ToString(),
        ActionUTC = _actionUTC.ToString(),
        ControlNumber = _controlNumber,
        SequenceNumber = _sequenceNumber,
        TargetStack = _targetStack,
        IBKey = _ibkey,
        Status = _status.ToString(),
      };
    }
  }
}
