
using System;
using System.Collections.Generic;
using VSP.MasterData.Customer.Data.Models;
namespace VSP.MasterData.Customer.Data
{
    public interface ICustomerDataService
    {
        void CreateCustomer(CreateCustomerEvent createCustomerEvent);
        int UpdateCustomer(UpdateCustomerEvent updateCustomerEvent);
        void DeleteCustomer(DeleteCustomerEvent deleteCustomerEvent);
        bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUserEvent);
        void DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUserEvent);
        List<Models.Customer> GetAssociatedCustomerbyUserUid(Guid UserUID);
        Models.Customer GetCustomer(Guid CustomerUID);
    }
}
