using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Reflection;
using log4net;
using Spring.Aop.Framework;
using VSS.Hosted.VLCommon.Services.MDM;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Hosted.VLCommon
{
	public enum GeocodingProvider
	{
		ALK = 0,
		NAVINFO = 1,
		MAPABC = 2,
		MAPBAR = 3
	}

	public static class API
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static ISessionAPI Session
		{
			get
			{
				if (session == null)
				{
					session = (ISessionAPI)GetProxy<SessionAPI, ISessionAPI>();
				}
				return session;
			}
		}

		public static ICustomerAPI Customer
		{
			get
			{
				if (customer == null)
				{
					customer = (ICustomerAPI)GetProxy<CustomerAPI, ICustomerAPI>();
				}
				return customer;
			}
		}
		
		public static ISiteAPI Site
		{
			get
			{
				if (site == null)
				{
					site = (ISiteAPI)GetProxy<SiteAPI, ISiteAPI>();
				}
				return site;
			}
		}

		public static IUserAPI User
		{
			get
			{
				if (user == null)
				{
					user = (IUserAPI)GetProxy<UserAPI, IUserAPI>();
				}
				return user;
			}
		}

		public static IDeviceAPI Device
		{
			get
			{
				if (device == null)
				{
					device = (IDeviceAPI)GetProxy<DeviceAPI, IDeviceAPI>();
				}
				return device;
			}
		}

		public static IEquipmentAPI Equipment
		{
			get
			{
				if (equipment == null)
				{
					equipment = (IEquipmentAPI)GetProxy<EquipmentAPI, IEquipmentAPI>();
				}
				return equipment;
			}
		}

		public static IMTSOutboundAPI MTSOutbound
		{
			get
			{
				if (MTSOutAPI == null)
				{
					MTSOutAPI = (IMTSOutboundAPI)GetProxy<MTSOutboundAPI, IMTSOutboundAPI>();
				}
				return MTSOutAPI;
			}
		}

		public static ITTOutboundAPI TTOutbound
		{
			get
			{
				if (TTOutAPI == null)
				{
					TTOutAPI = (ITTOutboundAPI)GetProxy<TTOutboundAPI, ITTOutboundAPI>();
				}
				return TTOutAPI;
			}
		}

		
		public static IPLOutboundAPI PLOutbound
		{
			get
			{
				if (plOutbound == null)
				{
					plOutbound = (IPLOutboundAPI)GetProxy<PLOutboundAPI, IPLOutboundAPI>();
				}

				return plOutbound;
			}
		}

		public static IFirmwareAPI Firmware
		{
			get
			{
				if (firmware == null)
				{
					firmware = (IFirmwareAPI)GetProxy<FirmwareAPI, IFirmwareAPI>();
				}

				return firmware;
			}
		}

		public static IMaintenanceAPI Maintenance
		{
			get
			{
				if (maintenance == null)
				{
					maintenance = (IMaintenanceAPI)GetProxy<MaintenanceAPI, IMaintenanceAPI>();
				}
				return maintenance;
			}
		}

		public static IAssetFeatureAPI AssetFeature
		{
			get
			{
				if (assetFeature == null)
				{
					assetFeature = (IAssetFeatureAPI)GetProxy<AssetFeatureAPI, IAssetFeatureAPI>();
				}
				return assetFeature;
			}
		}

		public static IServiceViewAPI ServiceView
		{
			get
			{
				if (serviceView == null)
				{
					serviceView = (IServiceViewAPI)GetProxy<ServiceViewAPI, IServiceViewAPI>();
				}
				return serviceView;
			}
		}


		public static IBssProvisioningMsgAPI BssProvisioningMsg
		{
			get
			{
				if (bssProvisioningMsg == null)
				{
					bssProvisioningMsg = (IBssProvisioningMsgAPI)GetProxy<BssProvisioningMsgAPI, IBssProvisioningMsgAPI>();
				}
				return bssProvisioningMsg;
			}
		}

		public static IBssResponseMsgAPI BssResponseMsg
		{
			get
			{
				if (bssResponseMsg == null)
				{
					bssResponseMsg = (IBssResponseMsgAPI)GetProxy<BssResponseMsgAPI, IBssResponseMsgAPI>();
				}
				return bssResponseMsg;
			}
		}
		
		public static IEmailAPI Email
		{
			get
			{
				if (email == null)
				{
					email = (IEmailAPI)GetProxy<EmailAPI, IEmailAPI>();
				}
				return email;
			}
		}

		public static IPMDuePopulatorAPI PMDuePopulator
		{
			get
			{
				if (pmPopulator == null)
				{
					pmPopulator = (IPMDuePopulatorAPI)GetProxy<PMDuePopulatorAPI, IPMDuePopulatorAPI>();
				}
				return pmPopulator;
			}
		}
		public static IAssetService AssetService
		{
			get
			{
				if (assetService == null)
				{
					assetService = GetProxy<AssetService, IAssetService>();
				}
				return assetService;
			}
		}

		public static IDeviceService DeviceService
		{
			get
			{
				if (deviceService == null)
				{
					deviceService = GetProxy<DeviceService, IDeviceService>();
				}
				return deviceService;
			}
		}

		public static IGeofenceService GeofenceService
		{
			get
			{
				if (geofenceService == null)
				{
					geofenceService = GetProxy<GeofenceService, IGeofenceService>();
				}
				return geofenceService;
			}
		}

		public static ICustomerService CustomerService
		{
			get
			{
				if (customerService == null)
				{
					customerService = GetProxy<CustomerService, ICustomerService>();
				}
				return customerService;
			}
		}


		public static IWorkDefinitionService WorkDefinitionService
		{
			get
			{
				if (workDefinitionService == null)
				{
					workDefinitionService = GetProxy<WorkDefinitionService, IWorkDefinitionService>();
				}
				return workDefinitionService;
			}
		}

		public static ISubscriptionService SubscriptionService
		{
			get
			{
				if (subscriptionService == null)
				{
					subscriptionService = GetProxy<SubscriptionService, ISubscriptionService>();
				}
				return subscriptionService;
			}
		}

		#region Templated Methods

		public static T Update<T>(INH_OP dataContext, T updatedT, List<Param> fieldsOfT)
		{
			if (updatedT != null)
			{
				DateTime utcNow = DateTime.UtcNow;
				fieldsOfT.Add(new Param() { Name = "UpdateUTC", Value = utcNow });

				// Apply params
				foreach (Param p in fieldsOfT)
				{
					PropertyInfo property = updatedT.GetType().GetProperty(p.Name);
					if (null != property)
					{
						if (p.Name.EndsWith("Reference") && p.Value is EntityKey)
						{
							object reference = property.GetValue(updatedT, null);
							PropertyInfo entProperty = reference.GetType().GetProperty("EntityKey");
							if (entProperty != null)
							{
								entProperty.SetValue(reference, p.Value, null);
							}
						}
						else
						{
							property.SetValue(updatedT, p.Value, null);
						}
					}
				}

				int result = dataContext.SaveChanges();

				if (result <= 0)
					throw new InvalidOperationException("Failed to save changes");
			}
			return updatedT;
		}

		#endregion

		private static K GetProxy<T, K>()
		{
			T obj = Activator.CreateInstance<T>();
			if (obj != null)
			{
				ProxyFactory factory = new ProxyFactory(obj);

				// add spring aspect advice here...

				return (K)factory.GetProxy();
			}
			return default(K);
		}

		private static IUserAPI user = null;
		private static ISiteAPI site = null;
		private static ICustomerAPI customer = null;
		private static ISessionAPI session = null;
		private static IDeviceAPI device = null;
		private static IEquipmentAPI equipment = null;
		private static IMTSOutboundAPI MTSOutAPI = null;
		private static ITTOutboundAPI TTOutAPI = null;
		private static IPLOutboundAPI plOutbound = null;
		private static IFirmwareAPI firmware = null;
		private static IMaintenanceAPI maintenance = null;
		private static IAssetFeatureAPI assetFeature = null;
		private static IServiceViewAPI serviceView = null;
		private static IBssProvisioningMsgAPI bssProvisioningMsg = null;
		private static IBssResponseMsgAPI bssResponseMsg = null;
		private static IEmailAPI email = null;
		private static IPMDuePopulatorAPI pmPopulator = null;
		private static IAssetService assetService = null;
		private static IDeviceService deviceService = null;
		private static IGeofenceService geofenceService = null;
		private static ICustomerService customerService = null;
		private static IWorkDefinitionService workDefinitionService = null;
		private static ISubscriptionService subscriptionService = null;

	}
}
