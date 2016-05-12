using System;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using Newtonsoft.Json;
using VSS.Customer.Data.Interfaces;
using VSS.Landfill.Common.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Customer.Data
{
  public class MySqlCustomerRepository : RepositoryBase, ICustomerService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    //private readonly string _connectionString;

    //public MySqlCustomerRepository()
    //{
    //  _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    //}

    public int StoreCustomer(ICustomerEvent evt)
    {
      var upsertedCount = 0;
      var customer = new Models.Customer();
      string eventType = "Unknown";

      if (evt is CreateCustomerEvent)
      {
        var customerEvent = (CreateCustomerEvent)evt;
        customer.CustomerName = customerEvent.CustomerName;
        customer.CustomerUid = customerEvent.CustomerUID.ToString();
        customer.CustomerType = (CustomerType) Enum.Parse(typeof (CustomerType), customerEvent.CustomerType, true);
        customer.LastActionedUtc = customerEvent.ActionUTC;        

        eventType = "CreateCustomerEvent";
      }
      else if (evt is UpdateCustomerEvent)
      {
        var customerEvent = (UpdateCustomerEvent)evt;
        customer.CustomerName = customerEvent.CustomerName;
        customer.CustomerUid = customerEvent.CustomerUID.ToString();
        customer.LastActionedUtc = customerEvent.ActionUTC;
        
        eventType = "UpdateCustomerEvent";
      }
      else if (evt is DeleteCustomerEvent)
      {
        var customerEvent = (DeleteCustomerEvent)evt;
        customer.CustomerUid = customerEvent.CustomerUID.ToString();
        customer.LastActionedUtc = customerEvent.ActionUTC;

        eventType = "DeleteCustomerEvent";
      }

      upsertedCount = UpsertCustomerDetail(customer, eventType);
      
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
    private int UpsertCustomerDetail(Models.Customer customer, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      var existing = Connection.Query<Models.Customer>
        (@"SELECT 
                  CustomerUID AS CustomerUid, CustomerName, fk_CustomerTypeID AS CustomerType, LastActionedUTC AS LastActionedUtc
              FROM Customer
              WHERE CustomerUID = @CustomerUid", new { customer.CustomerUid }).FirstOrDefault();

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

    public int CreateCustomer(Models.Customer customer, Models.Customer existing)
    {
      if (existing == null)
      {
        Log.DebugFormat("CustomerRepository: going to create customer={0}", JsonConvert.SerializeObject(customer));

        const string insert =
          @"INSERT Customer
              (CustomerUID, CustomerName, fk_CustomerTypeID, LastActionedUTC)
              VALUES
              (@CustomerUid, @CustomerName, @CustomerType, @LastActionedUtc)";

        return Connection.Execute(insert, customer);
      }

      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customer.LastActionedUtc);

      return 0;
    }

    public int UpdateCustomer(Models.Customer customer, Models.Customer existing)
    {
      if (existing != null)
      {
        if (customer.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
            @"UPDATE Customer                
                SET CustomerName = @CustomerName,
                    LastActionedUTC = @LastActionedUtc
                WHERE CustomerUID = @CustomerUid";
          return Connection.Execute(update, customer);
        }
        
        Log.DebugFormat("CustomerRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
          existing.LastActionedUtc, customer.LastActionedUtc);
      }
      else
      {
        Log.DebugFormat("CustomerRepository: can't update as none existing newActionedUTC={0}",
          customer.LastActionedUtc);
      }
      return 0;
    }

    public int DeleteCustomer(Models.Customer customer, Models.Customer existing)
    {
      if (existing != null)
      {
        if (customer.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string delete =
            @"DELETE 
              FROM Customer                
              WHERE CustomerUID = @CustomerUid";
          return Connection.Execute(delete, customer);
        }
        
        Log.DebugFormat("CustomerRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
          existing.LastActionedUtc, customer.LastActionedUtc);
      }
      else
      {
        Log.DebugFormat("CustomerRepository: can't delete as none existing newActionedUT={0}",
          customer.LastActionedUtc);
      }
      return 0;
    }

    public Models.Customer GetAssociatedCustomerbyUserUid(System.Guid userUid)
    {
      PerhapsOpenConnection();

      var customer = Connection.Query<Models.Customer>
          (@"SELECT c.* 
            FROM Customer c JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
            WHERE cu.fk_UserUID = @userUid", new {userUid = userUid.ToString()}).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }

    public Models.Customer GetCustomer(System.Guid customerUid)
    {
      PerhapsOpenConnection();

      var customer = Connection.Query<Models.Customer>
          (@"SELECT * 
             FROM Customer 
             WHERE CustomerUID = @customerUid", new { customerUid = customerUid.ToString() }).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }

    public int StoreCustomerUser(ICustomerUserEvent evt)
    {
      var upsertedCount = 0;
      var customerUser = new Models.CustomerUser();
      string eventType = "Unknown";

      if (evt is AssociateCustomerUserEvent)
      {
        var customerEvent = (AssociateCustomerUserEvent)evt;
        customerUser.fk_CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.fk_UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;

        eventType = "AssociateCustomerUserEvent";
      }
      else if (evt is DissociateCustomerUserEvent)
      {
        var customerEvent = (DissociateCustomerUserEvent)evt;
        customerUser.fk_CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.fk_UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;

        eventType = "DissociateCustomerUserEvent";
      }

      upsertedCount = UpsertCustomerUserDetail(customerUser, eventType);

      return upsertedCount;
    }

    /// <summary>
    /// All CustomerUser detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="customerUser"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private int UpsertCustomerUserDetail(Models.CustomerUser customerUser, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("CustomerRepository: Upserting eventType{0} CustomerUid={1}, UserUID={2}",
        eventType, customerUser.fk_CustomerUID, customerUser.fk_UserUID);

      var existing = Connection.Query<Models.CustomerUser>
        (@"SELECT 
            fk_UserUID, fk_CustomerUID, LastActionedUTC
              FROM CustomerUser
              WHERE fk_CustomerUID = @customerUID AND fk_UserUID = @userUID", new { customerUID = customerUser.fk_CustomerUID, userUID = customerUser.fk_UserUID }).FirstOrDefault();

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

    private int AssociateCustomerUser(Models.CustomerUser customerUser, Models.CustomerUser existing)
    {
      if (existing == null)
      {
        //TODO: May need to dummy this like projects and customers due to out of order events

        var customer = Connection.Query<VSS.Customer.Data.Models.Customer>
          (@"SELECT *
              FROM Customer
              WHERE CustomerUID = @customerUID", new { customerUID = customerUser.fk_CustomerUID }).FirstOrDefault();

        if (customer == null) return 0;


        const string insert =
          @"INSERT CustomerUser
            (fk_UserUID, fk_CustomerUID, LastActionedUTC)
            VALUES
            (@fk_UserUid, @fk_CustomerUID, @LastActionedUTC)";

        return Connection.Execute(insert, customerUser);
      }

      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customerUser.LastActionedUTC);
      return 0;
    }

    private int DissociateCustomerUser(Models.CustomerUser customerUser, Models.CustomerUser existing)
    {
      if (existing != null)
      {
        if (customerUser.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE 
              FROM CustomerUser               
              WHERE fk_CustomerUID = @fk_CustomerUID AND fk_UserUID = @fk_UserUID";
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
