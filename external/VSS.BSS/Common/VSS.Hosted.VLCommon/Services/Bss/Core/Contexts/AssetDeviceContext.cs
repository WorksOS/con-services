using System;

namespace VSS.Hosted.VLCommon.Bss
{
  /// <summary>
  /// Represents the current state and proposed states of 
  /// Assets, Devices, and Ownership in VL.
  /// 
  /// "IBAsset" property is the proposed state of the asset (as defined by the IB message)
  /// "Asset" property is the Asset, if it exists (by Make/SN) or is created, for the IBAsset. 
  /// 
  /// "IBDevice" property is the proposed state of the device (as defined by the IB message)
  /// "Device" property is the Device, if it exists (by IBKey) or is created, for the IBDevice
  /// 
  /// "Owner" property is the existing/proposed owner (as defined by the IB message)
  /// </summary>
  public class AssetDeviceContext
  {
    private bool? _isDeviceBeingInstalled;
    private bool? _isDeviceReplacement;
    private bool? _isDeviceTransfer;
    private bool? _isOwnershipTransfer;
    private bool? _isValidDeviceOwner;

    public DateTime TransferDate { get; set; }

    public ExistingAssetDto Asset { get; set; }
    public ExistingDeviceDto Device { get; set; }
    public ExistingOwnerDto Owner { get; set; }

    public AssetDto IBAsset { get; set; }
    public DeviceDto IBDevice { get; set; }

    /// <summary>
    /// DO NOT USE THIS TO MAKE DECISIONS ON HOW TO 
    /// VALIDATE OR PROCESS THE MESSAGE. BSS/ORACLE
    /// HAS MANY, MANY BUGS AND CANNOT BE TRUSTED.
    /// 
    /// ImpliedAction should only be used to verify that
    /// VL's infered action via it's state implemented 
    /// in IsDeviceReplacement() and IsDeviceTransfer()
    /// matches the IB message implied action.
    /// 
    /// It gives us a some confidence (key word: some)
    /// that we are doing the right thing.
    /// </summary>
    public BssImpliedAction ImpliedAction { get; set; }

    public AssetDeviceContext()
    {
      Asset = new ExistingAssetDto();
      Device = new ExistingDeviceDto();
      Owner = new ExistingOwnerDto();

      IBAsset = new AssetDto();
      IBDevice = new DeviceDto();

      // Used to help validate DeviceTransfer or DeviceReplacement
      ImpliedAction = BssImpliedAction.NotApplicable;
    }

    /// <summary>
    /// IsDeviceBeingInstalled let's now that VL's 
    /// state is dictating that Asset/Device 
    /// relationships are changing.
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsDeviceBeingInstalled()
    {
      if(_isDeviceBeingInstalled == null)
      {
        _isDeviceBeingInstalled =
          // Creating Device then installing on Asset
          (!Device.Exists) || 
          // Orphaned Device being installed on Asset
          (!Device.AssetExists) || 
          // Device is installed on an Asset and we're
          // creating new Asset then installing Device on it
          (!Asset.Exists) || 
          // Device and Asset exist, but 
          // Device is currently installed on different Asset
          (Device.AssetId != Asset.AssetId); 
      }
      return _isDeviceBeingInstalled.Value;
    }

    /// <summary>
    /// A Device Replacement occurs when the IB Device
    /// is being moved onto an Asset that currently has
    /// a different active Device installed.
    /// 
    /// It is invalid for the IB Device to be active,
    /// but that does not define a Device Replacement
    /// so we will catch that invalid scenario in the
    /// validations and not in the IsDeviceReplacement
    /// method implementation.
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsDeviceReplacement()
    {
      if(_isDeviceReplacement == null)
      {
        _isDeviceReplacement =
          // Device is being installed an a differnt Asset
          IsDeviceBeingInstalled() &&
          // Asset must exist
          Asset.Exists && 
          // Asset must have a Device installed
          Asset.DeviceExists && 
          // Asset Installed Device must be Active
          Asset.Device.DeviceState == DeviceStateEnum.Subscribed;
      }
      return _isDeviceReplacement.Value;
    }

    /// <summary>
    /// A Device Transfer occurs when we are moving an
    /// EXISTING Device to a different Asset and it is
    /// NOT a Device Replacement.
    /// 
    /// It will be invalid to execute a Device Transfer
    /// when either the IB Device is active or the Asset's
    /// currently installed Device (if it exists) is
    /// active, but that does not define a Device Transfer
    /// so we will catch that invalid scenario in the
    /// validations and not in the IsDeviceTransfer
    /// method implementation.
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsDeviceTransfer()
    {
      if(_isDeviceTransfer == null)
      {
        //_isDeviceTransfer = Device.Exists && (!Device.AssetExists || Device.AssetId != Asset.AssetId) &&
        //                    !IsDeviceReplacement();
        _isDeviceTransfer =
          // Device must exist
          Device.Exists &&
          // Device is being installed on different Asset
          IsDeviceBeingInstalled() &&
          // Is not a Device Replacement
          !IsDeviceReplacement();
      }
      return _isDeviceTransfer.Value;
    }

    /// <summary>
    /// When the new Owner is different from existing Device's Owner 
    /// it is considered an "Ownership Transfer"
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsOwnershipTransfer()
    {
      if(_isOwnershipTransfer == null)
      {
        _isOwnershipTransfer =
          // Device exists
          Device.Exists &&
          // Existing Device is Owned by a different Owner
          Owner.Id != Device.OwnerId;
      }
      return _isOwnershipTransfer.Value;
    }

    /// <summary>
    /// Device's may only be owned by Dealers or Accounts
    /// The proposed owner must be one of these types
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsValidDeviceOwner()
    {
      if(_isValidDeviceOwner == null)
      {
        _isValidDeviceOwner = Owner.Type == CustomerTypeEnum.Dealer || Owner.Type == CustomerTypeEnum.Account;
      }
      return _isValidDeviceOwner.Value;
    }
  }

  public enum BssImpliedAction
  {
    NotApplicable,
    DeviceReplacement,
    DeviceTransfer,
  }

  public class DeviceDto
  {
    public string IbKey { get; set; }
    public string GpsDeviceId { get; set; }
    public DeviceTypeEnum? Type { get; set; }
    public string PartNumber { get; set; }
    public string FirmwareVersionId { get; set; }
    public string SIMSerialNumber { get; set; }
    public string CellularModemIMEA { get; set; }
    public string OwnerBssId { get; set; }
    public DeviceStateEnum DeviceState { get; set; }

    public void MapDeviceDto(DeviceDto deviceDto)
    {
      IbKey = deviceDto.IbKey;
      GpsDeviceId = deviceDto.GpsDeviceId;
      Type = deviceDto.Type;
      PartNumber = deviceDto.PartNumber;
      FirmwareVersionId = deviceDto.FirmwareVersionId;
      SIMSerialNumber = deviceDto.SIMSerialNumber;
      CellularModemIMEA = deviceDto.CellularModemIMEA;
      OwnerBssId = deviceDto.OwnerBssId;
      DeviceState = deviceDto.DeviceState;
    }

    public void MapDevice(Device device)
    {
      IbKey = device.IBKey;
      GpsDeviceId = device.GpsDeviceID;
      Type = (DeviceTypeEnum)device.fk_DeviceTypeID;
      OwnerBssId = device.OwnerBSSID;
      DeviceState = (DeviceStateEnum)device.fk_DeviceStateID;
    }
  }

  public class AssetDto
  {
    public string Name { get; set; }

    private string _serialNumber;
    public string SerialNumber
    {
      get { return _serialNumber; }
      set { _serialNumber = (value == null) ? null : value.Trim(); }
    }

    public string MakeCode { get; set; }
    public string Model { get; set; }
    public int? ManufactureYear { get; set; }
    //public string ProductFamily { get; set; }
    public DateTime? InsertUtc { get; set; }
    public string AssetVinSN { get; set; }
    public long StoreID { get; set; }
    public Guid? AssetUID { get; set; }

    public void MapAssetDto(AssetDto assetDto)
    {
      Name = assetDto.Name;
      SerialNumber = assetDto.SerialNumber;
      MakeCode = assetDto.MakeCode;
      Model = assetDto.Model;
      ManufactureYear = assetDto.ManufactureYear;
      //ProductFamily = assetDto.ProductFamily;
      InsertUtc = assetDto.InsertUtc;
      AssetVinSN = assetDto.AssetVinSN;
      StoreID = assetDto.StoreID;
      AssetUID = assetDto.AssetUID;
    }

    public void MapAsset(Asset asset)
    {
      Name = asset.Name;
      SerialNumber = asset.SerialNumberVIN;
      MakeCode = asset.fk_MakeCode;
      Model = asset.Model;
      ManufactureYear = asset.ManufactureYear;
      //ProductFamily = asset.ProductFamilyName;
      InsertUtc = asset.InsertUTC;
      AssetVinSN = asset.EquipmentVIN;
      StoreID = asset.fk_StoreID;
      AssetUID = asset.AssetUID;
    }
  }

  public class OwnerDto
  {
    public string BssId { get; set; }
    public string Name { get; set; }
    public CustomerTypeEnum Type { get; set; }
    public bool IsActive { get; set; }
    public long RegisteredDealerId { get; set; }
    public DealerNetworkEnum RegisteredDealerNetwork { get; set; }

    public void MapOwnerDto(OwnerDto ownerDto)
    {
      BssId = ownerDto.BssId;
      Name = ownerDto.Name;
      Type = ownerDto.Type;
      IsActive = ownerDto.IsActive;
      RegisteredDealerId = ownerDto.RegisteredDealerId;
      RegisteredDealerNetwork = ownerDto.RegisteredDealerNetwork;
    }

    public void MapOwner(Customer owner)
    {
      BssId = owner.BSSID;
      Name = owner.Name;
      Type = (CustomerTypeEnum) owner.fk_CustomerTypeID;
      IsActive = owner.IsActivated;
    }
  }

  /// <summary>
  /// Represents an Asset currently existing in VL.
  /// "Device" property is the currently installed device.
  /// "DeviceOwner" property is the current owner of the Asset's installed device.
  /// </summary>
  public class ExistingAssetDto : AssetDto
  {
    public bool Exists { get { return AssetId > 0; } }
    public long AssetId { get; set; }

    public bool DeviceExists { get { return DeviceId > 0; } }
    public long DeviceId { get; set; }
    public DeviceDto Device { get; set; }

    public bool DeviceOwnerExists { get { return DeviceOwnerId > 0; } }
    public long DeviceOwnerId { get; set; }
    public OwnerDto DeviceOwner { get; set; }

    public ExistingAssetDto()
    {
      Device = new DeviceDto();
      DeviceOwner = new OwnerDto();
    }
  }

  /// <summary>
  /// Represents a Device currently existing in VL.
  /// "Owner" property is the Device's current owner.
  /// "Asset" property is the asset that the Device is currently installed on.
  /// </summary>
  public class ExistingDeviceDto : DeviceDto
  {
    public long Id { get; set; }
    public bool Exists { get { return Id > 0; } }

    public bool AssetExists { get { return AssetId > 0; } }
    public long AssetId { get; set; }
    public AssetDto Asset { get; set; }

    public bool OwnerExists { get { return OwnerId > 0; } }
    public long OwnerId { get; set; }
    public OwnerDto Owner { get; set; }

    public ExistingDeviceDto()
    {
      Asset = new AssetDto();
      Owner = new OwnerDto();
    }
  }

  /// <summary>
  /// Represents a Customer/Organization existing in VL
  /// </summary>
  public class ExistingOwnerDto : OwnerDto
  {
    public bool Exists { get { return Id > 0; } }
    public long Id { get; set; }
  }
}