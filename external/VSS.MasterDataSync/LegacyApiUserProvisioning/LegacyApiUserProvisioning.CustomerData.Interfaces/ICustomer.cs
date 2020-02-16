using System;

namespace LegacyApiUserProvisioning.CustomerData.Interfaces
{
    public interface ICustomer
    {
        Guid CustomerUID { get; set; }
        string CustomerName { get; set; }
        string CustomerType { get; set; }
        string NetworkCustomerCode { get; set; }
        string NetworkDealerCode { get; set; }
        string BSSID { get; set; }
    }
}