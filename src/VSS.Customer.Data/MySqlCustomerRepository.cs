using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using MySql.Data.MySqlClient;
using VSS.Customer.Data.Interfaces;
using VSS.Customer.Data.Models;

namespace VSS.Customer.Data
{
  public class MySqlCustomerRepository : ICustomerService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly string _connectionString;

    public MySqlCustomerRepository()
    {
      _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    }

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
        customer.CustomerType = customerEvent.CustomerType;
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
      using (var connection = new MySqlConnection(_connectionString))
      {
        Log.DebugFormat("CustomerRepository: Upserting eventType{0} customerUid={1}", eventType, customer.CustomerUid);

        connection.Open();
        var existing = connection.Query<Models.Customer>
          (@"SELECT 
                  CustomerUID, CustomerName, fk_CustomerTypeID AS CustomerType, LastActionedUTC
                FROM Customer
                WHERE CustomerUID = @CustomerUid", new { customer.CustomerUid }).FirstOrDefault();

        if (eventType == "CreateCustomerEvent")
        {
          upsertedCount = CreateCustomer(connection, customer, existing);
        }

        if (eventType == "UpdateCustomerEvent")
        {
          upsertedCount = UpdateCustomer(connection, customer, existing);
        }

        if (eventType == "DeleteCustomerEvent")
        {
          upsertedCount = DeleteCustomer(connection, customer, existing);
        }

        Log.DebugFormat("CustomerRepository: upserted {0} rows", upsertedCount);
        connection.Close();
      }
      return upsertedCount;
    }

    public int CreateCustomer(MySqlConnection connection, Models.Customer customer, Models.Customer existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Customer
              (CustomerUID, CustomerName, fk_CustomerTypeID, LastActionedUTC)
              VALUES
              (@CustomerUid, @CustomerName, @CustomerType, @LastActionedUtc)";

        return connection.Execute(insert, customer);
      }

      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customer.LastActionedUtc);

      return 0;
    }

    public int UpdateCustomer(MySqlConnection connection, Models.Customer customer, Models.Customer existing)
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
          return connection.Execute(update, customer);
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

    public int DeleteCustomer(MySqlConnection connection, Models.Customer customer, Models.Customer existing)
    {
      if (existing != null)
      {
        if (customer.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string delete =
            @"DELETE 
              FROM Customer                
              WHERE CustomerUID = @CustomerUid";
          return connection.Execute(delete, customer);
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
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        var customer = connection.Query<Models.Customer>
            (@"SELECT c.* 
              FROM Customer c JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
              WHERE cu.fk_UserUID = @userUid", new {userUid = userUid.ToString()}).FirstOrDefault();

        connection.Close();

        return customer;
      }
    }

    public Models.Customer GetCustomer(System.Guid customerUid)
    {
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        var customer = connection.Query<Models.Customer>
            (@"SELECT * 
                FROM Customer 
                WHERE CustomerUID = @customerUid", new { customerUid = customerUid.ToString() }).FirstOrDefault();

        connection.Close();

        return customer;
      }
    }

    public IEnumerable<Models.Customer> GetCustomers()
    {
      IEnumerable<Models.Customer> customers;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        customers = connection.Query<Models.Customer>
          (@"SELECT 
                   CustomerUID, CustomerName, fk_CustomerTypeID AS CustomerType, LastActionedUTC
                FROM Customer");

        connection.Close();
      }
      return customers;
    }

  }
}
