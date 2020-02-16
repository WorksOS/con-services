using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Linq;
using VSS.Hosted.VLCommon.Services.MDM;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Hosted.VLCommon
{
  public class CustomerAPI : ICustomerAPI
  {
		private static readonly ILog Log =
			LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private static readonly string _TrimbleOperations = "Trimble Operations"; 
    private static readonly string _DeletedCustomerNamePrefix = "_DELETED_";
    private static readonly List<int> featureTypes = new List<int> {  (int)FeatureEnum.FarmWorksService, (int)FeatureEnum.AEMPService,(int)FeatureEnum.StartStopService, 
                                                                      (int)FeatureEnum.FenceAlertService,(int)FeatureEnum.FuelService,(int)FeatureEnum.EventService, (int)FeatureEnum.DiagnosticService,(int)FeatureEnum.EngineParametersService,
                                                                      (int)FeatureEnum.DigitalSwitchStatusService, (int)FeatureEnum.SecurityService,(int)FeatureEnum.VLReadyAPI,
                                                                      (int)FeatureEnum.SMULocationService, (int)FeatureEnum.RFIDData, (int)FeatureEnum.VLReadyAPI};
		private static readonly bool EnableNextGenSync =
			Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.CustomerAPI.EnableSync"]);

    private long? trimbleOperationsCustomerID = null;
    private Object dataSyncObject = new Object();
    private UUIDSequentialGuid customerUUID = new UUIDSequentialGuid();
    private readonly ICustomerService _customerServiceApi;

    public CustomerAPI()
	  {
		  _customerServiceApi = API.CustomerService;
	  }
    public CustomerAPI(ICustomerService customerService)
      : this()
		{
			_customerServiceApi = customerService;
		}

    public Customer CreateDealer(INH_OP ctx, string name, string bssID, string networkDealerCode, DealerNetworkEnum dealerNetworkType, string emailContact, string firstName, string lastName, long storeId = 1)
    {
      Customer newCustomer = new Customer { Name = name, UpdateUTC = DateTime.UtcNow, IsActivated = true };
      newCustomer.BSSID = bssID;
      newCustomer.fk_CustomerTypeID = (int)CustomerTypeEnum.Dealer;
      newCustomer.NetworkDealerCode = networkDealerCode;
      newCustomer.fk_DealerNetworkID = (int)dealerNetworkType;
      newCustomer.CustomerUID = customerUUID.CreateGuid();
      newCustomer.PrimaryEmailContact = emailContact;
      newCustomer.FirstName = firstName;
      newCustomer.LastName = lastName;

      ctx.Customer.AddObject(newCustomer);
      AddToCustomerStore(ctx, newCustomer.ID, storeId);
      int result = ctx.SaveChanges();

			if (EnableNextGenSync)
			{
				MdmHelpers.CreateCustomerInNextGen(ctx,_customerServiceApi ,newCustomer, Log);
			}

      if (result <= 0)
        throw new InvalidOperationException("Failed to save Customer");

      return newCustomer;
    }

    public Customer CreateCustomer(INH_OP ctx, string name, string bssID, string emailContact, string firstName, string lastName, long storeId = 1, string networkCustomerCode=null)
    {
      Customer newCustomer = new Customer { Name = name, UpdateUTC = DateTime.UtcNow, IsActivated = true };
      newCustomer.BSSID = bssID;
      newCustomer.fk_CustomerTypeID = (int)CustomerTypeEnum.Customer;
      newCustomer.fk_DealerNetworkID = (int)DealerNetworkEnum.None;
      newCustomer.NetworkCustomerCode = networkCustomerCode;
      newCustomer.PrimaryEmailContact = emailContact;
      newCustomer.FirstName = firstName;
      newCustomer.LastName = lastName;

      newCustomer.CustomerUID = customerUUID.CreateGuid();
      ctx.Customer.AddObject(newCustomer);
      AddToCustomerStore(ctx, newCustomer.ID, storeId);
      int result = ctx.SaveChanges();

			if (EnableNextGenSync)
			{
        MdmHelpers.CreateCustomerInNextGen(ctx,_customerServiceApi, newCustomer, Log);
			}
			
      if (result <= 0)
        throw new InvalidOperationException("Failed to save Customer");

      return newCustomer;
    }

	  

	  /// <summary>
    /// Create user with CustomerType of Account
    /// Customer type is CustomerTypeEnum.Account
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="name">Customer Name</param>
    /// <param name="bssID"></param>
    /// <param name="dealerAccountCode"></param>
    /// <param name="networkCustomerCode"></param>
    /// <returns>Newly Created Customer</returns>
    public Customer CreateAccount(INH_OP ctx, string name, string bssID, string dealerAccountCode, string networkCustomerCode, long storeId = 1)
    {
      Customer newCustomer = new Customer { Name = name, UpdateUTC = DateTime.UtcNow, IsActivated = true };
      newCustomer.BSSID = bssID;
      newCustomer.fk_CustomerTypeID = (int)CustomerTypeEnum.Account;
      newCustomer.DealerAccountCode = dealerAccountCode;
      newCustomer.NetworkCustomerCode = networkCustomerCode;
      newCustomer.fk_DealerNetworkID = (int)DealerNetworkEnum.None;
      newCustomer.CustomerUID = customerUUID.CreateGuid();
      ctx.Customer.AddObject(newCustomer);
      AddToCustomerStore(ctx, newCustomer.ID, storeId);
      int result = ctx.SaveChanges();

			if (EnableNextGenSync)
			{
        MdmHelpers.CreateCustomerInNextGen(ctx,_customerServiceApi, newCustomer, Log);
			}

      if (result <= 0)
        throw new InvalidOperationException("Failed to save Customer");

      return newCustomer;
    }

    private void AddToCustomerStore(INH_OP ctx, long customerId, long storeId)
    {
      CustomerStore customerStore = new CustomerStore { fk_CustomerID = customerId, fk_StoreID = storeId };
      ctx.CustomerStore.AddObject(customerStore);
    }

    /// <summary>
    /// Sets isActive flag to true on Customer.
    /// </summary>
    /// <param name="session">SessionContext</param>
    /// <param name="customerID">Customer ID to set isActived flag</param>
    /// <returns>Save successful?</returns>
    public bool Activate(INH_OP dataContext, long customerID)
    {
      return ChangeCustomerActivation(dataContext, customerID);
    }

    /// <summary>
    /// Sets isActive flag to false on Customer.
    /// </summary>
    /// <param name="session">SessionContext</param>
    /// <param name="customerID">Customer ID to set isActived flag</param>
    /// <returns>Save successful?</returns>
    public bool Deactivate(INH_OP dataContext, long customerID)
    {
      return ChangeCustomerActivation(dataContext, customerID, false);
    }

    public bool CreateCustomerRelationship(INH_OP dataContext, long parentCustomerID, long clientCustomerID, string relationshipID, CustomerRelationshipTypeEnum relationshipType)
    {
      CustomerRelationship customerRelationship = new CustomerRelationship { fk_ParentCustomerID = parentCustomerID, fk_ClientCustomerID = clientCustomerID, fk_CustomerRelationshipTypeID = (int)relationshipType };
      customerRelationship.BSSRelationshipID = relationshipID;
      dataContext.CustomerRelationship.AddObject(customerRelationship);

      int result = dataContext.SaveChanges();

      if (result < 1)
        return false;

      return true;
    }

    public bool UpdateCustomerNCC(INH_OP opContext, long parentCustomerID, long clientCustomerID)
    {
      var updated = false;
      var custNCC = GetCustomerNCC(opContext, parentCustomerID);



      if (custNCC != null)
      {
        var account = (from a in opContext.Customer
                       where a.ID == clientCustomerID
                       select a).FirstOrDefault();
        account.NetworkCustomerCode = custNCC;
        account.UpdateUTC = DateTime.UtcNow;
        var result = opContext.SaveChanges();
        if (result > 0)
          updated = true;
      }
      return updated;
    }

    public string GetCustomerNCC(INH_OP opCtx, long customerId)
    {
      return (from c in opCtx.CustomerReadOnly
              where c.ID == customerId
              select c.NetworkCustomerCode
             ).FirstOrDefault();
    }

    public bool RemoveCustomerRelationship(INH_OP dataContext, long parentCustomerID, long clientCustomerID)
    {
      bool success = false;
      var customer = (from c in dataContext.CustomerRelationship
                      where c.fk_ParentCustomerID == parentCustomerID &&
                            c.fk_ClientCustomerID == clientCustomerID
                      select c).FirstOrDefault();

      dataContext.CustomerRelationship.DeleteObject(customer);

      int result = dataContext.SaveChanges();

      if (result > 0)
        success = true;

      return success;
    }

    public bool UpdateCustomerRelationshipId(INH_OP dataContext, long parentCustomerId, long clientCustomerId, string relationshipId)
    {
      var success = false;
      var customerRelationship = (from c in dataContext.CustomerRelationship
                                  where c.fk_ParentCustomerID == parentCustomerId
                                     && c.fk_ClientCustomerID == clientCustomerId
                                  select c).FirstOrDefault();

      if (customerRelationship == null)
        throw new InvalidOperationException("Failed to find customer relationship");

      customerRelationship.BSSRelationshipID = relationshipId;
      var result = dataContext.SaveChanges();

      if (result > 0)
        success = true;

      return success;
    }

    public bool Update(INH_OP dataContext, long customerID, List<Param> modifiedProperties)
    {
      //Prevent 'Trimble Op' customer name changing
      var nameParam = (from m in modifiedProperties
                       where m.Name == "Name"
                       select m).FirstOrDefault();
      if (nameParam != null)
      {
        if (customerID == GetTrimbleOperationsCustomerID())
          throw new InvalidOperationException("Trimble Operations customer cannot be renamed", new IntentionallyThrownException());
      }
      Customer c = (from cc in dataContext.Customer where cc.ID == customerID select cc).Single();
      bool success = API.Update<Customer>(dataContext, c, modifiedProperties) != null;
	    if (success && EnableNextGenSync)
	    {
            var nextGenUpdateFields = new List<string>
				{
					"Name",
					"UpdateUTC",
					"BSSID",
					"fk_DealerNetworkID",
					"NetworkDealerCode",
					"NetworkCustomerCode",
					"DealerAccountCode",
					"CustomerUID"
				};
				var updatedEntries = modifiedProperties.Where(x => nextGenUpdateFields.Contains(x.Name)).ToDictionary(y => y.Name, y => y.Value);
                if (updatedEntries.ContainsKey("Name"))
                {
                    var value = updatedEntries["Name"].ToString();
                    updatedEntries.Remove("Name");
                    updatedEntries.Add("CustomerName", value);
                }
                if (updatedEntries.ContainsKey("UpdateUTC"))
                {
                    var value = Convert.ToDateTime(updatedEntries["UpdateUTC"]);
                    updatedEntries.Remove("UpdateUTC");
                    updatedEntries.Add("ActionUTC", value);
                }
                
                if (updatedEntries.ContainsKey("fk_DealerNetworkID"))
                {
                    var value = (int)updatedEntries["fk_DealerNetworkID"];
                    var dealerNetworkValue = (from dealerNetwork in dataContext.DealerNetworkReadOnly where dealerNetwork.ID == value select dealerNetwork.Name).FirstOrDefault();
                    updatedEntries.Remove("fk_DealerNetworkID");
                    updatedEntries.Add("DealerNetwork", dealerNetworkValue);
                }


                _customerServiceApi.Update(updatedEntries);
				
	    }
	    return success;
    }

    public bool Delete(SessionContext session, long customerID)
    {    
      bool deleted = false;
      
      Customer customer = (from cc in session.NHOpContext.Customer where cc.ID == (long)customerID select cc).SingleOrDefault();
      if (customer != null)
      {
        customer.IsActivated = false;
        customer.UpdateUTC = DateTime.UtcNow;
        deleted = (session.NHOpContext.SaveChanges() != 0);
				if (deleted && EnableNextGenSync && customer.CustomerUID.HasValue)
				{
					_customerServiceApi.Delete(customer.CustomerUID.Value, DateTime.UtcNow);
				}
      }
      return deleted;
    }

    public string GetDeviceOwnerCustomerName(INH_OP ctx, long assetID)
    {      
      return (from a in ctx.AssetReadOnly
              join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
              join c in ctx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
              where a.AssetID == assetID
              select (c.fk_CustomerTypeID == (int)CustomerTypeEnum.Account || c.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer) ? c.Name : "na").SingleOrDefault();
    }


    private bool ChangeCustomerActivation(INH_OP dataContext, long customerID, bool isActive = true)
    {
      var activated = false;
      
      var customer = (from c in dataContext.Customer
                      where c.ID == customerID
                      select c
                      ).FirstOrDefault();
      customer.IsActivated = isActive;
      customer.UpdateUTC = DateTime.UtcNow;
      var result = dataContext.SaveChanges();

      if (result > 0)
        activated = true;
      

      return activated;
    }

    public string GetAssetAccountOrDealerName(INH_OP ctx, long assetID)
    {
      return (from a in ctx.AssetReadOnly
              join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
              join c in ctx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
              where a.AssetID == assetID
              select ((c.fk_CustomerTypeID == (int)CustomerTypeEnum.Account) || (c.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer)) ? c.Name : "-").SingleOrDefault();
    }

		#region private

		

    //TODO: create a wrapper expression that maps an expression taking a generic interface param to cast to concrete param so we don't have to duplicate queries
    private static readonly Func<NH_OP, string, long, Customer> _assetViewableByCustomerCompiledQuery =
      CompiledQuery.Compile<NH_OP, string, long, Customer>((opCtx, gpsDeviceId, customerId) =>
                    (from customer in opCtx.CustomerReadOnly
                     where customer.ID == customerId
                     join dealerNetwork in opCtx.DealerNetworkReadOnly on customer.fk_DealerNetworkID equals dealerNetwork.ID
                     let isTNL = (dealerNetwork.ID == (int)DealerNetworkEnum.THC || dealerNetwork.ID == (int)DealerNetworkEnum.DOOSAN)
                     let isCNH = (dealerNetwork.ID == (int)DealerNetworkEnum.CASE || dealerNetwork.ID == (int)DealerNetworkEnum.NEWHOLLAND)
                     from device in opCtx.DeviceReadOnly
                     where device.GpsDeviceID.Equals(gpsDeviceId)
                     join ownerOfDevice in opCtx.CustomerReadOnly on device.OwnerBSSID equals ownerOfDevice.BSSID
                     where
                    (isTNL ? ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.THC || ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.DOOSAN :
                      isCNH ? ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.CASE || ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.NEWHOLLAND :
                        ownerOfDevice.fk_DealerNetworkID == dealerNetwork.ID)
                     select customer).FirstOrDefault());
	#endregion private
    public bool IsAssetViewableByCustomer(string gpsDeviceId, long customerId)
    {
      
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Customer cust = null;

        if (opCtx is NH_OP)
        {
          cust = _assetViewableByCustomerCompiledQuery.Invoke(opCtx as NH_OP, gpsDeviceId, customerId);
        }
        else
        {
          cust = (from customer in opCtx.CustomerReadOnly
                  where customer.ID == customerId
                  join dealerNetwork in opCtx.DealerNetworkReadOnly on customer.fk_DealerNetworkID equals dealerNetwork.ID
                  let isTNL = (dealerNetwork.ID == (int)DealerNetworkEnum.THC || dealerNetwork.ID == (int)DealerNetworkEnum.DOOSAN)
                  let isCNH = (dealerNetwork.ID == (int)DealerNetworkEnum.CASE || dealerNetwork.ID == (int)DealerNetworkEnum.NEWHOLLAND)
                  from device in opCtx.DeviceReadOnly
                  where device.GpsDeviceID.Equals(gpsDeviceId)
                  join ownerOfDevice in opCtx.CustomerReadOnly on device.OwnerBSSID equals ownerOfDevice.BSSID
                  where
                      (isTNL ? ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.THC || ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.DOOSAN :
                        isCNH ? ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.CASE || ownerOfDevice.fk_DealerNetworkID == (int)DealerNetworkEnum.NEWHOLLAND :
                          ownerOfDevice.fk_DealerNetworkID == dealerNetwork.ID)
                  select customer).FirstOrDefault();
        }

        if (cust != null)
          return true;
        else if (customerId == GetTrimbleOperationsCustomerID())
          return true;
        return false;
      }
    }


    #region VLTier1Support

    public Customer GetCustomerDetails(string bssID)
    {
      using (INH_OP opCtx  = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return (from cus in opCtx.Customer
                where cus.BSSID == bssID
                select cus).FirstOrDefault();
      }
    }

    public List<Customer> GetCustomerList()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        List<int> customerTypes = new List<int> { (int)CustomerTypeEnum.Dealer, (int)CustomerTypeEnum.Customer };

        return ((from c in opCtx.CustomerReadOnly
                 where c.IsActivated && customerTypes.Contains(c.fk_CustomerTypeID)
                 select c).ToList());
      }
    }

    public List<User> GetApiUserList(long customerID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return ((from u in opCtx.UserReadOnly
                 join uf in opCtx.UserFeatureReadOnly on u.ID equals uf.fk_User
                 where u.Active && u.fk_CustomerID == customerID && featureTypes.Contains(uf.fk_Feature)
                 select u).Distinct().ToList());
      }
    }

    public List<CustomerRelationship> GetCustomerRelationship(long clientCusID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return (from cr in opCtx.CustomerRelationshipReadOnly
                where cr.fk_ClientCustomerID == clientCusID
                select cr).ToList();
      }
    }

    public Customer GetCustomer(long cusID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return (from cus in opCtx.Customer
                where cus.ID == cusID
                select cus).FirstOrDefault();
      }
    }

    public User GetUser(long userID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return (from user in opCtx.User
                where user.ID == userID
                select user).FirstOrDefault();
      }
    }

    #endregion

    
    #region Utilities


    public string GetDeletedCustomerPrefix()
    {
      return _DeletedCustomerNamePrefix;
    }

    public long GetTrimbleOperationsCustomerID()
    {
      lock (dataSyncObject)
      {
        if (!trimbleOperationsCustomerID.HasValue)
        {
          using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
          {
            trimbleOperationsCustomerID = (from c in ctx.CustomerReadOnly
                                           where c.fk_CustomerTypeID == (int)CustomerTypeEnum.Operations && c.Name == _TrimbleOperations
                                           select c.ID).Single();
          }
        }
      }
      return trimbleOperationsCustomerID.Value;
    }

    #endregion

    
  }
}
