using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Models;
using Dapper;
using log4net;
using Newtonsoft.Json;
using VSS.Customer.Data.Interfaces;
using VSS.MasterData.Common.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Customer.Data
{
  public class MySqlCustomerRepository : RepositoryBase, ICustomerService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
  
    public int StoreCustomer(ICustomerEvent evt)
    {
      var upsertedCount = 0;

      if (evt is CreateCustomerEvent)
      {
        var customerEvent = (CreateCustomerEvent)evt;
        var customer = new Common.Models.Customer();
        customer.CustomerName = customerEvent.CustomerName;
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.CustomerType = (CustomerType) Enum.Parse(typeof (CustomerType), customerEvent.CustomerType, true);
        customer.LastActionedUTC = customerEvent.ActionUTC;        
        upsertedCount = UpsertCustomerDetail(customer, "CreateCustomerEvent");
      }
      else if (evt is UpdateCustomerEvent)
      {
        var customerEvent = (UpdateCustomerEvent)evt;
        var customer = new Common.Models.Customer();
        customer.CustomerName = customerEvent.CustomerName;
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = UpsertCustomerDetail(customer, "UpdateCustomerEvent");
      }
      else if (evt is DeleteCustomerEvent)
      {
        var customerEvent = (DeleteCustomerEvent)evt;
        var customer = new Common.Models.Customer();
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = UpsertCustomerDetail(customer, "DeleteCustomerEvent");
      }
      else if (evt is AssociateCustomerUserEvent)
      {
        var customerEvent = (AssociateCustomerUserEvent)evt;
        var customerUser = new CustomerUser();
        customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = UpsertCustomerUserDetail(customerUser, "AssociateCustomerUserEvent");
      }
      else if (evt is DissociateCustomerUserEvent)
      {
        var customerEvent = (DissociateCustomerUserEvent)evt;
        var customerUser = new CustomerUser();
        customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = UpsertCustomerUserDetail(customerUser, "DissociateCustomerUserEvent");
      }
   
      return upsertedCount;
    }

    /// <summary>
    /// All Customer detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private int UpsertCustomerDetail(Common.Models.Customer customer, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      var existing = Connection.Query<Common.Models.Customer>
        (@"SELECT 
                  CustomerUID, CustomerName, fk_CustomerTypeID AS CustomerType, LastActionedUTC
              FROM Customer
              WHERE CustomerUID = @CustomerUid", new { CustomerUid = customer.CustomerUID }).FirstOrDefault();

      if (eventType == "CreateCustomerEvent")
      {
        upsertedCount = CreateCustomer(customer, existing);
      }

      if (eventType == "UpdateCustomerEvent")
      {
        upsertedCount = UpdateCustomer(customer, existing);
      }

      if (eventType == "DeleteCustomerEvent")
      {
        upsertedCount = DeleteCustomer(customer, existing);
      }

      Log.DebugFormat("CustomerRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateCustomer(Common.Models.Customer customer, Common.Models.Customer existing)
    {
      if (existing == null)
      {
        Log.DebugFormat("CustomerRepository: going to create customer={0}", JsonConvert.SerializeObject(customer));

        const string insert =
          @"INSERT Customer
              (CustomerUID, CustomerName, fk_CustomerTypeID, LastActionedUTC)
              VALUES
              (@CustomerUID, @CustomerName, @CustomerType, @LastActionedUTC)";

        return Connection.Execute(insert, customer);
      }

      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customer.LastActionedUTC);

      return 0;
    }

    private int UpdateCustomer(Common.Models.Customer customer, Common.Models.Customer existing)
    {
      if (existing != null)
      {
        if (customer.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string update =
            @"UPDATE Customer                
                SET CustomerName = @CustomerName,
                    LastActionedUTC = @LastActionedUTC
                WHERE CustomerUID = @CustomerUID";
          return Connection.Execute(update, customer);
        }
        
        Log.DebugFormat("CustomerRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
          existing.LastActionedUTC, customer.LastActionedUTC);
      }
      else
      {
        Log.DebugFormat("CustomerRepository: can't update as none existing newActionedUTC={0}",
          customer.LastActionedUTC);
      }
      return 0;
    }

    private int DeleteCustomer(Common.Models.Customer customer, Common.Models.Customer existing)
    {
      if (existing != null)
      {
        if (customer.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE 
              FROM Customer                
              WHERE CustomerUID = @CustomerUID";
          return Connection.Execute(delete, customer);
        }
        
        Log.DebugFormat("CustomerRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
          existing.LastActionedUTC, customer.LastActionedUTC);
      }
      else
      {
        Log.DebugFormat("CustomerRepository: can't delete as none existing newActionedUT={0}",
          customer.LastActionedUTC);
      }
      return 0;
    }

    //public IEnumerable<Common.Models.Customer> GetAssociatedCustomerbyUserUid(System.Guid userUid)
    //{
    //  PerhapsOpenConnection();

    //  var customer = Connection.Query<Common.Models.Customer>
    //      (@"SELECT c.* 
    //        FROM Customer c JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
    //        WHERE cu.fk_UserUID = @userUid", new {userUid = userUid.ToString()});

    //  PerhapsCloseConnection();

    //  return customer;
    //}

    //public Common.Models.Customer GetCustomer(System.Guid customerUid)
    //{
    //  PerhapsOpenConnection();

    //  var customer = Connection.Query<Common.Models.Customer>
    //      (@"SELECT * 
    //         FROM Customer 
    //         WHERE CustomerUID = @customerUid", new { customerUid = customerUid.ToString() }).FirstOrDefault();

    //  PerhapsCloseConnection();

    //  return customer;
    //}

    /// <summary>
    /// All CustomerUser detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="customerUser"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private int UpsertCustomerUserDetail(CustomerUser customerUser, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("CustomerRepository: Upserting eventType={0} CustomerUid={1}, UserUid={2}",
        eventType, customerUser.CustomerUID, customerUser.UserUID);

      var existing = Connection.Query<CustomerUser>
        (@"SELECT 
            fk_UserUID AS UserUID, fk_CustomerUID AS CustomerUID, LastActionedUTC
              FROM CustomerUser
              WHERE fk_CustomerUID = @customerUID AND fk_UserUID = @userUID", new { customerUID = customerUser.CustomerUID, userUID = customerUser.UserUID }).FirstOrDefault();

      if (eventType == "AssociateCustomerUserEvent")
      {
        upsertedCount = AssociateCustomerUser(customerUser, existing);
      }

      if (eventType == "DissociateCustomerUserEvent")
      {
        upsertedCount = DissociateCustomerUser(customerUser, existing);
      }

      Log.DebugFormat("CustomerRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateCustomerUser(CustomerUser customerUser, CustomerUser existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT CustomerUser
            (fk_UserUID, fk_CustomerUID, LastActionedUTC)
            VALUES
            (@UserUID, @CustomerUID, @LastActionedUTC)";

        return Connection.Execute(insert, customerUser);
      }

      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customerUser.LastActionedUTC);
      return 0;
    }

    private int DissociateCustomerUser(CustomerUser customerUser, CustomerUser existing)
    {
      if (existing != null)
      {
        if (customerUser.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE 
              FROM CustomerUser               
              WHERE fk_CustomerUID = @CustomerUID AND fk_UserUID = @UserUID";
          return Connection.Execute(delete, customerUser);
        }

        Log.DebugFormat("CustomerRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
          existing.LastActionedUTC, customerUser.LastActionedUTC);
      }
      else
      {
        Log.DebugFormat("CustomerRepository: can't delete as none existing newActionedUTC {0}",
          customerUser.LastActionedUTC);
      }
      return 0;
    }


  }
}
