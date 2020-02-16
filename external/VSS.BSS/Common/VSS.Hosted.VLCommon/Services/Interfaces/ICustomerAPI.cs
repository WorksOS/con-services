using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{
  public interface ICustomerAPI
  {
    Customer CreateDealer(INH_OP ctx, string name, string bssID, string networkDealerCode, DealerNetworkEnum dealerNetworkType, string emailContact, string firstName, string lastName, long storeId = 1);
    Customer CreateCustomer(INH_OP ctx, string name, string bssID, string emailContact, string firstName, string lastName, long storeId = 1, string networkCustomerCode = null);
    Customer CreateAccount(INH_OP ctx, string name, string bssID, string dealerAccountCode, string networkCustomerCode, long storeId = 1);

    bool CreateCustomerRelationship(INH_OP dataContext, long parentCustomerID, long clientCustomerID, string BSSRelationshipID, CustomerRelationshipTypeEnum relationshipType);
    bool RemoveCustomerRelationship(INH_OP dataContext, long parentCustomerID, long clientCustomerID);
    bool UpdateCustomerRelationshipId(INH_OP dataContext, long parentCustomerId, long clientCustomerId, string relationshipId);

    bool UpdateCustomerNCC(INH_OP opContext, long parentCustomerID, long clientCustomerID);

    bool Deactivate(INH_OP dataContext, long customerID);
    bool Activate(INH_OP dataContext, long customerID);    
    
    bool Update(INH_OP dataContext, long customerID, List<Param> modifiedProperties);

    string GetDeviceOwnerCustomerName(INH_OP ctx, long assetID);

    string GetAssetAccountOrDealerName(INH_OP ctx, long assetID);
    string GetCustomerNCC(INH_OP opCtx, long customerId);

    bool IsAssetViewableByCustomer(string gpsDeviceId, long customerId);

    //Utility Methods    
    long GetTrimbleOperationsCustomerID();
    
    Customer GetCustomerDetails(string bssID);
    List<Customer> GetCustomerList();
    List<User> GetApiUserList(long customerID);
    List<CustomerRelationship> GetCustomerRelationship(long clientCusID);
    User GetUser(long userID);
    Customer GetCustomer(long cusID);
  }
}
