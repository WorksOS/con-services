using System;
using System.Collections.Generic;
using System.Linq;
using VSS.UnitTest.Common.Contexts;
using VSS.UnitTest.Common.EntityBuilder;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
	public class AssetBuilder
	{
		#region Asset Fields

		private string _name = "TEST_ASSET_" + IdGen.GetId();
		private string _serialNumberVin = "SERIAL_NUMBER" + IdGen.GetId();
		private string _makeCode = "CAT";
		private string _productFamilyName = "PRODUCT_FAMILY";
		private string _modelName = "MODEL_NAME";
		private int _manufactureYear = DateTime.UtcNow.Year;
		private int _iconID = 0;
		private DateTime? _insertUtc = DateTime.UtcNow;
		private bool _isMetric;
		private DateTime _updateUtc = DateTime.UtcNow;
		private double? _expectedWeeklyMileage = null;
		private string _equipmentVIN = IdGen.StringId();
		private Guid _assetUid = Guid.NewGuid();
		private int? _modelVariant = null;
		private bool _isEngineStartStopSupported = false;

		private Device _device;

		private bool _syncWithRpt;
		private long? _accountID = null;
		private long? _dealerID = null;
		private long? _owningCustomerID = null;

		private IList<ServiceTypeEnum> _services = new List<ServiceTypeEnum>();
		private bool _withDefaultAssetUtilizationSettings = false;

		#endregion

		public virtual AssetBuilder Name(string name)
		{
			_name = name;
			return this;
		}
		public virtual AssetBuilder SerialNumberVin(string serialNumberVin)
		{
			_serialNumberVin = serialNumberVin;
			return this;
		}
		public virtual AssetBuilder IconID(int iconID)
		{
			_iconID = iconID;
			return this;
		}
		public virtual AssetBuilder AssetUid(Guid uid)
		{
			_assetUid = uid;
			return this;
		}
		public virtual AssetBuilder MakeCode(string makeCode)
		{
			_makeCode = makeCode;
			return this;
		}
		public virtual AssetBuilder ProductFamily(string productFamilyName)
		{
			_productFamilyName = productFamilyName;
			return this;
		}
		public virtual AssetBuilder ModelName(string modelName)
		{
			_modelName = modelName;
			return this;
		}
		public virtual AssetBuilder ManufactureYear(int year)
		{
			_manufactureYear = year;
			return this;
		}
		public virtual AssetBuilder InsertUtc(DateTime? insertUtc)
		{
			_insertUtc = insertUtc;
			return this;
		}
		public virtual AssetBuilder IsMetric(bool isMetric)
		{
			_isMetric = isMetric;
			return this;
		}
		public virtual AssetBuilder UpdateUtc(DateTime updateUtc)
		{
			_updateUtc = updateUtc;
			return this;
		}
		public virtual AssetBuilder IsEngineStartStopSupported(bool isEngineStartStopSupported)
		{
			_isEngineStartStopSupported = isEngineStartStopSupported;
			return this;
		}
		public virtual AssetBuilder ExpectedWeeklyMileage(double weeklyMileage)
		{
			_expectedWeeklyMileage = weeklyMileage;
			return this;
		}
		public virtual AssetBuilder ModelVariant(int modelVariant)
		{
			_modelVariant = modelVariant;
			return this;
		}
		public virtual AssetBuilder WithDevice(Device device)
		{
			_device = device;
			return this;
		}
		public virtual AssetBuilder WithCoreService()
		{
			if (_device.fk_DeviceTypeID == (int)DeviceTypeEnum.MANUALDEVICE)
			{
				_services.Remove(ServiceTypeEnum.Essentials);
				WithService(ServiceTypeEnum.ManualMaintenanceLog);
			}
			else
			{
				_services.Remove(ServiceTypeEnum.ManualMaintenanceLog);
				WithService(ServiceTypeEnum.Essentials);
			}
			return this;
		}
		public virtual AssetBuilder WithService(ServiceTypeEnum serviceType)
		{
			if (!_services.Contains(serviceType))
				_services.Add(serviceType);

			return this;
		}
		public virtual AssetBuilder WithDefaultAssetUtilizationSettings()
		{
			_withDefaultAssetUtilizationSettings = true;

			return this;
		}
		public virtual AssetBuilder SyncWithRpt(long? ownerCustomerID = null, long? accountID = null, long? registeredDealerID = null)
		{
			_syncWithRpt = true;
			_accountID = accountID;
			_dealerID = registeredDealerID;
			_owningCustomerID = ownerCustomerID;
			return this;
		}
		public virtual AssetBuilder EquipmentVIN(string equipmentVIN)
		{
			_equipmentVIN = equipmentVIN;
			return this;
		}
		public Asset Build()
		{
			Asset asset = new Asset();

			asset.AssetID = Asset.ComputeAssetID(_makeCode, _serialNumberVin);
			asset.Name = _name;
			asset.IconID = _iconID;
			asset.SerialNumberVIN = _serialNumberVin;
			asset.ProductFamilyName = _productFamilyName;
			asset.Model = _modelName;
			asset.ManufactureYear = _manufactureYear;
			asset.ExpectedWeeklyMileage = _expectedWeeklyMileage;
			asset.InsertUTC = _insertUtc;
			asset.UpdateUTC = _updateUtc;
			asset.EquipmentVIN = _equipmentVIN;
			asset.AssetUID = _assetUid;
			asset.fk_ModelVariant = _modelVariant;
			asset.IsEngineStartStopSupported = _isEngineStartStopSupported;

			// Relationships
			Make m = ContextContainer.Current.OpContext.MakeReadOnly.Where(exp => exp.Code == _makeCode).FirstOrDefault();
			if (null == m)
			{
				m = new Make { Code = _makeCode, Name = _makeCode.ToString(), UpdateUTC = DateTime.UtcNow };
				ContextContainer.Current.OpContext.Make.AddObject(m);
			}
			asset.fk_MakeCode = _makeCode;
			asset.Device = _device;

			return asset;
		}

		public virtual Asset Save()
		{
			Asset asset = Build();

			ContextContainer.Current.OpContext.Asset.AddObject(asset);
			ContextContainer.Current.OpContext.SaveChanges();

			if (_withDefaultAssetUtilizationSettings)
			{
				Entity.AssetExpectedRuntimeHoursProjected.ForAsset(asset).Save();
				Entity.AssetBurnRates.ForAsset(asset).Save();
				Entity.AssetWorkingDefinition.ForAsset(asset).Save();
			}

			AddServiceToAsset(asset);

			return asset;
		}

		private void AddServiceToAsset(Asset asset)
		{
			Customer deviceOwner = null;
			foreach (var serviceType in _services)
			{
				if (deviceOwner == null)
				{
					deviceOwner = GetDeviceOwner(asset);
				}

				var service = Entity.Service.ServiceType(serviceType).ForDevice(asset.Device).SyncWithRpt(_syncWithRpt)
																		.WithView(x => x.ForAsset(asset).ForCustomer(deviceOwner)).Save();
			}
		}

		private Customer GetDeviceOwner(Asset asset)
		{
			Customer deviceOwner = ContextContainer.Current.OpContext.Customer.FirstOrDefault(cust => cust.BSSID == asset.Device.OwnerBSSID);

			if (deviceOwner == null)
			{
				throw new InvalidOperationException(string.Format("Unable find customer with BSSID {0}. Please ensure the device used to create this asset has it's OwnerBSSID propery set explicitly to an existing dealer or account with parent customer.", asset.Device.OwnerBSSID));
			}

			if (deviceOwner.fk_CustomerTypeID != (int)CustomerTypeEnum.Account) return deviceOwner;

			// Get the parent customer who should have the view of the asset
			Customer parentCustomer = (from relationship in ContextContainer.Current.OpContext.CustomerRelationship
																 join customer in ContextContainer.Current.OpContext.Customer on relationship.fk_ParentCustomerID equals customer.ID
																 where relationship.fk_ClientCustomerID == deviceOwner.ID
																			 && customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer
																 select customer).First();

			if (parentCustomer == null)
			{
				throw new InvalidOperationException(string.Format("Unable find parent customer of account with BSSID {0}. Please ensure the device used to create this asset has it's OwnerBSSID propery set explicitly to an existing dealer or account with parent customer.", asset.Device.OwnerBSSID));
			}

			return parentCustomer;
		}
	}
}
