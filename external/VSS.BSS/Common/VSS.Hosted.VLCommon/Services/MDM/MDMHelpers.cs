using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using log4net;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace VSS.Hosted.VLCommon.Services.MDM
{
    public class MdmHelpers
    {

        #region customersExcludedFromNextGen

        private static readonly List<string> customersToBeExcluded = ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"] != null ? ConfigurationManager.AppSettings["CustomersToBeExcludedFromNextGenSync"].Split('$').Select(s => s.Trim()).ToList() : new List<string>();
        private static readonly List<int> CustomerTypesToBeIncluded = new List<int>() { (int)CustomerTypeEnum.Customer, (int)CustomerTypeEnum.Dealer };

        public static bool ShouldSubscriptionSyncWithNextGen(long customerID)
        {
            INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>();

            var customer = (from cust in ctx.CustomerReadOnly
                            where cust.ID == customerID && !(customersToBeExcluded.Contains(cust.Name) && (cust.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer)) && CustomerTypesToBeIncluded.Contains(cust.fk_CustomerTypeID)
                            select cust);

            return customer.Count() > 0;

        }

        #endregion customersExcludedFromNextGen


        #region customerAsset
        public static void SyncAssociateCustomerAssetWithNextGen(INH_OP ctx, ICustomerService customerService, Guid customerUID, Guid assetUID, string relationType)
        {
            var customerId = ctx.CustomerReadOnly.Where(x => x.CustomerUID == customerUID)
                              .Select(x => x.ID)
                              .FirstOrDefault();

            if (ShouldSubscriptionSyncWithNextGen(customerId))
            {

                var associateCustomerAsset = new AssociateCustomerAssetEvent()
                {
                    CustomerUID = customerUID,
                    AssetUID = assetUID,
                    ActionUTC = DateTime.UtcNow,
                    RelationType = relationType
                };

                customerService.AssociateCustomerAsset(associateCustomerAsset);
            }
        }

        public static void SyncDissociateCustomerAssetWithNextGen(INH_OP ctx, ICustomerService customerService, DateTime actionUtc, Guid assetGuid, Guid customerGuid, log4net.ILog log)
        {
            var customerId = ctx.CustomerReadOnly.Where(x => x.CustomerUID == customerGuid)
                              .Select(x => x.ID)
                              .FirstOrDefault();

            if (ShouldSubscriptionSyncWithNextGen(customerId))
            {
                var dissociateCustomer = new DissociateCustomerAssetEvent()
                {
                    ActionUTC = actionUtc,
                    AssetUID = assetGuid,
                    CustomerUID = customerGuid
                };

                var success = customerService.DissociateCustomerAsset(dissociateCustomer);
                if (!success)
                {
                    log.IfWarnFormat("Error occurred while dissociating customer {0} and asset {1} in vsp stack", customerGuid, assetGuid);
                }
            }
        }
        public static RelationType GetRelationTypeForCustomerType(CustomerTypeEnum customerTypeEnum)
        {
            RelationType relationType;
            switch (customerTypeEnum)
            {
                case CustomerTypeEnum.Customer:
                    relationType = RelationType.Customer;
                    break;
                case CustomerTypeEnum.Dealer:
                    relationType = RelationType.Dealer;
                    break;
                case CustomerTypeEnum.Corporate:
                    relationType = RelationType.Corporate;
                    break;
                case CustomerTypeEnum.Operations:
                    relationType = RelationType.Operations;
                    break;
                default:
                    relationType = RelationType.Owner; //AB - shouldn't realistically get here..since we're going to push Owner as a seperate property in the future into NextGen
                    break;
            }
            return relationType;
        }
        #endregion
        #region customerUser

        public static void SyncAssociateCustomerUserWithNextGen(ICustomerService customerService, Guid? customerUID, User newUser)
        {


            var associateEvent = new AssociateCustomerUserEvent()
            {
                CustomerUID = customerUID.Value,
                UserUID = new Guid(newUser.UserUID),
                ActionUTC = DateTime.UtcNow
            };
            customerService.AssociateCustomerUser(associateEvent);

        }

        public static void SyncDissociateCustomerUserWithNextGen(ICustomerService customerService,  User user, INH_OP opContext)
        {
            if (!user.fk_CustomerID.HasValue || string.IsNullOrEmpty(user.UserUID))
                return;

            int userCountWithSameUid = (from usr in opContext.UserReadOnly
                                        join cust in opContext.CustomerReadOnly on usr.fk_CustomerID equals cust.ID
                                        where usr.fk_CustomerID == user.fk_CustomerID
                                        && usr.Active && usr.UserUID == user.UserUID && usr.ID != user.ID
                                        select 1).Count();



            var customerUid =
                     opContext.CustomerReadOnly.Where(x => x.ID == user.fk_CustomerID.Value)
                       .Select(x => x.CustomerUID)
                       .First();


            if (userCountWithSameUid == 0)
            {
                if (!customerUid.HasValue)
                    return;
                var dissociateEvent = new DissociateCustomerUserEvent()
                {
                    CustomerUID = customerUid.Value,
                    UserUID = new Guid(user.UserUID),
                    ActionUTC = DateTime.UtcNow,
                };

                customerService.DissociateCustomerUser(dissociateEvent);
            }
        }
        #endregion

        #region customer
        public static void CreateCustomerInNextGen(INH_OP ctx,ICustomerService customerService, Customer newCustomer, log4net.ILog log)
        {
            var createCustomer = new
            {
                CustomerName = newCustomer.Name,
                ActionUTC = newCustomer.UpdateUTC,
                BSSID = newCustomer.BSSID,
                CustomerType = (from customerType in ctx.CustomerTypeReadOnly where customerType.ID == newCustomer.fk_CustomerTypeID select customerType.Name).FirstOrDefault(),
                NetworkDealerCode = newCustomer.NetworkDealerCode,
                DealerNetwork = (from dealerNetwork in ctx.DealerNetworkReadOnly where dealerNetwork.ID == newCustomer.fk_DealerNetworkID select dealerNetwork.Name).FirstOrDefault(),
                CustomerUID = newCustomer.CustomerUID.Value,
            };

            var success = customerService.Create(createCustomer);
            if (!success)
            {
                log.IfWarnFormat("Error occurred while creating {1} in VSP stack. Customer name :{0}", newCustomer.Name, ((CustomerTypeEnum)newCustomer.fk_CustomerTypeID).ToString());
            }
        }
        #endregion


    }
}
