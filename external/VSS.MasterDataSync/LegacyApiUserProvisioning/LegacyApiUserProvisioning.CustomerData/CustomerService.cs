using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using log4net;
using LegacyApiUserProvisioning.CustomerData.Interfaces;
using VSS.Hosted.VLCommon;

namespace LegacyApiUserProvisioning.CustomerData
{
    public class CustomerService : ICustomerService
    {
        private readonly ILog _logger;
        private readonly INH_OP _nhOpContext;

        private static readonly string EngineeringOperationsCustomerUid =
            ConfigurationManager.AppSettings["EngineeringOperationsCustomerUid"];

        private static readonly string VssSupportCustomerUid =
            ConfigurationManager.AppSettings["VSS_SupportCustomerUid"];

        public CustomerService(ILog logger, INH_OP nhOpContext)
        {
            _logger = logger;
            _nhOpContext = nhOpContext;
        }

        //for inactive customers network customer code is referenced from the customer table directly since there is no account
        public IEnumerable<ICustomer> GetInActiveCustomers(string filter, int maxResults = 20)
        {
            _logger.IfDebugFormat(
                $"CustomerService.GetInActiveCustomers called with {filter} maxResult = {maxResults}");

            var customers = (from c in _nhOpContext.CustomerReadOnly
                    where !c.IsActivated
                          && (c.fk_CustomerTypeID == 0 || c.fk_CustomerTypeID == 1)
                          && c.CustomerUID.ToString() != EngineeringOperationsCustomerUid
                          && c.CustomerUID.ToString() != VssSupportCustomerUid
                          && (c.Name.Contains(filter)
                              || (c.fk_CustomerTypeID == 1 && c.NetworkCustomerCode.Contains(filter))
                              || (c.fk_CustomerTypeID == 0 && c.NetworkDealerCode.Contains(filter))
                              || (c.BSSID.Contains(filter))
                          )
                    select new Customer
                    {
                        CustomerUID = c.CustomerUID.Value,
                        BSSID = c.BSSID,
                        CustomerName = c.Name,
                        NetworkDealerCode = c.NetworkDealerCode,
                        NetworkCustomerCode = c.NetworkCustomerCode,
                        CustomerType = c.fk_CustomerTypeID.ToString()
                    }
                ).Distinct().Take(maxResults).ToList();
            _logger.IfDebugFormat($"CustomerService.GetInActiveCustomers found {customers.Count} customers");
            return customers;
        }

        //for active customers network customer code is referenced from account
        public IEnumerable<ICustomer> GetActiveCustomers(string filter, int maxResults = 20)
        {
            const string classMethod = "CustomerService.GetActiveCustomers";
            _logger.IfDebugFormat($"{classMethod} called with {filter} maxResult = {maxResults}");

            var customers = (from c in _nhOpContext.CustomerReadOnly
                    join cr in _nhOpContext.CustomerRelationshipReadOnly on c.ID equals cr.fk_ParentCustomerID into
                        crJoin
                    from crSub in crJoin.DefaultIfEmpty()
                    join acc in _nhOpContext.CustomerReadOnly on crSub.fk_ClientCustomerID equals acc.ID into
                        accountJoin
                    from accSub in accountJoin.DefaultIfEmpty()
                    where c.IsActivated
                          && (c.fk_CustomerTypeID == 0 || c.fk_CustomerTypeID == 1)
                          && c.CustomerUID.ToString() != EngineeringOperationsCustomerUid
                          && c.CustomerUID.ToString() != VssSupportCustomerUid
                          && (
                              c.Name.Contains(filter)
                              || c.BSSID.Contains(filter)
                              || (c.fk_CustomerTypeID == 0 && c.NetworkDealerCode.Contains(filter))
                              || (c.fk_CustomerTypeID == 1 && accSub.NetworkCustomerCode.Contains(filter))
                          )
                    select new Customer
                    {
                        CustomerUID = c.CustomerUID.Value,
                        BSSID = c.BSSID,
                        CustomerName = c.Name,
                        NetworkDealerCode = c.NetworkDealerCode,
                        NetworkCustomerCode = c.NetworkCustomerCode,
                        CustomerType = c.fk_CustomerTypeID.ToString(),
                    }
                ).Distinct().Take(maxResults).ToList();
            _logger.IfDebugFormat($"{classMethod} found {customers.Count} customers");
            return customers;
        }

        public IEnumerable<ICustomer> GetActiveInactiveCustomers(string filter, int maxResults = 20)
        {
            const string classMethod = "CustomerService.GetActiveInactiveCustomers";
            _logger.IfDebugFormat($"{classMethod} called with {filter} maxResult = {maxResults}");

            var customers = (from c in _nhOpContext.CustomerReadOnly
                    join cr in _nhOpContext.CustomerRelationshipReadOnly on c.ID equals cr.fk_ParentCustomerID into
                        crJoin
                    from crSub in crJoin.DefaultIfEmpty()
                    join acc in _nhOpContext.CustomerReadOnly on crSub.fk_ClientCustomerID equals acc.ID into
                        accountJoin
                    from accSub in accountJoin.DefaultIfEmpty()
                    where (c.fk_CustomerTypeID == 0 || c.fk_CustomerTypeID == 1)
                          && c.CustomerUID.ToString() != EngineeringOperationsCustomerUid
                          && c.CustomerUID.ToString() != VssSupportCustomerUid
                          && (
                              c.Name.Contains(filter)
                              || c.BSSID.Contains(filter)
                              || (c.fk_CustomerTypeID == 0 && c.NetworkDealerCode.Contains(filter))
                              || (c.fk_CustomerTypeID == 1 && c.IsActivated &&
                                  accSub.NetworkCustomerCode.Contains(filter))
                              || (c.fk_CustomerTypeID == 1 && !c.IsActivated && c.NetworkCustomerCode.Contains(filter))
                          )
                    select new Customer
                    {
                        CustomerUID = c.CustomerUID.Value,
                        BSSID = c.BSSID,
                        CustomerName = c.Name,
                        NetworkDealerCode = c.NetworkDealerCode,
                        NetworkCustomerCode = c.NetworkCustomerCode,
                        CustomerType = c.fk_CustomerTypeID.ToString(),
                    }
                ).Distinct().Take(maxResults).ToList();
            _logger.IfDebugFormat($"{classMethod} found {customers.Count} customers");
            return customers;
        }

        public IEnumerable<ICustomer> GetCustomersByBssid(string filter, bool active = true, bool exactMatch = true,
            int maxResults = 20)
        {
            if (exactMatch)
            {
                return (from c in _nhOpContext.CustomerReadOnly
                    where c.BSSID == filter && c.IsActivated == active && (c.fk_CustomerTypeID == 0 || c.fk_CustomerTypeID == 1)
                    select new Customer
                    {
                        CustomerUID = c.CustomerUID.Value,
                        BSSID = c.BSSID,
                        CustomerName = c.Name,
                        NetworkDealerCode = c.NetworkDealerCode,
                        NetworkCustomerCode = c.NetworkCustomerCode,
                        CustomerType = c.fk_CustomerTypeID.ToString(),
                    }).Distinct().Take(maxResults).ToList();
            }

            return (from c in _nhOpContext.CustomerReadOnly
                where c.BSSID.Contains(filter) && c.IsActivated == active && (c.fk_CustomerTypeID == 0 || c.fk_CustomerTypeID == 1)
                    select new Customer
                {
                    CustomerUID = c.CustomerUID.Value,
                    BSSID = c.BSSID,
                    CustomerName = c.Name,
                    NetworkDealerCode = c.NetworkDealerCode,
                    NetworkCustomerCode = c.NetworkCustomerCode,
                    CustomerType = c.fk_CustomerTypeID.ToString(),
                }).Distinct().Take(maxResults).ToList();
        }
    }
}