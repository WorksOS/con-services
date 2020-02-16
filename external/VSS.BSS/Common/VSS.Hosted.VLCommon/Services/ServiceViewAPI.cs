using System.Web.UI.WebControls;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using VSS.Hosted.VLCommon.Services.MDM;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon
{
    public class ServiceViewAPI : IServiceViewAPI
    {
        const int NullEndDate = 99991231;
        private static int? _serviceViewBlockSize;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool EnableCustomerNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.CustomerAPI.EnableSync"]);
        private static readonly bool EnableSubscriptionNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.SubscriptionAPI.EnableSync"]);
        private static readonly bool EnableDeviceNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.DeviceAPI.EnableSync"]);
        private static List<Int64> _assetSubscriptionList = new List<long> { 
            (Int64)ServiceTypeEnum.Essentials, 
            (Int64)ServiceTypeEnum.ManualMaintenanceLog, 
            (Int64)ServiceTypeEnum.CATHealth,
            (Int64)ServiceTypeEnum.StandardHealth,
            (Int64)ServiceTypeEnum.CATUtilization,
            (Int64)ServiceTypeEnum.StandardUtilization,
            (Int64)ServiceTypeEnum.CATMAINT,
            (Int64)ServiceTypeEnum.VLMAINT,
            (Int64)ServiceTypeEnum.RealTimeDigitalSwitchAlerts,
            (Int64)ServiceTypeEnum.e1minuteUpdateRateUpgrade,
            (Int64)ServiceTypeEnum.ConnectedSiteGateway,
            (Int64)ServiceTypeEnum.e2DProjectMonitoring,
            (Int64)ServiceTypeEnum.e3DProjectMonitoring,
            (Int64)ServiceTypeEnum.VisionLinkRFID,
            (Int64)ServiceTypeEnum.VehicleConnect,
            (Int64)ServiceTypeEnum.UnifiedFleet,
            (Int64)ServiceTypeEnum.AdvancedProductivity,
            (Int64)ServiceTypeEnum.CATHarvest
        };

        private static List<Int64> _projectSubscriptionList = new List<long> { 
            (Int64)ServiceTypeEnum.Landfill, 
            (Int64)ServiceTypeEnum.ProjectMonitoring, 
        };

        private readonly ICustomerService _customerServiceApi;
        public ServiceViewAPI()
        {
            _customerServiceApi = API.CustomerService;
        }

        public ServiceViewAPI(ICustomerService customerService)
            : this()
        {
            _customerServiceApi = customerService;
        }

        private class AssetIDInfo
        {
            public long AssetID { get; set; }
            public Guid AssetGuid { get; set; }
        }

        public class ServiceViewWithCore
        {
            public ServiceView ServiceView { get; set; }
            public bool IsCore { get; set; }
        }

        private bool TerminateServiceViews(INH_OP opContext, List<ServiceViewWithCore> viewsToTerminateWithCore, DateTime terminationDate )
        {
            if (viewsToTerminateWithCore.Count == 0)
                return false;

            int terminationKeyDate = terminationDate.KeyDate();
            bool terminateInThePast = terminationDate <= DateTime.UtcNow;

            var viewsToTerminate = viewsToTerminateWithCore.Select(x => x.ServiceView).ToList();

            foreach (var sView in viewsToTerminate)
            {
                sView.EndKeyDate = terminationKeyDate;
                sView.UpdateUTC = DateTime.UtcNow;
            }

            var success = opContext.SaveChanges() > 0;

            if (success)
            {
                if (EnableSubscriptionNextGenSync)
                {
                    foreach (var sv in viewsToTerminate)
                    {
                        UpdateSubscription(opContext, sv, isEndDate: true);
                    }
                }

                if (EnableCustomerNextGenSync)
                {
                    SyncDissociateCustomerAssetWithNextGen(terminationDate, viewsToTerminateWithCore, opContext);
                }
            }
            return success;
        }

        private bool TerminateServiceViewsForService(INH_OP ctx, long serviceID, DateTime terminationDate)
        {
            var keyDate = terminationDate.KeyDate();
            var views = (from s in ctx.Service
                         join sv in ctx.ServiceView on s.ID equals sv.fk_ServiceID
                         join st in ctx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                         where sv.fk_ServiceID == serviceID
          && sv.EndKeyDate > keyDate
                         select new ServiceViewWithCore {ServiceView = sv, IsCore = st.IsCore}).ToList();
            return TerminateServiceViews(ctx, views, terminationDate);
        }

        public Service CreateService(INH_OP opContext, long deviceID, string bssPlanLineID, DateTime activationDate, ServiceTypeEnum serviceType)
        {
            var isCore = ServicePlanIsCore(opContext, serviceType);
            Log.IfDebugFormat("line 92 isCoreService {0}", isCore);

            var service = new Service
            {
                BSSLineID = bssPlanLineID,
                ActivationKeyDate = activationDate.KeyDate(),
                OwnerVisibilityKeyDate = activationDate.AddMonths(-13).KeyDate(),
                CancellationKeyDate = NullEndDate,
                fk_DeviceID = deviceID,
                fk_ServiceTypeID = (int)serviceType,
                // only set first report required for BSS core service plan
                IsFirstReportNeeded = isCore && !bssPlanLineID.StartsWith("-"),
                ServiceUID = Guid.NewGuid()
            };
            Log.IfDebugFormat("line 103 serviceRecord isFirstReportNeeded{0}", service.IsFirstReportNeeded);
            opContext.Service.AddObject(service);
            opContext.SaveChanges();

            CreateServiceViewsForService(opContext, service);

            return service;
        }

        private void CreateServiceViewsForService(INH_OP ctx, Service service, int serviceViewStartKeyDate = 0)
        {
            var serviceID = service.ID;
            var assetID = (from a in ctx.AssetReadOnly
                           where a.fk_DeviceID == service.fk_DeviceID
                           select new AssetIDInfo { AssetID = a.AssetID, AssetGuid = a.AssetUID.Value }).Single();
            var ownerCustomer = (from c in ctx.CustomerReadOnly
                                 join d in ctx.DeviceReadOnly on c.BSSID equals d.OwnerBSSID
                                 where d.ID == service.fk_DeviceID
                                 select c).Single();
            Customer dealerCustomer;

            var createdServiceViews = new List<ServiceView>();
            try
            {
                if (ownerCustomer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account)
                {
                    // Setup the service views all the way up the customer hierarchy, starting directly above the account
                    var parentCustomer = GetParent(ownerCustomer.ID, CustomerRelationshipTypeEnum.TCSCustomer);
                    while (parentCustomer != null)
                    {
                        if (service.OwnerVisibilityKeyDate.HasValue)
                        {
                            var customerViewStartKeyDate = serviceViewStartKeyDate >= 20090101 ? serviceViewStartKeyDate : service.OwnerVisibilityKeyDate.Value;
                            createdServiceViews.Add(CreateServiceView(ctx, parentCustomer.ID, assetID.AssetID, assetID.AssetGuid, serviceID, NullEndDate, customerViewStartKeyDate, RelationType.Customer));
                        }
                        parentCustomer = GetParent(parentCustomer.ID, CustomerRelationshipTypeEnum.TCSCustomer);
                    }

                    dealerCustomer = GetParent(ownerCustomer.ID, CustomerRelationshipTypeEnum.TCSDealer);
                }
                else if (ownerCustomer.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer)
                {
                    dealerCustomer = ownerCustomer;
                }
                else
                {
                    // Can only handle adding service views if the root is an account or a dealer.
                    // Basically, standalone customers are simply not supported.
                    return;
                }

                if (serviceViewStartKeyDate < 20090101)
                    serviceViewStartKeyDate = service.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate();

                // Set up the service views required along the dealer path
                // Service view for the dealer

                if (dealerCustomer != null)
                {
                    //IsOwner calculation: dealerCustomer == ownerCustomer ? RelationType.Owner
                    createdServiceViews.Add(CreateServiceView(ctx, dealerCustomer.ID, assetID.AssetID, assetID.AssetGuid, serviceID, NullEndDate, serviceViewStartKeyDate,
                        RelationType.Dealer));
                    var sv = CreateServiceViewForCorporate(ctx, dealerCustomer.fk_DealerNetworkID, assetID, serviceID, serviceViewStartKeyDate);
                    if (sv != null) createdServiceViews.Add(sv);

                    // Service view for grandparent
                    dealerCustomer = GetParent(dealerCustomer.ID, CustomerRelationshipTypeEnum.TCSDealer);

                    if (dealerCustomer != null)
                    {
                        createdServiceViews.Add(CreateServiceView(ctx, dealerCustomer.ID, assetID.AssetID, assetID.AssetGuid, serviceID, NullEndDate, serviceViewStartKeyDate, RelationType.Dealer));
                        sv = CreateServiceViewForCorporate(ctx, dealerCustomer.fk_DealerNetworkID, assetID, serviceID, serviceViewStartKeyDate);
                        if (sv != null) createdServiceViews.Add(sv);
                    }
                }

                //not syncing with next-gen here as CreateServiceView() takes care of it
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error caught creating Service Views: {0}", ex);
            }
        }

        private ServiceView CreateServiceViewForCorporate(INH_OP ctx, int dealerNetworkID, AssetIDInfo assetID, long serviceID, int serviceViewStartKeyDate)
        {
            // Create Corporate service view if appropriate for this dealer - note that it can be different if multiple dealer networks are in the hierarchy
            var corpCustID = (from c in ctx.CustomerReadOnly
                              where c.fk_DealerNetworkID == dealerNetworkID
                                      && c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate
                              select c.ID).SingleOrDefault();
            if (corpCustID == 0)
                return null;

            return CreateServiceView(ctx, corpCustID, assetID.AssetID, assetID.AssetGuid, serviceID, NullEndDate, serviceViewStartKeyDate, RelationType.Corporate);
        }

        private Customer GetParent(long childCustomerID, CustomerRelationshipTypeEnum type)
        {
            using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>(true))
            {
                return (from pr in ctx.CustomerRelationshipReadOnly
                        join p in ctx.CustomerReadOnly on pr.fk_ParentCustomerID equals p.ID
                        where pr.fk_ClientCustomerID == childCustomerID
                            && pr.fk_CustomerRelationshipTypeID == (int)type
                        select p).SingleOrDefault();
            }
        }

        public bool TerminateService(INH_OP opContext, string bssPlanLineID, DateTime terminationUTC)
        {
            var svc = (from srv in opContext.Service
                       where srv.BSSLineID == bssPlanLineID
                       select srv).SingleOrDefault<Service>();

            if (svc == null)
            {
                return false;
            }

            // Get rid of all of the service views for the service before cancelling the service
            if (!TerminateServiceViewsForService(opContext, svc.ID, terminationUTC))
            {
                return false;
            }

            // Actually cancel the service by setting the termination date
            svc.CancellationKeyDate = terminationUTC.KeyDate();
            svc.IsFirstReportNeeded = false;

            return opContext.SaveChanges() > 0;
        }

        private bool ServicePlanIsCore(INH_OP ctx, ServiceTypeEnum serviceType)
        {
            return (from s in ctx.ServiceTypeReadOnly where s.ID == (int)serviceType select s.IsCore).SingleOrDefault();
        }

        public bool BssPlanLineIDExists(INH_OP opContext, string bssPlanLineID)
        {
            var serviceRecord = (from s in opContext.ServiceReadOnly
                                 where s.BSSLineID == bssPlanLineID
                                 select s).SingleOrDefault();

            return serviceRecord != null;
        }

        public bool DeviceSupportsService(INH_OP opContext, ServiceTypeEnum serviceType, DeviceTypeEnum deviceType)
        {
            return (from dtst in opContext.DeviceTypeServiceTypeReadOnly
                    where dtst.fk_ServiceTypeID == (int)serviceType
                    && dtst.fk_DeviceTypeID == (int)deviceType
                    select 1).Any();
        }

        public bool DeviceHasAnActiveService(INH_OP opContext, long deviceID)
        {
            var keyDateNow = DateTime.UtcNow.KeyDate();
            return (from s in opContext.ServiceReadOnly
                    where s.fk_DeviceID == deviceID
                    && s.ActivationKeyDate <= keyDateNow
                    && s.CancellationKeyDate >= keyDateNow
                    select s).Any();
        }

        public bool DeviceHasActiveCoreService(INH_OP opContext, long deviceID)
        {
            var keyDateNow = DateTime.UtcNow.KeyDate();
            return (from s in opContext.ServiceReadOnly
                    join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                    where s.fk_DeviceID == deviceID
                    && s.ActivationKeyDate <= keyDateNow
                    && s.CancellationKeyDate >= keyDateNow
                    && st.IsCore
                    select s).Any();
        }

        /// <summary>
        /// Checks to see if the device has any core services owned by non-Corporate customers
        /// This has been put in to allow for De-reg of devices on cancellation of subscription
        /// </summary>  
        public bool DeviceHasActiveBSSService(INH_OP opContext, long deviceID)
        {
            var keyDateNow = DateTime.UtcNow.KeyDate();

            return (from a in opContext.AssetReadOnly
                    from sv in opContext.ServiceViewReadOnly
                    join s in opContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                    join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                    join c in opContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                    where a.fk_DeviceID == deviceID
                        && sv.fk_AssetID == a.AssetID
                        && sv.StartKeyDate <= keyDateNow
                        && sv.EndKeyDate >= keyDateNow
                        && st.IsCore
                        && c.fk_CustomerTypeID != 4   // Exclude corporate customers
                    select s).Any();
        }

        public ServiceView CreateServiceView(INH_OP opContext, long customerID, long assetID, Guid assetGuid, long serviceID, int startKeyDate, RelationType relationType)
        {
            return CreateServiceView(opContext, customerID, assetID, assetGuid, serviceID, NullEndDate, startKeyDate, relationType);
        }

        /*private ServiceView CreateServiceView(INH_OP opContext, long customerID, long assetID, long serviceID, int endKeyDate, int startKeyDate, List<ServiceView> serviceViewsOnParent = null)
        {
            ServiceView sv = (from vw in opContext.ServiceView
                                                where vw.fk_AssetID == assetID &&
                                                            vw.fk_CustomerID == customerID &&
                                                            vw.fk_ServiceID == serviceID
                                                select vw).FirstOrDefault();

            // Service view already exists.  Update if required, otherwise just return it
            if (sv != null)
            {
                if (sv.EndKeyDate != endKeyDate)
                {
                    sv.EndKeyDate = endKeyDate;
                    sv.UpdateUTC = DateTime.UtcNow;
                    opContext.SaveChanges();
                }
                return sv;
            }

            ServiceView view = new ServiceView();
            view.EndKeyDate = endKeyDate;
            view.fk_AssetID = assetID;
            view.fk_CustomerID = customerID;
            view.fk_ServiceID = serviceID;
            view.StartKeyDate = startKeyDate;
            view.UpdateUTC = DateTime.UtcNow;

            //add the new serviceview to this list here because we will need to look at it again later
            if (serviceViewsOnParent != null)
                serviceViewsOnParent.Add(view);

            opContext.ServiceView.AddObject(view);
            opContext.SaveChanges();

            return view;
        }*/

        #region ServiceView management for BSS V2

        #region Service Plan Activated action supporting methods

        /// <summary>
        /// Create Service and Service Views for Dealer and Customer and Corporate Customer
        /// It will also create Service Views in the Dealer and Customer Hierarchy
        /// </summary>
        /// <param name="opContext">NH_OP edm context</param>
        /// <param name="deviceID">Device ID for which the service is being created</param>
        /// <param name="deviceType">Type of the Device for which the service is being created</param>
        /// <param name="bssPlanLineID">This will uniquely identifies the service</param>
        /// <param name="activationDate">The Service activation date</param>
        /// <param name="ownerVisibilityDate">The date which decides the customer visibility to asset/device status</param>
        /// <param name="serviceType">Type of service which is being created</param>
        /// <returns>A Tuple with the service and its corresponding service views created</returns>
        public Tuple<Service, IList<ServiceView>> CreateServiceAndServiceViews(INH_OP opContext, long deviceID, DeviceTypeEnum deviceType,
                    string bssPlanLineID, DateTime activationDate, DateTime? ownerVisibilityDate, ServiceTypeEnum serviceType)
        {
            var result = new Tuple<Service, IList<ServiceView>>(null, null);

            //if the device doesn't support the service don't create the service
            if (!DeviceSupportsService(opContext, serviceType, deviceType))
                return result;

            var isCore = ServicePlanIsCore(opContext, serviceType);

            var service = new Service
            {
                BSSLineID = bssPlanLineID,
                ActivationKeyDate = activationDate.KeyDate(),
                OwnerVisibilityKeyDate = ownerVisibilityDate.HasValue ? ownerVisibilityDate.KeyDate() : (int?)null,
                CancellationKeyDate = DotNetExtensions.NullKeyDate,
                fk_DeviceID = deviceID,
                fk_ServiceTypeID = (int)serviceType,
                // only set first report required for BSS core service plan
                IsFirstReportNeeded = isCore && !bssPlanLineID.StartsWith("-"),
                ServiceUID = Guid.NewGuid()
            };

            opContext.Service.AddObject(service);
            opContext.SaveChanges();

            var serviceViews = CreateServiceViewsForServiceV2(opContext, service);

            result = new Tuple<Service, IList<ServiceView>>(service, serviceViews);

            return result;
        }

        private List<ServiceView> CreateServiceViewsForServiceV2(INH_OP ctx, Service service)
        {
            var createdServiceViews = new List<ServiceView>();

            var assetIDInfo = (from a in ctx.AssetReadOnly
                               where a.fk_DeviceID == service.fk_DeviceID
                               select new AssetIDInfo { AssetID = a.AssetID, AssetGuid = a.AssetUID.Value }
                                         ).Single();

            var ownerCustomer = (from c in ctx.CustomerReadOnly
                                 join d in ctx.DeviceReadOnly on c.BSSID equals d.OwnerBSSID
                                 where d.ID == service.fk_DeviceID
                                 select c).Single();
            try
            {
                if (ownerCustomer.fk_CustomerTypeID == (int)CustomerTypeEnum.Account)
                {
                    //create service views for the dealer hierarchy and corp service views for the corresponding dealer network
                    CreateServiceViewsForDealer(ctx, service, assetIDInfo, ownerCustomer, createdServiceViews);

                    //create service views for the customer hierarchy provided owner visibility key date is not null
                    CreateServiceViewsForCustomer(ctx, service, assetIDInfo, ownerCustomer, createdServiceViews);
                }
                else if (ownerCustomer.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer)
                    CreateServiceViewsForDealer(ctx, service, assetIDInfo, ownerCustomer, createdServiceViews);
                else
                    // Can only handle adding service views if the root is an account or a dealer.
                    // Basically, standalone customers are simply not supported.
                    return createdServiceViews;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error caught creating Service Views: {0}", ex);
                throw;
            }
            return createdServiceViews;
        }

        private void CreateServiceViewsForDealer(INH_OP ctx, Service service, AssetIDInfo assetIDInfo, Customer account, List<ServiceView> createdServiceViews)
        {
            var deviceID = ctx.AssetReadOnly.Where(a => a.AssetID == assetIDInfo.AssetID).Select(a => a.fk_DeviceID).SingleOrDefault();
            var startKeyDate = IsOnboardingAsset(ctx, deviceID, assetIDInfo.AssetID)
                                                     ? service.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate()
                                                     : service.ActivationKeyDate;

            if (account.fk_CustomerTypeID == (int)CustomerTypeEnum.Account)
                account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSDealer);
            while (account != null)
            {
                createdServiceViews.Add(CreateServiceView(ctx, account.ID, assetIDInfo.AssetID, assetIDInfo.AssetGuid, service.ID, service.CancellationKeyDate, startKeyDate, RelationType.Dealer));
                var sv = CreateServiceViewForCorporate(ctx, account.fk_DealerNetworkID, assetIDInfo, service.ID, startKeyDate);
                if (sv != null) createdServiceViews.Add(sv);
                account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSDealer);
            }
        }

        private void CreateServiceViewsForCustomer(INH_OP ctx, Service service, AssetIDInfo assetIDInfo, Customer account, List<ServiceView> createdServiceViews)
        {
            if (!service.OwnerVisibilityKeyDate.HasValue)
                return;
            account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSCustomer);
            while (account != null)
            {
                createdServiceViews.Add(CreateServiceView(ctx, account.ID, assetIDInfo.AssetID, assetIDInfo.AssetGuid, service.ID, service.CancellationKeyDate, service.OwnerVisibilityKeyDate.Value, RelationType.Customer));

                account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSCustomer);
            }
        }

        public ServiceView CreateServiceView(INH_OP opContext, long customerID, long assetID, Guid assetGuid, long serviceID, int endKeyDate, int startKeyDate, RelationType relationType)
        {
            var svCore = (from sv1 in opContext.ServiceView
                          join service in opContext.ServiceReadOnly on sv1.fk_ServiceID equals service.ID
                          join st in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                          where sv1.fk_AssetID == assetID &&
                                      sv1.fk_CustomerID == customerID &&
                                      sv1.fk_ServiceID == serviceID
                          select new ServiceViewWithCore { IsCore = st.IsCore, ServiceView = sv1}).FirstOrDefault();
            var sv = svCore == null ? null : svCore.ServiceView;
            // Service view already exists.  Unterminate if required, otherwise just return it
            if (sv != null)
            {
                bool endKeyChanged=false, startKeyChanged=false;
                if ((endKeyChanged = sv.EndKeyDate != endKeyDate) | (startKeyChanged = sv.StartKeyDate != startKeyDate))
                {
                    sv.EndKeyDate = endKeyDate;
                    sv.StartKeyDate = startKeyDate;
                    sv.UpdateUTC = DateTime.UtcNow;
                    var success = opContext.SaveChanges() > 0;

                    if (success && EnableSubscriptionNextGenSync)
                    {
                        UpdateSubscription(opContext, sv, isStartDate: startKeyChanged, isEndDate: endKeyChanged);
                    }

                    if (success && EnableCustomerNextGenSync && svCore.IsCore)
                    {
                        var customerUID = (from c in opContext.CustomerReadOnly
                                           where c.ID == customerID
                                           select c.CustomerUID).Single();

                        if(endKeyChanged && endKeyDate.FromKeyDate() <= DateTime.UtcNow)
                            MdmHelpers.SyncDissociateCustomerAssetWithNextGen(opContext, _customerServiceApi, endKeyDate.FromKeyDate(), assetGuid, customerUID.Value, Log);

                        if(startKeyChanged)
                            MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opContext, _customerServiceApi, customerUID.Value, assetGuid, relationType.ToString());
                    }
                }
                return sv;
            }
            var serviceType = (from s in opContext.ServiceReadOnly.Where(x => x.ID == serviceID)
                               join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                               select st).FirstOrDefault();
            bool isCore = serviceType != null && serviceType.IsCore;

            sv = new ServiceView
            {
                EndKeyDate = endKeyDate,
                fk_AssetID = assetID,
                fk_CustomerID = customerID,
                fk_ServiceID = serviceID,
                StartKeyDate = startKeyDate,
                UpdateUTC = DateTime.UtcNow,
                ServiceViewUID = Guid.NewGuid()
            };

            opContext.ServiceView.AddObject(sv);
            var createSuccess = opContext.SaveChanges() > 0;
            if (createSuccess && EnableSubscriptionNextGenSync)
            {
                CreateSubscription(opContext, sv);
            }

            if (EnableCustomerNextGenSync && isCore)
            {
                var customerUID = (from c in opContext.CustomerReadOnly
                                   where c.ID == customerID
                                   select c.CustomerUID).Single();
                MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opContext, _customerServiceApi, customerUID.Value, assetGuid, relationType.ToString());
            }

            //if (createSuccess && EnableDeviceNextGenSync && isCore)
            //{
            //    var deviceId = (from asset in opContext.Asset
            //                          join device in opContext.Device on asset.fk_DeviceID equals device.ID
            //                            where asset.AssetID == assetID
            //                          select device.ID).FirstOrDefault();

            //    API.Device.UpdateDeviceState(deviceId, DeviceStateEnum.Subscribed);
            //}

            return sv;
        }

        #endregion

        #region Service Plan Termination action supporting methods

        /// <summary>
        /// termiante service and all service views related to that service
        /// Invoked by BSS ServicePlan Cancelled action
        /// </summary>
        /// <param name="opContext">NH_OP edm context</param>
        /// <param name="bssPlanLineID">BSS Line ID of the service which needs to be terminated</param>
        /// <param name="terminationUTC">Date on which this service needs to be terminated</param>
        /// <returns></returns>
        public bool TerminateServiceAndServiceViews(INH_OP opContext, string bssPlanLineID, DateTime terminationUTC)
        {
            var keydate = terminationUTC.KeyDate();
            var service = opContext.Service.SingleOrDefault(t => t.BSSLineID == bssPlanLineID);

            //no service found for the given bss plan line id
            if (service == null)
                return false;

            //terminate the service
            //service.OwnerVisibilityKeyDate = keydate;
            service.CancellationKeyDate = keydate;
            service.IsFirstReportNeeded = false;

            bool ret =  TerminateServiceViewsForService(opContext, service.ID, terminationUTC);
            //if no service view changes have been saved in ret, just save service changes
            return ret || opContext.SaveChanges() > 0;


        }

        public bool ReleaseAsset(INH_OP opContext, long deviceId)
        {

            var keyDateNow = DateTime.UtcNow.KeyDate();
            var hasAnyActiveService = opContext.ServiceReadOnly.Any(x => x.fk_DeviceID == deviceId && x.CancellationKeyDate > keyDateNow);

            if (!hasAnyActiveService)
            {
                var assetToBeReleased = opContext.Asset.Where(t => t.fk_DeviceID == deviceId).Select(t => t).SingleOrDefault();

               if (assetToBeReleased != null)
               {
                  assetToBeReleased.fk_StoreID = 0;
                  assetToBeReleased.UpdateUTC = DateTime.UtcNow;
               }

               if (opContext.SaveChanges() > 0)
                    return true;
            }
            return false;
        }



        private void TerminateServiceViewsForCustomer(INH_OP opContext, long serviceID, int terminationKeyDate, List<ServiceViewWithCore> viewsToTerminate)
        {
            var serviceViews = (from sv in opContext.ServiceView
                                join service in opContext.ServiceReadOnly on sv.fk_ServiceID equals service.ID
                                join st in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                                join c in opContext.Customer on sv.fk_CustomerID equals c.ID
                                where sv.fk_ServiceID == serviceID
                                && sv.EndKeyDate > terminationKeyDate
                                && c.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer
                                select new ServiceViewWithCore {ServiceView = sv, IsCore = st.IsCore}).ToList();

            //no service views exist for the service. 
            //returning true will make the work flow to assume that the termnation was
            //successful.
            if (serviceViews.Count == 0)
                return;

            //termiante all the service views for the service
            foreach (var serviceView in serviceViews)
            {
                serviceView.ServiceView.EndKeyDate = terminationKeyDate;
                serviceView.ServiceView.UpdateUTC = DateTime.UtcNow;
                viewsToTerminate.Add(serviceView);
            }

            //not syncing with next-gen here as it's caller's responsibility
        }

        /// <summary>
        /// Checks to see if the device has any core services owned by non-Corporate customers
        /// This has been put in to allow for De-reg of devices on cancellation of subscription
        /// While checking for active services make sure not to incluse the service which is being terminated
        /// </summary>  
        public bool DeviceHasActiveBSSService(INH_OP opContext, long deviceID, long serviceID)
        {
            var keyDateNow = DateTime.UtcNow.KeyDate();

            return (from a in opContext.AssetReadOnly
                    from sv in opContext.ServiceViewReadOnly
                    join s in opContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                    join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                    join c in opContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                    where a.fk_DeviceID == deviceID
                        && sv.fk_AssetID == a.AssetID
                        && sv.StartKeyDate <= keyDateNow
                        && sv.EndKeyDate >= keyDateNow
                        && c.fk_CustomerTypeID != 4   // Exclude corporate customers
                        && s.ID != serviceID
                    select s).Any();
        }

        #endregion

        #region Private ServicePlan Termination Methods

        private IList<ServiceView> TerminateServiceViews(INH_OP opCtx, DateTime terminationDate, IList<ServiceViewDto> serviceViews)
        {
            var terminatedViews = new List<ServiceView>();

            var serviceIds = serviceViews.Select(x => x.ServiceId);

            var endKeyDate = DateTime.UtcNow.KeyDate();
            var viewsToTerminate =
                (from sv in opCtx.ServiceView
                 join service in opCtx.ServiceReadOnly on sv.fk_ServiceID equals service.ID
                 join st in opCtx.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                 where serviceIds.Contains(sv.fk_ServiceID)
                             && sv.EndKeyDate > endKeyDate
                 select new ServiceViewWithCore() {ServiceView = sv, IsCore = st.IsCore}).ToList();

            return TerminateServiceViews(opCtx, terminationDate, viewsToTerminate, terminatedViews);
        }

        private IList<ServiceView> TerminateServiceViewsForCustomer(INH_OP opCtx, CustomerInfoDto customer, DateTime terminationDate, IList<ServiceViewDto> serviceViews)
        {
            var terminatedViews = new List<ServiceView>();

            var serviceIds = serviceViews.Select(x => x.ServiceId);

            var endKeyDate = DateTime.UtcNow.KeyDate();
            var viewsToTerminate =
                (from sv in opCtx.ServiceView
                 join service in opCtx.ServiceReadOnly on sv.fk_ServiceID equals service.ID
                 join st in opCtx.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                 where serviceIds.Contains(sv.fk_ServiceID)
                             && sv.EndKeyDate > endKeyDate
                             && sv.fk_CustomerID == customer.ID
                 select new ServiceViewWithCore() { ServiceView = sv, IsCore = st.IsCore }).ToList();

            return TerminateServiceViews(opCtx, terminationDate, viewsToTerminate, terminatedViews);
        }

        private IList<ServiceView> TerminateServiceViews(INH_OP opCtx, DateTime terminationDate, List<ServiceViewWithCore> viewsToTerminateWithCore, List<ServiceView> terminatedViews)
        {
            var viewsToTerminate = viewsToTerminateWithCore.Select(x => x.ServiceView).ToList();
            foreach (var serviceView in viewsToTerminate)
            {
                serviceView.EndKeyDate = terminationDate.KeyDate();
                serviceView.UpdateUTC = DateTime.UtcNow;
                terminatedViews.Add(serviceView);
            }

            var success = opCtx.SaveChanges() > 0;
            if (success && EnableSubscriptionNextGenSync)
            {
                foreach (var sv in terminatedViews)
                {
                    UpdateSubscription(opCtx, sv, isEndDate: true);
                }
            }
            if (success && EnableCustomerNextGenSync)
            {
                SyncDissociateCustomerAssetWithNextGen(terminationDate, viewsToTerminateWithCore, opCtx);
            }
            return terminatedViews;
        }

        private IList<ServiceView> TerminateServiceViewsForCustomerHierarchy(INH_OP opCtx, CustomerInfoDto customer, DateTime terminationDate, IList<ServiceViewDto> serviceViews)
        {
            var terminatedViews = new List<ServiceView>();

            if (customer == null)
                return terminatedViews;

            if (customer.TypeID == (int)CustomerTypeEnum.Account)
            {
                // Terminate ParentCustomer Views
                terminatedViews.AddRange(TerminateServiceViewsForCustomerHierarchy(opCtx, GetParent(opCtx, customer, CustomerTypeEnum.Customer), terminationDate, serviceViews));

                // Terminate ParentDealer Views
                terminatedViews.AddRange(TerminateServiceViewsForCustomerHierarchy(opCtx, GetParent(opCtx, customer, CustomerTypeEnum.Dealer), terminationDate, serviceViews));

                return terminatedViews;
            }

            // Terminate the views for the passed in customer
            var parent = customer;
            while (parent != null)
            {
                terminatedViews.AddRange(TerminateServiceViewsForCustomer(opCtx, parent, terminationDate, serviceViews));

                if (parent.TypeID == (int)CustomerTypeEnum.Dealer)
                {
                    var corporate = GetCorporateCustomerForDealerNetwork(opCtx, parent.DealerNetworkID);
                    terminatedViews.AddRange(TerminateServiceViewsForCustomer(opCtx, corporate, terminationDate, serviceViews));
                }

                parent = GetParent(opCtx, parent);
            }

            return terminatedViews;
        }

        #endregion

        #region Service Plan Updated action supporting methods

        public List<ServiceView> UpdateServiceAndServiceViews(INH_OP opContext, DateTime? ownerVisibilityDate,
            string ownerBSSID, long serviceID, long assetID, ServiceTypeEnum serviceType)
        {
            var service = opContext.Service.SingleOrDefault(t => t.ID == serviceID &&
                                                                                                                     t.CancellationKeyDate == DotNetExtensions.NullKeyDate &&
                                                                                                                     t.fk_ServiceTypeID == (int)serviceType);
            var viewsToTerminate = new List<ServiceViewWithCore>();
            var viewsToUpdate = new List<ServiceViewWithCore>();
            //no services exist for the device
            if (service == null)
                return new List<ServiceView>();
            var now = DateTime.UtcNow;
            var today = now.KeyDate();
            var serviceViews =
                (from sv in opContext.ServiceView
                 join s in opContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                 join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                 where sv.fk_ServiceID == service.ID && sv.Customer.fk_CustomerTypeID == (int) CustomerTypeEnum.Customer && sv.EndKeyDate > today
                 select new ServiceViewWithCore() {IsCore = st.IsCore, ServiceView = sv}
                    ).ToList();

            //just update the start keydate of the service view for all the customers in the hierarchy or
            //create service views for the service in the associated customer hierarchy
            if (service.OwnerVisibilityKeyDate.HasValue && ownerVisibilityDate.HasValue || !service.OwnerVisibilityKeyDate.HasValue && ownerVisibilityDate.HasValue)
            {
                var ownerVisibilityKeyDate = ownerVisibilityDate.KeyDate();
                service.OwnerVisibilityKeyDate = ownerVisibilityKeyDate;
                CreateServiceViewsForCustomer(opContext, service, ownerBSSID, assetID, ownerVisibilityKeyDate, serviceViews, viewsToUpdate);
            }
            else if (service.OwnerVisibilityKeyDate.HasValue && !ownerVisibilityDate.HasValue)
            {
                //terminate service views for the service in the associated customer hierarchy
                service.OwnerVisibilityKeyDate = null;
                TerminateServiceViewsForCustomer(opContext, service.ID, now.KeyDate(), viewsToTerminate);
            }
            var success = opContext.SaveChanges() > 0;
            if (success)
            {
                if (EnableSubscriptionNextGenSync)
                {
                    foreach (var sv in viewsToUpdate.Select(x=>x.ServiceView))
                        UpdateSubscription(opContext, sv, isStartDate: true);

                    foreach (var sv in viewsToTerminate.Select(x=>x.ServiceView))
                        UpdateSubscription(opContext, sv, isEndDate: true);

                }

                if (EnableCustomerNextGenSync)
                {
                    SyncAssociateCustomerAssetWithNextGen(opContext, viewsToUpdate );
                    SyncDissociateCustomerAssetWithNextGen(now, viewsToTerminate ,opContext);
                }
            }

            return serviceViews.Select(x=>x.ServiceView).ToList();
        }

        #endregion

        #region Private ServicePlan Update Methods

        private void CreateServiceViewsForCustomer(INH_OP opContext, Service service, string ownerBSSID, long assetID, int actionKeyDate, List<ServiceViewWithCore> serviceViews, List<ServiceViewWithCore> viewsToUpdate)
        {
            var account = opContext.CustomerReadOnly.Single(t => t.BSSID == ownerBSSID);

            //device is held by either dealer or customer. In either case service views can't be created for the customer hierarchy
            if (account.fk_CustomerTypeID != (int)CustomerTypeEnum.Account)
                return;

            CreateOrUpdateServiceViews(opContext, service, assetID, account, actionKeyDate, serviceViews, viewsToUpdate);
        }

        private void CreateOrUpdateServiceViews(INH_OP ctx, Service service, long assetID, Customer account, int actionKeyDate, List<ServiceViewWithCore> serviceViews, List<ServiceViewWithCore> viewsToUpdate)
        {
            account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSCustomer);
            while (account != null)
            {
                //if the service view is already exists for this customer don't re-create it, just update the start key date
                var existingServiceViews = serviceViews.Where(t => t.ServiceView.fk_CustomerID == account.ID && t.ServiceView.fk_ServiceID == service.ID && t.ServiceView.EndKeyDate > DateTime.UtcNow.KeyDate()).ToList();

                if (existingServiceViews.Count() != 0)
                {
                    foreach (var existingServiceView in existingServiceViews)
                    {
                        existingServiceView.ServiceView.StartKeyDate = actionKeyDate;
                        existingServiceView.ServiceView.UpdateUTC = DateTime.UtcNow;
                        viewsToUpdate.Add(existingServiceView);
                    }
                }
                else
                    serviceViews.Add(CreateServiceViewV2(ctx, account.ID, assetID, service.ID, service.CancellationKeyDate, service.OwnerVisibilityKeyDate.Value, (ServiceTypeEnum)service.fk_ServiceTypeID));

                account = GetParent(account.ID, CustomerRelationshipTypeEnum.TCSCustomer);
            }
        }

        private ServiceViewWithCore CreateServiceViewV2(INH_OP opContext, long customerID, long assetID, long serviceID, int endKeyDate, int startKeyDate, ServiceTypeEnum serviceType)
        {
            var today = DateTime.UtcNow.KeyDate();

            ServiceViewWithCore svWithCore =
             (from vw in opContext.ServiceView
              join s in opContext.ServiceReadOnly on vw.fk_ServiceID equals s.ID
              join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
              where vw.fk_AssetID == assetID &&
                          vw.fk_CustomerID == customerID &&
                          vw.fk_ServiceID == serviceID &&
                          vw.EndKeyDate >= today
              select new ServiceViewWithCore{ServiceView = vw, IsCore = st.IsCore}).FirstOrDefault();
            var sv = svWithCore == null ? null : svWithCore.ServiceView;
            // Service view already exists.  Un-terminate if required, otherwise just return it
            if (sv != null)
            {
                if (sv.EndKeyDate != endKeyDate)
                {
                    sv.EndKeyDate = endKeyDate;
                    sv.StartKeyDate = startKeyDate;
                    sv.UpdateUTC = DateTime.UtcNow;
                    var success = opContext.SaveChanges() > 0;
                    if (success && EnableSubscriptionNextGenSync)
                    {
                        UpdateSubscription(opContext, sv, isStartDate: true, isEndDate: true);
                    }
                }
                return svWithCore;
            }

            sv = new ServiceView
            {
                EndKeyDate = endKeyDate,
                fk_AssetID = assetID,
                fk_CustomerID = customerID,
                fk_ServiceID = serviceID,
                StartKeyDate = startKeyDate,
                UpdateUTC = DateTime.UtcNow,
                ServiceViewUID = Guid.NewGuid()
            };

            opContext.ServiceView.AddObject(sv);

            svWithCore = new ServiceViewWithCore {IsCore = ServicePlanIsCore(opContext, serviceType), ServiceView = sv};


            var createSuccess = opContext.SaveChanges() > 0;
            if (createSuccess)
            {
                if(EnableSubscriptionNextGenSync)
                    CreateSubscription(opContext, sv);
                if (EnableCustomerNextGenSync && svWithCore.IsCore)
                {
                    var assetDetails = (from a in opContext.AssetReadOnly
                                        //join ca in opContext.CustomerAssetReadOnly on a.AssetID equals ca.fk_AssetID
                                        where a.AssetID == assetID
                                        select new { a.AssetUID }).Single();


                    var custDetails = opContext.CustomerReadOnly.Where(c => c.ID == customerID).Select(x => new { x.CustomerUID, x.fk_CustomerTypeID }).Single();
                    // todo send this value: bool isOwned = assetDetails.IsOwned && assetDetails.fk_CustomerID == customerID;
                    RelationType rt =  MdmHelpers.GetRelationTypeForCustomerType((CustomerTypeEnum)custDetails.fk_CustomerTypeID);
                    MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opContext, _customerServiceApi, custDetails.CustomerUID.Value, assetDetails.AssetUID.Value, rt.ToString());
                }

            }

            return svWithCore;
        }

        #endregion

        #region Private Utiltiy Methods

        private CustomerInfoDto GetCustomerInfo(INH_OP opCtx, long customerId)
        {
            return (from c in opCtx.CustomerReadOnly
                    where c.ID == customerId
                    select new CustomerInfoDto { ID = c.ID, BSSID = c.BSSID, TypeID = c.fk_CustomerTypeID, DealerNetworkID = c.fk_DealerNetworkID, UID = c.CustomerUID.Value }).Single();
        }

        private CustomerInfoDto GetCorporateCustomerForDealerNetwork(INH_OP opCtx, int dealerNetworkId)
        {
            return (from c in opCtx.CustomerReadOnly
                    where c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate
                        && c.fk_DealerNetworkID == dealerNetworkId
                    select new CustomerInfoDto { ID = c.ID, BSSID = c.BSSID, TypeID = c.fk_CustomerTypeID, DealerNetworkID = c.fk_DealerNetworkID, UID = c.CustomerUID.Value }).FirstOrDefault();
        }

        private CustomerInfoDto GetParent(INH_OP opCtx, CustomerInfoDto child)
        {
            var parent = (from c in opCtx.Customer
                          join cr in opCtx.CustomerRelationship on c.ID equals cr.fk_ParentCustomerID
                          where cr.fk_ClientCustomerID == child.ID
                          select new CustomerInfoDto { ID = c.ID, BSSID = c.BSSID, TypeID = c.fk_CustomerTypeID, DealerNetworkID = c.fk_DealerNetworkID, UID = c.CustomerUID.Value }).SingleOrDefault();

            return parent;
        }

        private CustomerInfoDto GetChild(INH_OP opCtx, CustomerInfoDto child)
        {
            var grandchild = (from c in opCtx.Customer
                              join cr in opCtx.CustomerRelationship on c.ID equals cr.fk_ClientCustomerID
                              where cr.fk_ClientCustomerID == child.ID
                              select new CustomerInfoDto { ID = c.ID, BSSID = c.BSSID, TypeID = c.fk_CustomerTypeID, DealerNetworkID = c.fk_DealerNetworkID, UID = c.CustomerUID.Value }).SingleOrDefault();

            return grandchild;
        }

        private CustomerInfoDto GetParent(INH_OP opCtx, CustomerInfoDto child, CustomerTypeEnum parentType)
        {
            var parent = (from c in opCtx.Customer
                          join cr in opCtx.CustomerRelationship on c.ID equals cr.fk_ParentCustomerID
                          where cr.fk_ClientCustomerID == child.ID
                          && c.fk_CustomerTypeID == (int)parentType
                          select new CustomerInfoDto { ID = c.ID, BSSID = c.BSSID, TypeID = c.fk_CustomerTypeID, DealerNetworkID = c.fk_DealerNetworkID, UID = c.CustomerUID.Value }).SingleOrDefault();

            return parent;
        }

        #endregion

        #region DTO Classes

        public class CustomerInfoDto
        {
            public long ID { get; set; }
            public string BSSID { get; set; }
            public int TypeID { get; set; }
            public int DealerNetworkID { get; set; }
            public Guid UID { get; set; }

            public override string ToString()
            {
                return string.Format("ID={0}, BSSID={1}, TypeID={2}, DealerNetworkID={3}, UID={4}", ID, BSSID, TypeID, DealerNetworkID, UID);
            }
        }

        public class ServiceViewDto
        {
            public long ServiceId { get; set; }
            public long DeviceId { get; set; }
            public int ActivationKeyDate { get; set; }
            public int? OwnerVisibilityDate { get; set; }
            public long AssetId { get; set; }
            public int StartDate { get; set; }
            public Guid AssetGuid { get; set; }
            public bool IsCore { get; set; }

        }

        #endregion

        #endregion

        #region DeviceReplacement for BSS V2

        /// <summary>
        /// Transfer services from one device to another device and 
        /// reconfigure the devices for the remaining/active service plans
        /// </summary>
        /// <param name="oldDeviceID">ID of the device from which the services needs to be transferred</param>
        /// <param name="newDeviceID">ID of the new Device for which the services getting transferred to</param>
        /// <param name="actionUTC">Date since the transfer is effective</param>
        /// <returns>List of services which are transferred from old device to new device</returns>
        public List<Service> TransferServices(INH_OP opContext, long oldDeviceID, long newDeviceID, DateTime actionUTC)
        {

            #region validations

            if (oldDeviceID == default(long))
                throw new ArgumentException("oldDeviceID must have a value.", "oldDeviceID");

            if (newDeviceID == default(long))
                throw new ArgumentException("newDeviceID must have a value.", "newDeviceID");

            if (actionUTC == default(DateTime))
                throw new ArgumentException("actionUTC must have a value.", "actionUTC");

            #endregion

            var keyDate = actionUTC.KeyDate();

            var servicesToTransfer = (from s in opContext.Service
                                      join st in opContext.ServiceType on s.fk_ServiceTypeID equals st.ID
                                      where s.fk_DeviceID == oldDeviceID &&
                                      s.ActivationKeyDate <= keyDate &&
                                      s.CancellationKeyDate == DotNetExtensions.NullKeyDate
                                      //sort the services in the assending order so that the device is configured for 
                                      //the service plans from low to high instead of random order
                                      orderby st.ID
                                      select new { Service = s, IsCore = st.IsCore }).ToList();

            foreach (var service in servicesToTransfer)
            {
                service.Service.fk_DeviceID = newDeviceID;
            }

            opContext.SaveChanges();

            //HACK: MMather's hack to cover other hacks
            var oldAndNew = (from oldD in opContext.DeviceReadOnly
                             from newD in opContext.DeviceReadOnly
                             where oldD.ID == oldDeviceID && newD.ID == newDeviceID
                             select new
                             {
                                 oldState = oldD.fk_DeviceStateID,
                                 newState = newD.fk_DeviceStateID
                             }).FirstOrDefault();

            if (oldAndNew != null)
            {
                //update the device state of new Device to 'Subscribed'
                if (oldAndNew.newState != (int)DeviceStateEnum.DeregisteredStore &&
                    oldAndNew.newState != (int)DeviceStateEnum.DeregisteredTechnician)
                {
                    if (servicesToTransfer.Any(t => t.IsCore))
                        API.Device.UpdateDeviceState(newDeviceID, DeviceStateEnum.Subscribed);
                    else
                        API.Device.UpdateDeviceState(newDeviceID, DeviceStateEnum.Provisioned);
                }

                //update the device state of old Device to 'Provisioned'
                if (oldAndNew.oldState != (int)DeviceStateEnum.DeregisteredStore &&
                    oldAndNew.oldState != (int)DeviceStateEnum.DeregisteredTechnician)
                    API.Device.UpdateDeviceState(oldDeviceID, DeviceStateEnum.Provisioned);
            }

            var assetID = (from asset in opContext.AssetReadOnly
                           where asset.fk_DeviceID == newDeviceID
                           select asset.AssetID).FirstOrDefault();

            if (EnableDeviceNextGenSync)
            {
                    ReplaceDevice(opContext, oldDeviceID, newDeviceID);
            }

            return servicesToTransfer.Select(t => t.Service).ToList();
        }

        /// <summary>
        /// Move Service Views from old asset and re-create the same service views for the new asset
        /// Actions:
        ///   1. Terminate services views on old asset 
        ///   2. Create service views on new asset
        /// </summary>
        /// <param name="oldAssetID">ID of the old asset for which the service views needs to be terminated</param>
        /// <param name="newAssetID">ID of the new asset for which the service views needs to be re-created</param>
        /// <param name="actionUTC">Date since the service view termination/re-creation start</param>
        /// <returns>A tuple containing the service views terminated for old asset and service views created for new asset</returns>
        public bool SwapServiceViewsBetweenOldAndNewAsset(INH_OP opContext, long oldAssetID, long newAssetID, DateTime actionUTC)
        {

            #region validations

            if (oldAssetID == default(long))
                throw new ArgumentException("oldAssetID must have a value.", "oldAssetID");

            if (newAssetID == default(long))
                throw new ArgumentException("newAssetID must have a value.", "newAssetID");

            if (actionUTC == default(DateTime))
                throw new ArgumentException("actionUTC must have a value.", "actionUTC");

            #endregion

            var today = DateTime.UtcNow.KeyDate();
            var svsForOldAsset = (from sv in opContext.ServiceView
                                  join service in opContext.ServiceReadOnly on sv.fk_ServiceID equals service.ID
                                  join st in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                                  where sv.fk_AssetID == oldAssetID &&
                                  sv.EndKeyDate > today
                                  select new ServiceViewWithCore() { ServiceView = sv, IsCore = st.IsCore }).ToList();

            var svsForNewAsset = (from sv in opContext.ServiceView
                                  join service in opContext.ServiceReadOnly on sv.fk_ServiceID equals service.ID
                                  join st in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                                  where sv.fk_AssetID == newAssetID &&
                                  sv.EndKeyDate > today
                                  select new ServiceViewWithCore() { ServiceView = sv, IsCore = st.IsCore }).ToList();

            var viewsToCreate = new List<ServiceViewWithCore>();
            var viewsToUpdate = new List<ServiceViewWithCore>();

            //terminate service views of old asset and create for new asset
            foreach (var item in svsForOldAsset)
                CreateAndTerminateServiceView(opContext, item, newAssetID, actionUTC, viewsToCreate, viewsToUpdate);

            //terminate service views of new asset and create for old asset
            foreach (var item in svsForNewAsset)
                CreateAndTerminateServiceView(opContext, item, oldAssetID, actionUTC, viewsToCreate, viewsToUpdate);

            //return empty collection if no service views created and/or terminated
            if (svsForNewAsset.Count == 0 && svsForOldAsset.Count == 0)
                return true;

            var success = opContext.SaveChanges() > 0;
            if (success)
            {
                if (EnableSubscriptionNextGenSync)
                {
                    foreach (var sv in viewsToCreate.Select(x=>x.ServiceView))
                    {
                        CreateSubscription(opContext, sv);
                    }
                    foreach (var sv in viewsToUpdate.Select(x=>x.ServiceView))
                    {
                        UpdateSubscription(opContext, sv, isEndDate: true);
                    }
                }

                if (EnableCustomerNextGenSync)
                {
                    SyncAssociateCustomerAssetWithNextGen(opContext, viewsToCreate);
                    SyncDissociateCustomerAssetWithNextGen(actionUTC, viewsToUpdate, opContext);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Verifies that the device transfer is valid or not
        /// </summary>
        /// <param name="oldDeviceId">Old Device ID which is being replaced</param>
        /// <param name="newDeviceType">new Device Type which is replacing the old device</param>
        /// <returns>A boolean value indicating the validity of the transfer. A true value indicates valid and false indicates invalid</returns>
        public bool IsDeviceTransferValid(INH_OP opContext, long oldDeviceId, DeviceTypeEnum newDeviceType)
        {
            var keyDate = DateTime.UtcNow.KeyDate();

            var oldDeviceServices = (from s in opContext.ServiceReadOnly
                                     where s.fk_DeviceID == oldDeviceId &&
                                     s.CancellationKeyDate > keyDate
                                     select s.fk_ServiceTypeID).ToList();

            var supportedServices = (from dtst in opContext.DeviceTypeServiceTypeReadOnly
                                     where dtst.fk_DeviceTypeID == (int)newDeviceType
                                     select dtst.fk_ServiceTypeID).ToList();

            if (oldDeviceServices.Intersect(supportedServices).Count() != oldDeviceServices.Count())
                return false;
            return true;
        }

        private List<DeviceConfig.ServicePlanIDs> GetServicePlanIDs(IEnumerable<Tuple<long, bool>> servicePlanIDs)
        {
            var IDs = new List<DeviceConfig.ServicePlanIDs>();

            if (servicePlanIDs == null)
                return IDs;

            foreach (var svc in servicePlanIDs)
                IDs.Add(new DeviceConfig.ServicePlanIDs { PlanID = svc.Item1, IsCore = svc.Item2 });

            return IDs;
        }

        private void CreateAndTerminateServiceView(INH_OP opContext, ServiceViewWithCore svWithCore, long assetID, DateTime actionUTC, List<ServiceViewWithCore> viewsToCreate, List<ServiceViewWithCore> viewsToUpdate)
        {
            var keyDate = actionUTC.KeyDate();
            var sv = svWithCore.ServiceView;
            var newServiceView =
                new ServiceViewWithCore{
                    IsCore = svWithCore.IsCore,
                    ServiceView = new ServiceView
            {
                EndKeyDate = DotNetExtensions.NullKeyDate,
                StartKeyDate = keyDate,
                fk_AssetID = assetID,
                fk_CustomerID = sv.fk_CustomerID,
                fk_ServiceID = sv.fk_ServiceID,
                UpdateUTC = DateTime.UtcNow,
                ServiceViewUID = Guid.NewGuid()
            }};
            sv.EndKeyDate = keyDate;
            sv.UpdateUTC = DateTime.UtcNow;
            viewsToUpdate.Add(svWithCore);
            viewsToCreate.Add(newServiceView);
            opContext.ServiceView.AddObject(newServiceView.ServiceView);
        }

        public AssetDeviceHistory CreateAssetDeviceHistory(INH_OP opContext, long assetId, long deviceId, string ownerBssId, DateTime startUtc)
        {

            #region Arg Validation

            if (assetId == default(long))
                throw new ArgumentException("assetId must have a value.", "assetId");

            if (deviceId == default(long))
                throw new ArgumentException("deviceId must have a value.", "deviceId");

            if (string.IsNullOrWhiteSpace(ownerBssId))
                throw new ArgumentException("ownerBssId must have a value.", "ownerBssId");

            if (startUtc == default(DateTime))
                throw new ArgumentException("startUtc must have a value.", "startUtc");

            #endregion

            var existingHistory = (from adh in opContext.AssetDeviceHistoryReadOnly
                                   where adh.fk_DeviceID == deviceId
                                   orderby adh.StartUTC descending
                                   select adh.EndUTC).FirstOrDefault();

            var assetDeviceHistory = new AssetDeviceHistory
            {
                fk_AssetID = assetId,
                fk_DeviceID = deviceId,
                OwnerBSSID = ownerBssId,
                StartUTC = existingHistory.HasValue ? existingHistory.Value : startUtc,
                EndUTC = DateTime.UtcNow
            };

            opContext.AssetDeviceHistory.AddObject(assetDeviceHistory);

            if (opContext.SaveChanges() > 0)
                return assetDeviceHistory;

            return null;
        }

        /// <summary>
        /// Reconfigure the device reporting intervals based on the service plas activated
        /// </summary>
        /// <param name="assetID">Asset for which the device is associated</param>
        /// <param name="actionUTC">The date since these changes will take effect</param>
        /// <param name="gpsDeviceID">Gps Device ID of the device</param>
        /// <param name="deviceType">Type of the device</param>
        /// <param name="serviceType">Type of the service</param>
        /// <returns>A boolean value indicating the status of the configuration. A true value indicates success and false indicates failure</returns>
        public bool ConfigureDeviceForActivatedServicePlans(INH_OP opContext, long assetID, DateTime actionUTC, string gpsDeviceID, DeviceTypeEnum deviceType, ServiceTypeEnum serviceType, bool isAdded, long? serviceID)
        {
            var utcNowKeyDate = actionUTC.KeyDate();
            var currentViews = (from view in opContext.ServiceViewReadOnly
                                join service in opContext.ServiceReadOnly on view.fk_ServiceID equals service.ID
                                join st in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                                where view.fk_AssetID == assetID
                                            && view.StartKeyDate <= utcNowKeyDate
                                            && view.EndKeyDate > utcNowKeyDate
                                            && view.Customer.fk_CustomerTypeID != (int)CustomerTypeEnum.Corporate
                                orderby st.ID
                                select new
                                {
                                    ServiceTypeID = st.ID,
                                    IsCore = st.IsCore,
                                    BSSPlanLineID = service.BSSLineID
                                }).ToList();

            var success = DeviceConfig.ConfigureDeviceForServicePlan(opContext, gpsDeviceID,
                                         deviceType, isAdded, serviceType, GetServicePlanIDs(currentViews.Select(t => new Tuple<long, bool>(t.ServiceTypeID, t.IsCore)).ToList()));

            opContext.SaveChanges();

            if (currentViews.Count > 0)
                return success;

            return true;
        }

        #endregion

        #region AssetRepossession Methods

        public IList<ServiceView> TerminateServiceViewAtBeginRepo(long assetID, long customerID, INH_OP ctx, int today,
                                                                                                    int yesterday, DateTime updateUTC)
        {
            var viewsToTerminate = (from sv in ctx.ServiceView
                                    where sv.fk_AssetID == assetID
                                                && sv.fk_CustomerID != customerID
                                                && sv.EndKeyDate > today
                                    select sv).ToList();

            foreach (var serviceView in viewsToTerminate)
            {
                serviceView.EndKeyDate = yesterday;
                serviceView.UpdateUTC = updateUTC;
            }

            return viewsToTerminate;
        }

        public IList<ServiceView> CreateServiceViewAfterEndRepo(long assetID, long customerID,
                                                                                                AssetReposessionHistory assetRepoStatus, INH_OP ctx, int today,
                                                                                                int tomorrow, DateTime updateUTC)
        {
            var viewsToCreate = new List<ServiceView>();
            var svList = (from sv in ctx.ServiceViewReadOnly
                          join svo in ctx.ServiceViewReadOnly on
                              new
                              {
                                  sv.fk_ServiceID,
                                  sv.fk_AssetID
                              }
                              equals new
                              {
                                  svo.fk_ServiceID,
                                  svo.fk_AssetID
                              }
                          where sv.fk_CustomerID == customerID &&
                                      sv.fk_AssetID == assetID &&
                                      sv.EndKeyDate > today &&
                                      svo.EndKeyDate == assetRepoStatus.StartKeyDate &&
                                      svo.fk_CustomerID != customerID &&
                                      svo.ifk_SharedViewID == null
                          select new
                          {
                              sv.EndKeyDate,
                              sv.fk_ServiceID,
                              svo.fk_CustomerID
                          }).ToList();
            foreach (var sv in svList)
            {
                var newSv = new ServiceView();
                newSv.EndKeyDate = sv.EndKeyDate;
                newSv.StartKeyDate = tomorrow;
                newSv.fk_AssetID = assetID;
                newSv.fk_CustomerID = sv.fk_CustomerID;
                newSv.fk_ServiceID = sv.fk_ServiceID;
                newSv.UpdateUTC = updateUTC;
                viewsToCreate.Add(newSv);
                ctx.ServiceView.AddObject(newSv);
            }
            return viewsToCreate;
        }

        #endregion
        public IList<ServiceViewDto> GetViewsToCreateForCustomer(INH_OP ctx, CustomerInfoDto customer, IList<ServiceViewDto> activeViews, DateTime? startDateUtc = null)
        {
            var serviceIds = (customer.TypeID != (int)CustomerTypeEnum.Customer)
                                             ? activeViews.Select(x => x.ServiceId).Distinct().ToList()
                                             : activeViews.Where(x => x.OwnerVisibilityDate != null).Select(x => x.ServiceId).Distinct().ToList();
            var index = 0;
            var count = serviceIds.Count;
            var blockSize = GetServiceViewBlockSizeValue();
            var existingViews = new List<ServiceView>();

            while (index < count)
            {
                var serviceIdBlock = serviceIds.Skip(index).Take(blockSize);
                existingViews.AddRange(from view in ctx.ServiceViewReadOnly
                                       where view.fk_CustomerID == customer.ID
                                                   && serviceIdBlock.Contains(view.fk_ServiceID)
                                       orderby view.EndKeyDate descending
                                       select view);
                index += blockSize;
            }

            var viewsToCreate = new List<ServiceViewDto>();
            foreach (var activeView in activeViews)
            {
                var startDate = startDateUtc != null ? startDateUtc.KeyDate() : activeView.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate();
                if (customer.TypeID == (int)CustomerTypeEnum.Customer)
                {
                    if (!activeView.OwnerVisibilityDate.HasValue)
                        continue;

                    if (startDateUtc == null)
                        startDate = activeView.OwnerVisibilityDate.Value;
                }

                var matchedRow = existingViews.FirstOrDefault(e => e.fk_ServiceID == activeView.ServiceId && e.fk_AssetID == activeView.AssetId);

                if (matchedRow != null && matchedRow.EndKeyDate == NullEndDate)
                    continue;

                viewsToCreate.Add(new ServiceViewDto
                {
                    ServiceId = activeView.ServiceId,
                    DeviceId = activeView.DeviceId,
                    ActivationKeyDate = activeView.ActivationKeyDate,
                    OwnerVisibilityDate = activeView.OwnerVisibilityDate,
                    AssetId = activeView.AssetId,
                    IsCore = activeView.IsCore,
                    StartDate = (matchedRow != null && matchedRow.EndKeyDate > activeView.StartDate) ? matchedRow.EndKeyDate : startDate
                });

            }
            return viewsToCreate;
        }

        public IList<ServiceView> CreateRelationshipServiceViews(long parentId, long childId)
        {
            var createdViews = new List<ServiceView>();

            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var child = GetCustomerInfo(opCtx, childId);

                List<ServiceViewDto> activeServices;

                if (child.TypeID == (int)CustomerTypeEnum.Account)
                {
                    // Get active services by device ownership
                    activeServices = (from s in opCtx.ServiceReadOnly
                                      join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                      join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                                      join a in opCtx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                      where d.OwnerBSSID == child.BSSID
                                                  && s.CancellationKeyDate == NullEndDate
                                      select new ServiceViewDto
                                                       {
                                                           ServiceId = s.ID,
                                                           DeviceId = s.fk_DeviceID,
                                                           ActivationKeyDate = s.ActivationKeyDate,
                                                           OwnerVisibilityDate = s.OwnerVisibilityKeyDate,
                                                           AssetId = a.AssetID,
                                                           AssetGuid = a.AssetUID.Value,
                                                           IsCore = st.IsCore
                                                       }).ToList();
                }
                else
                {
                    // Get active services by service view
                    activeServices = (from s in opCtx.ServiceReadOnly
                                      join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                      join sv in opCtx.ServiceViewReadOnly on s.ID equals sv.fk_ServiceID
                                      join a in opCtx.AssetReadOnly on sv.fk_AssetID equals a.AssetID
                                      where sv.fk_CustomerID == child.ID
                                                  && sv.EndKeyDate == NullEndDate
                                      select new ServiceViewDto
                                                       {
                                                           ServiceId = s.ID,
                                                           DeviceId = s.fk_DeviceID,
                                                           ActivationKeyDate = s.ActivationKeyDate,
                                                           OwnerVisibilityDate = s.OwnerVisibilityKeyDate,
                                                           AssetId = sv.fk_AssetID,
                                                           AssetGuid = a.AssetUID.Value,
                                                           IsCore = st.IsCore
                                                       }).ToList();
                }

                foreach (var customer in GetHierarchyCustomers(opCtx, GetCustomerInfo(opCtx, parentId)))
                {
                    var viewsToCreate = GetViewsToCreateForCustomer(opCtx, customer, activeServices);

                    foreach (var viewToCreate in viewsToCreate)
                    {
                        if (customer.TypeID == (int)CustomerTypeEnum.Customer && !viewToCreate.OwnerVisibilityDate.HasValue)
                            continue;

                        createdViews.Add(CreateServiceView(viewToCreate.ServiceId, customer.ID, viewToCreate.AssetId, viewToCreate.StartDate));
                        if (viewToCreate.IsCore && EnableCustomerNextGenSync)
                            MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opCtx, _customerServiceApi, customer.UID, viewToCreate.AssetGuid, MdmHelpers.GetRelationTypeForCustomerType((CustomerTypeEnum)customer.TypeID).ToString());
                    }
                }

                // Bulk insert the created views into the database.
                var newViews = ServiceViewSaver.Save(createdViews);
                if (EnableSubscriptionNextGenSync)
                {
                    foreach (var createdView in createdViews)
                    {
                        CreateSubscription(opCtx, createdView);
                    }
                }

                return newViews;
            }
        }

        public IList<ServiceView> TerminateRelationshipServiceViews(long parentId, long childId, DateTime endDateUtc)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var child = GetCustomerInfo(opCtx, childId);
                var parent = GetCustomerInfo(opCtx, parentId);

                IList<long> activeServiceIds;
                IList<long> corpIdsToExclude = new List<long>();

                if (child.TypeID == (int)CustomerTypeEnum.Account)
                {
                    activeServiceIds = (from s in opCtx.ServiceReadOnly
                                        join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                                        where d.OwnerBSSID == child.BSSID
                                                    && s.CancellationKeyDate == NullEndDate
                                        select s.ID).ToList();
                }
                else
                {
                    activeServiceIds = (from s in opCtx.ServiceReadOnly
                                        join sv in opCtx.ServiceViewReadOnly on s.ID equals sv.fk_ServiceID
                                        where sv.fk_CustomerID == child.ID
                                                    && s.CancellationKeyDate == NullEndDate
                                        select s.ID).ToList();

                    //child dealer network and parent dealer network is same, 
                    //don't terminate the corporate service views
                    var info = GetCorporateCustomerForDealerNetwork(opCtx, child.DealerNetworkID);
                    if (info == null)
                    {
                        Log.IfWarnFormat(
                            "Unable to look up the dealer network for customer \"{0}\".  No service views will be modified for respective corporate customers.",
                            child);
                        info = GetCorporateCustomerForDealerNetwork(opCtx, parent.DealerNetworkID);
                        if (info == null)
                        {
                            Log.IfWarnFormat(
                                "Unable to look up the dealer network for customer \"{0}\".  No service views will be modified for respective corporate customers.",
                                parent);
                        }
                        else
                        {
                            corpIdsToExclude.Add(info.ID);
                        }
                    }
                    else
                    {
                        corpIdsToExclude.Add(info.ID);
                    }
                    if (parent.DealerNetworkID != child.DealerNetworkID)
                    {
                        const int customerType = (int)CustomerTypeEnum.Account;
                        while (true)
                        {
                            var childDealer = GetChild(opCtx, child);
                            if (childDealer.TypeID == customerType || child.ID == childDealer.ID)
                                break;
                            if (child.DealerNetworkID != childDealer.DealerNetworkID)
                            {
                                info = GetCorporateCustomerForDealerNetwork(opCtx, childDealer.DealerNetworkID);
                                if (info == null)
                                {
                                    Log.IfWarnFormat("Unable to look up the dealer network for customer \"{0}\".  No service views will be modified for respective corporate customers.", childDealer);
                                }
                                else
                                {
                                    corpIdsToExclude.Add(info.ID);
                                }
                            }
                        }
                    }
                }

                var customers = GetHierarchyCustomers(opCtx, parent);
                var customerIds = customers.Select(x => x.ID).Except(corpIdsToExclude);
                var customerIdMap = customers.ToDictionary(x => x.ID, y => y.UID);

                var viewsToTerminateWithCore = (from sv in opCtx.ServiceView
                                                join s in opCtx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                                                join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                                where activeServiceIds.Contains(sv.fk_ServiceID)
                                                            && customerIds.Contains(sv.fk_CustomerID)
                                                            && sv.EndKeyDate == NullEndDate
                                                select new { ServiceView = sv, st.IsCore }).ToList();
                var viewsToTerminate = viewsToTerminateWithCore.Select(x => x.ServiceView).ToList();

                foreach (var serviceView in viewsToTerminate)
                {
                    opCtx.ServiceView.Detach(serviceView); // We'll be saving back to the db using bulk update.
                    serviceView.EndKeyDate = endDateUtc.KeyDate();
                }

                // Save as bulk
                ServiceViewSaver.Save(viewsToTerminate);
                bool terminateInThePast = endDateUtc <= DateTime.UtcNow;
                foreach (var svItem in viewsToTerminateWithCore)
                {
                    var viewToTerminate = svItem.ServiceView;
                    if (EnableSubscriptionNextGenSync)
                        UpdateSubscription(opCtx, viewToTerminate, isEndDate: true);
                    if (EnableCustomerNextGenSync && svItem.IsCore && terminateInThePast)
                    {
                        var assetGuid = opCtx.AssetReadOnly.Where(x => x.AssetID == viewToTerminate.fk_AssetID).Select(x => x.AssetUID).First();
                        if (assetGuid.HasValue)
                            MdmHelpers.SyncDissociateCustomerAssetWithNextGen(opCtx, _customerServiceApi, endDateUtc, assetGuid.Value, customerIdMap[viewToTerminate.fk_CustomerID], Log);
                    }
                }


                return viewsToTerminate;
            }
        }

        public IList<ServiceView> CreateAssetServiceViews(long assetId, DateTime? startDateUtc = null)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                List<ServiceViewDto> activeServices;

                activeServices = (from s in opCtx.ServiceReadOnly
                                  join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                  join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                                  join a in opCtx.AssetReadOnly on d.ID equals a.fk_DeviceID
                                  where a.AssetID == assetId
                                              && s.CancellationKeyDate == NullEndDate
                                  select new ServiceViewDto
                                                   {
                                                       ServiceId = s.ID,
                                                       DeviceId = s.fk_DeviceID,
                                                       ActivationKeyDate = s.ActivationKeyDate,
                                                       OwnerVisibilityDate = s.OwnerVisibilityKeyDate,
                                                       AssetId = a.AssetID,
                                                       AssetGuid = a.AssetUID.Value,
                                                       IsCore = st.IsCore
                                                   }).ToList();

                var owner = (from c in opCtx.CustomerReadOnly
                             join d in opCtx.DeviceReadOnly on c.BSSID equals d.OwnerBSSID
                             join a in opCtx.AssetReadOnly on d.ID equals a.fk_DeviceID
                             where a.AssetID == assetId
                             select new CustomerInfoDto
                                                {
                                                    ID = c.ID,
                                                    TypeID = c.fk_CustomerTypeID,
                                                    DealerNetworkID = c.fk_DealerNetworkID,
                                                    BSSID = c.BSSID,
                                                    UID = c.CustomerUID.Value
                                                }).FirstOrDefault();

                IList<ServiceView> createdViews = new List<ServiceView>();

                foreach (var customer in GetHierarchyCustomers(opCtx, owner))
                {
                    var viewsToCreate = GetViewsToCreateForCustomer(opCtx, customer, activeServices, startDateUtc);

                    foreach (var viewToCreate in viewsToCreate)
                    {
                        if (customer.TypeID == (int)CustomerTypeEnum.Customer && !viewToCreate.OwnerVisibilityDate.HasValue)
                            continue;

                        var newServiceView = CreateServiceView(
                            viewToCreate.ServiceId,
                            customer.ID,
                            viewToCreate.AssetId,
                            viewToCreate.StartDate);

                        opCtx.ServiceView.AddObject(newServiceView);
                        opCtx.SaveChanges();
                        createdViews.Add(newServiceView);
                    }
                    if (EnableSubscriptionNextGenSync)
                    {
                        foreach (var createdView in createdViews)
                        {
                            CreateSubscription(opCtx, createdView);
                        }
                    }
                    if (EnableCustomerNextGenSync)
                    {

                        //bool owner =  owner.ID == customer.ID
                        RelationType rt = MdmHelpers.GetRelationTypeForCustomerType((CustomerTypeEnum)customer.TypeID);

                        foreach (var viewToCreate in viewsToCreate.Where(x => x.IsCore))
                            MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opCtx, _customerServiceApi, customer.UID, viewToCreate.AssetGuid, rt.ToString());
                    }
                }
                return createdViews;
            }
        }

        public IList<ServiceView> TerminateAssetServiceViews(long assetId, DateTime terminationDate)
        {
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var terminationKeyDate = terminationDate.KeyDate();

                var viewsToTerminateWithCore = (from s in opCtx.Service
                                                join sv in opCtx.ServiceView on s.ID equals sv.fk_ServiceID
                                                join st in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                                where sv.fk_AssetID == assetId
                                                            && sv.EndKeyDate > terminationKeyDate
                                                select new ServiceViewWithCore { ServiceView = sv, IsCore = st.IsCore }).ToList();
                var viewsToTerminate = viewsToTerminateWithCore.Select(x => x.ServiceView).ToList();
                foreach (var serviceView in viewsToTerminate)
                {
                    serviceView.EndKeyDate = terminationDate.KeyDate();
                    serviceView.UpdateUTC = DateTime.UtcNow;
                }

                var success = opCtx.SaveChanges() > 0;
                if (success)
                {

                    if (EnableSubscriptionNextGenSync)
                        foreach (var viewToTerminate in viewsToTerminateWithCore.Select(x => x.ServiceView))
                            UpdateSubscription(opCtx, viewToTerminate, isEndDate: true);

                    if (EnableCustomerNextGenSync)
                    {
                        SyncDissociateCustomerAssetWithNextGen(terminationDate, viewsToTerminateWithCore, opCtx);
                    }
                }

                return viewsToTerminate;
            }
        }

        private void SyncAssociateCustomerAssetWithNextGen(INH_OP opContext, List<ServiceViewWithCore> viewsToCreate)
        {
            var coreViews = viewsToCreate.Where(x => x.IsCore).Select(x => x.ServiceView).ToList();
            if (!coreViews.Any())
                return;
            var assetKeyMap = coreViews.GroupBy(x => x.fk_AssetID)
                .ToDictionary(x => x.Key,
                    x => opContext.AssetReadOnly.Where(a => a.AssetID == x.Key).Select(a => a.AssetUID).FirstOrDefault());
            var custKeyMap = coreViews.GroupBy(x => x.fk_CustomerID)
                .ToDictionary(x => x.Key,
                    x => opContext.CustomerReadOnly.Where(c => c.ID == x.Key).Select(c => new {c.CustomerUID, c.fk_CustomerTypeID}).FirstOrDefault());
            //todo owner calculation in a sust
            foreach (var viewToCreate in coreViews)
            {
                if (EnableCustomerNextGenSync && assetKeyMap[viewToCreate.fk_AssetID].HasValue &&
                        custKeyMap[viewToCreate.fk_CustomerID].CustomerUID.HasValue)
                    MdmHelpers.SyncAssociateCustomerAssetWithNextGen(opContext,_customerServiceApi, custKeyMap[viewToCreate.fk_CustomerID].CustomerUID.Value,
                        assetKeyMap[viewToCreate.fk_AssetID].Value, MdmHelpers.GetRelationTypeForCustomerType((CustomerTypeEnum)custKeyMap[viewToCreate.fk_CustomerID].fk_CustomerTypeID).ToString());
            }
        }

        private void SyncDissociateCustomerAssetWithNextGen(DateTime terminationDate, List<ServiceViewWithCore> viewsToTerminateWithCore, INH_OP opCtx)
        {
            if (terminationDate > DateTime.UtcNow)
                return;

            var coreViews = viewsToTerminateWithCore.Where(x => x.IsCore).Select(x => x.ServiceView).ToList();
            if (!coreViews.Any())
                return;
            var assetGuidMap = coreViews.GroupBy(x => x.fk_AssetID)
                .ToDictionary(x => x.Key,
                    x => opCtx.AssetReadOnly.Where(a => a.AssetID == x.Key).Select(a => a.AssetUID).FirstOrDefault());
            var custGuidMap = coreViews.GroupBy(x => x.fk_CustomerID)
                .ToDictionary(x => x.Key,
                    x => opCtx.CustomerReadOnly.Where(c => c.ID == x.Key).Select(c =>  c.CustomerUID).FirstOrDefault());

            foreach (var svItem in coreViews)
            {
                Guid? assetGuid = assetGuidMap[svItem.fk_AssetID];
                Guid? custGuid = custGuidMap[svItem.fk_CustomerID];

                if (assetGuid.HasValue && custGuid.HasValue)
                    MdmHelpers.SyncDissociateCustomerAssetWithNextGen(opCtx, _customerServiceApi, terminationDate, assetGuid.Value, custGuid.Value, Log);
            }
        }


        public bool CanCreateServiceViewForOrganization(INH_OP opCtx, long deviceOwnerCustomerId, long organizationId)
        {
            return GetHierarchyCustomers(opCtx, GetCustomerInfo(opCtx, deviceOwnerCustomerId)).
                            Any(c => c.ID == organizationId);
        }

        private IList<CustomerInfoDto> GetHierarchyCustomers(INH_OP opCtx, CustomerInfoDto customer)
        {
            var hierarchyCustomers = new List<CustomerInfoDto>();

            if (customer == null)
                return hierarchyCustomers;

            if (customer.TypeID == (int)CustomerTypeEnum.Account)
            {
                hierarchyCustomers.AddRange(
                    GetHierarchyCustomers(opCtx, GetParent(opCtx, customer, CustomerTypeEnum.Customer)));

                hierarchyCustomers.AddRange(
                    GetHierarchyCustomers(opCtx, GetParent(opCtx, customer, CustomerTypeEnum.Dealer)));

                return hierarchyCustomers;
            }


            if (!hierarchyCustomers.Select(x => x.ID).Contains(customer.ID))
                hierarchyCustomers.Add(customer);

            var parent = customer;
            while (parent != null)
            {
                hierarchyCustomers.AddRange(GetHierarchyCustomers(opCtx, GetParent(opCtx, parent)));

                if (parent.TypeID == (int)CustomerTypeEnum.Dealer)
                {
                    var corporate = GetCorporateCustomerForDealerNetwork(opCtx, parent.DealerNetworkID);
                    if (corporate != null && !hierarchyCustomers.Select(x => x.ID).Contains(corporate.ID)) hierarchyCustomers.Add(corporate);
                }

                parent = GetParent(opCtx, parent);
            }

            return hierarchyCustomers;
        }

        private ServiceView CreateServiceView(long serviceId, long customerId, long assetId, int startKeyDate)
        {
            //var existingView = (from sv in opCtx.ServiceViewReadOnly
            //                    where sv.fk_ServiceID == serviceId
            //                          && sv.fk_CustomerID == customerId
            //                          && sv.fk_AssetID == assetId
            //                    orderby sv.UpdateUTC descending 
            //                    select sv).FirstOrDefault();

            //if(existingView != null)
            //{
            //  if(existingView.EndKeyDate == NULL_END_DATE)
            //  {
            //    return null;
            //  }
            //  if (existingView.EndKeyDate > startKeyDate)
            //  {
            //    startKeyDate = existingView.EndKeyDate;
            //  }
            //}

            var newView = new ServiceView
            {
                fk_ServiceID = serviceId,
                fk_CustomerID = customerId,
                fk_AssetID = assetId,
                StartKeyDate = startKeyDate,
                EndKeyDate = NullEndDate,
                UpdateUTC = DateTime.UtcNow,
                ServiceViewUID = Guid.NewGuid()
            };

            // Saving to the database is the responsibility of calling method 
            // to save one at a time or in bulk using ServiceViewAccess sproc wrapper class.
            // DO NOT SAVE HERE.
            return newView;

        }

        /// <summary>
        /// Check to see if Asset has been previously OnBoarded.
        /// </summary>
        /// <param name="opContext">NH_OP context</param>
        /// <param name="deviceID">Device ID</param>
        /// <param name="assetID">Asset ID</param>
        /// <returns>Has Asset Been OnBoarded?</returns>
        private static bool IsOnboardingAsset(INH_OP opContext, long deviceID, long assetID)
        {
            return !HasDeviceBeenActivatedBefore(opContext, deviceID) &&
                         !AssetHasHadOtherDevicesOrPreviousOwners(opContext, assetID);
        }

        /// <summary>
        /// Checks to see if Device has ever had Core service cancelled
        /// </summary>
        /// <param name="opContext">NH_OP Context</param>
        /// <param name="deviceID">Device id to check</param>
        /// <returns>Cancelled?</returns>
        private static bool HasDeviceBeenActivatedBefore(INH_OP opContext, long deviceID)
        {
            var keyDateNow = DateTime.UtcNow.KeyDate();
            return (from svc in opContext.ServiceReadOnly
                    join svct in opContext.ServiceTypeReadOnly on svc.fk_ServiceTypeID equals svct.ID
                    where svc.fk_DeviceID == deviceID &&
                    svct.IsCore &&
                    svc.CancellationKeyDate <= keyDateNow
                    select svc).Any();
        }

        /// <summary>
        /// Checks whether Asset has ever been OnBoarded with another Device.
        /// </summary>
        /// <param name="opContext">NH_OP context</param>
        /// <param name="assetID">Asset to check</param>
        /// <returns>Asset has been previously onboarded?</returns>
        private static bool AssetHasHadOtherDevicesOrPreviousOwners(INH_OP opContext, long assetID)
        {
            return opContext.AssetDeviceHistoryReadOnly.Any(a => a.fk_AssetID == assetID);
        }

        public bool TerminateVisibility(long customerId, long assetId, long subscriptionId, DateTime endDate, INH_OP opContext)
        {
            var isTerminated = false;
            var serviceViews = (from sv in opContext.ServiceView
                                join s in opContext.ServiceReadOnly on  sv.fk_ServiceID equals s.ID
                                join st in opContext.ServiceTypeReadOnly on s.fk_ServiceTypeID equals st.ID
                                where sv.fk_CustomerID == customerId
                                && sv.fk_AssetID == assetId
                                && sv.fk_ServiceID == subscriptionId
                                select new ServiceViewWithCore {IsCore = st.IsCore, ServiceView = sv }).ToList();
            int endDateKey = endDate.KeyDate();
            var now = DateTime.UtcNow;
            foreach (var serviceView in serviceViews.Select(x=>x.ServiceView))
            {
                serviceView.EndKeyDate = endDateKey;
                serviceView.UpdateUTC = now;
            }

            var result = opContext.SaveChanges();

            if (result > 0)
            {
                isTerminated = true;
                if (EnableSubscriptionNextGenSync)
                {
                    foreach (var viewToTerminate in serviceViews.Select(x => x.ServiceView))
                    {
                        UpdateSubscription(opContext, viewToTerminate, isEndDate: true);
                    }
                }

                if (EnableCustomerNextGenSync)
                {
                    SyncDissociateCustomerAssetWithNextGen(endDate, serviceViews, opContext);
                }
            }

            return isTerminated;
        }

        public static void ResetServiceViewBlockSizeValue()
        {
            _serviceViewBlockSize = null;
            GetServiceViewBlockSizeValue();
        }

        public static int GetServiceViewBlockSizeValue()
        {
            if (!_serviceViewBlockSize.HasValue)
            {
                int blockSize;
                var serviceViewBlockSizeConfig = ConfigurationManager.AppSettings["ServiceViewBlockSize"];
                if (int.TryParse(serviceViewBlockSizeConfig, out blockSize))
                {
                    _serviceViewBlockSize = blockSize;
                    Log.IfInfoFormat("ServiceViewAPI.GetServiceViewBlockSizeValue: ServiceViewBlockSize is {0}.",
                        _serviceViewBlockSize);
                }
                else
                {
                    // default block size is 80
                    _serviceViewBlockSize = 80;
                    Log.IfInfoFormat(
                        "ServiceViewAPI.GetServiceViewBlockSizeValue: Could not parse ServiceViewBlockSize. Value set to default of {0}.",
                        _serviceViewBlockSize);
                }
            }

            Log.IfDebugFormat("ServiceViewAPI.GetServiceViewBlockSizeValue: Returning {0}", _serviceViewBlockSize.Value);
            return _serviceViewBlockSize.Value;
        }

        public void UpdateSubscription(INH_OP opContext, ServiceView sv, bool isStartDate = false, bool isEndDate = false)
        {
            var serviceTypeId = (from service in opContext.ServiceReadOnly
                                 join serviceType in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals serviceType.ID
                                 where service.ID == sv.fk_ServiceID
                                 select serviceType.ID).FirstOrDefault();

            if (MdmHelpers.ShouldSubscriptionSyncWithNextGen(sv.fk_CustomerID))
            {
                if (_assetSubscriptionList.Contains(serviceTypeId))
                {
                    List<Param> updatedFields = new List<Param>();
                    updatedFields.Add(new Param { Name = "SubscriptionUID", Value = sv.ServiceViewUID ?? Guid.Empty });
                    updatedFields.Add(new Param { Name = "ActionUTC", Value = DateTime.UtcNow });
                    if (isStartDate)
                    {
                        updatedFields.Add(new Param { Name = "StartDate", Value = sv.StartKeyDate.FromKeyDate() });
                    }
                    if (isEndDate)
                    {
                        updatedFields.Add(new Param { Name = "EndDate", Value = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1) });
                    }

                    var updatedEntries = updatedFields.ToDictionary(field => field.Name, field => field.Value);

                    var result = API.SubscriptionService.UpdateAssetSubscription(updatedEntries);
                    if (!result)
                    {
                        Log.IfInfoFormat("Error occurred while updating Asset Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                    }
                }
                if (_projectSubscriptionList.Contains(serviceTypeId))
                {
                    List<Param> updatedFields = new List<Param>();
                    updatedFields.Add(new Param { Name = "SubscriptionUID", Value = sv.ServiceViewUID ?? Guid.Empty });
                    updatedFields.Add(new Param { Name = "ActionUTC", Value = DateTime.UtcNow });
                    if (isStartDate)
                    {
                        updatedFields.Add(new Param { Name = "StartDate", Value = sv.StartKeyDate.FromKeyDate() });
                    }
                    if (isEndDate)
                    {
                        updatedFields.Add(new Param { Name = "EndDate", Value = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1) });
                    }

                    var updatedEntries = updatedFields.ToDictionary(field => field.Name, field => field.Value);

                    var result = API.SubscriptionService.UpdateProjectSubscription(updatedEntries);
                    if (!result)
                    {
                        Log.IfInfoFormat("Error occurred while updating Project Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                    }
                }

                if (serviceTypeId == (int)ServiceTypeEnum.Manual3DProjectMonitoring)
                {
                  List<Param> updatedFields = new List<Param>();
                  updatedFields.Add(new Param { Name = "SubscriptionUID", Value = sv.ServiceViewUID ?? Guid.Empty });
                  updatedFields.Add(new Param { Name = "ActionUTC", Value = DateTime.UtcNow });
                  if (isStartDate)
                  {
                    updatedFields.Add(new Param { Name = "StartDate", Value = sv.StartKeyDate.FromKeyDate() });
                  }
                  if (isEndDate)
                  {
                    updatedFields.Add(new Param { Name = "EndDate", Value = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1) });
                  }

                  var updatedEntries = updatedFields.ToDictionary(field => field.Name, field => field.Value);

                  var result = API.SubscriptionService.UpdateCustomerSubscription(updatedEntries);
                  if (!result)
                  {
                    Log.IfInfoFormat("Error occurred while updating Customer Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                  }
                }
            }
        }

        public void CreateSubscription(INH_OP opContext, ServiceView sv)
        {
            var serviceTypeObj = (from service in opContext.ServiceReadOnly
                                  join serviceType in opContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals serviceType.ID
                                  where service.ID == sv.fk_ServiceID
                                  select new { ServiceTypeId = serviceType.ID, ServiceTypeName = serviceType.Name, DeviceTypeID = service.fk_DeviceID }).FirstOrDefault();

            if (MdmHelpers.ShouldSubscriptionSyncWithNextGen(sv.fk_CustomerID))
            {
                if (_assetSubscriptionList.Contains(serviceTypeObj.ServiceTypeId))
                {
                    var createEvent = new
                    {
                        SubscriptionUID = sv.ServiceViewUID ?? Guid.Empty,
                        CustomerUID = (from customer in opContext.CustomerReadOnly where customer.ID == sv.fk_CustomerID select customer.CustomerUID ?? Guid.Empty).FirstOrDefault(),
                        AssetUID = (from asset in opContext.AssetReadOnly where asset.AssetID == sv.fk_AssetID select asset.AssetUID ?? Guid.Empty).FirstOrDefault(),
                        DeviceUID = (from device in opContext.Device where device.ID == serviceTypeObj.DeviceTypeID select device.DeviceUID ?? Guid.Empty).FirstOrDefault(),
                        SubscriptionType = serviceTypeObj.ServiceTypeName,
                        StartDate = sv.StartKeyDate.FromKeyDate(),
                        EndDate = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1),
                        ActionUTC = DateTime.UtcNow
                    };

                    var result = API.SubscriptionService.CreateAssetSubscription(createEvent);
                    if (!result)
                    {
                        Log.IfInfoFormat("Error occurred while creating Asset Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                    }
                }

                if (_projectSubscriptionList.Contains(serviceTypeObj.ServiceTypeId))
                {
                    var createEvent = new
                    {
                        SubscriptionUID = sv.ServiceViewUID ?? Guid.Empty,
                        CustomerUID = (from customer in opContext.CustomerReadOnly where customer.ID == sv.fk_CustomerID select customer.CustomerUID ?? Guid.Empty).FirstOrDefault(),
                        SubscriptionType = serviceTypeObj.ServiceTypeName,
                        StartDate = sv.StartKeyDate.FromKeyDate(),
                        EndDate = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1),
                        ActionUTC = DateTime.UtcNow
                    };

                    var result = API.SubscriptionService.CreateProjectSubscription(createEvent);
                    if (!result)
                    {
                        Log.IfInfoFormat("Error occurred while creating Project Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                    }
                }

                if (serviceTypeObj.ServiceTypeId == (int)ServiceTypeEnum.Manual3DProjectMonitoring)
                {
                  var createEvent = new
                  {
                    SubscriptionUID = sv.ServiceViewUID ?? Guid.Empty,
                    CustomerUID = (from customer in opContext.CustomerReadOnly where customer.ID == sv.fk_CustomerID select customer.CustomerUID ?? Guid.Empty).FirstOrDefault(),
                    SubscriptionType = serviceTypeObj.ServiceTypeName,
                    StartDate = sv.StartKeyDate.FromKeyDate(),
                    EndDate = sv.EndKeyDate.FromKeyDate().AddTicks(-1).AddDays(1),
                    ActionUTC = DateTime.UtcNow
                  };

                  var result = API.SubscriptionService.CreateCustomerSubscription(createEvent);
                  if (!result)
                  {
                    Log.IfInfoFormat("Error occurred while creating Customer Subscription in VSP stack. ServiceViewUID :{0}", sv.ServiceViewUID);
                  }
                }
            }
        }

        public void ReplaceDevice(INH_OP opContext, long oldDeviceID, long newDeviceID)
        {

            var oldDeviceGUID = (from device in opContext.DeviceReadOnly
                                 where device.ID == oldDeviceID
                                 select device.DeviceUID).FirstOrDefault();

            var newDeviceGUID = (from device in opContext.DeviceReadOnly
                                 where device.ID == newDeviceID
                                 select device.DeviceUID).FirstOrDefault();

            var AssetGUID = (from asset in opContext.AssetReadOnly
                             where asset.fk_DeviceID == newDeviceID
                             select asset.AssetUID).FirstOrDefault();

            //Replace device
            DeviceReplacementEvent replaceDevice = new DeviceReplacementEvent()
            {
                OldDeviceUID = (Guid)oldDeviceGUID,
                NewDeviceUID = (Guid)newDeviceGUID,
                AssetUID = (Guid)AssetGUID
            };
            var replaceSucess = API.DeviceService.ReplaceDevice(replaceDevice);
            if (!replaceSucess)
            {
                Log.IfInfoFormat("Error occurred while replace device in VSP stack. oldDeviceId :{0} - newDeviceId :{1}",
                  oldDeviceID, newDeviceID);
            }

        }
    }
}
