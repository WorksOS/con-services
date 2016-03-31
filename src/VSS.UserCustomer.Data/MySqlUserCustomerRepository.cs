using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using MySql.Data.MySqlClient;
using VSS.UserCustomer.Data.Interfaces;
using VSS.UserCustomer.Data.Models;

namespace VSS.UserCustomer.Data
{
  public class MySqlUserCustomerRepository : IUserCustomerService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly string _connectionString;

    public MySqlUserCustomerRepository()
    {
      _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    }

    public int StoreUserCustomer(IUserCustomerEvent evt)
    {
      var upsertedCount = 0;
      var userCustomer = new Models.UserCustomer();
      string eventType = "Unknown";

      if (evt is AssociateCustomerUserEvent)
      {
        var customerEvent = (AssociateCustomerUserEvent)evt;
        userCustomer.fk_CustomerUID = customerEvent.CustomerUID.ToString();
        userCustomer.fk_UserUID = customerEvent.UserUID.ToString();
        userCustomer.LastUserUTC = customerEvent.ActionUTC;
        
        eventType = "AssociateCustomerUserEvent";
      }
      else if (evt is DissociateCustomerUserEvent)
      {
        var customerEvent = (DissociateCustomerUserEvent)evt;
        userCustomer.fk_CustomerUID = customerEvent.CustomerUID.ToString();
        userCustomer.fk_UserUID = customerEvent.CustomerUID.ToString();
        userCustomer.LastUserUTC = customerEvent.ActionUTC;

        eventType = "DissociateCustomerUserEvent";
      }

      upsertedCount = UpsertUserCustomerDetail(userCustomer, eventType);

      return upsertedCount;
    }

    /// <summary>
    /// All UserCustomer detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="userCustomer"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private int UpsertUserCustomerDetail(Models.UserCustomer userCustomer, string eventType)
    {
      int upsertedCount = 0;
      using (var connection = new MySqlConnection(_connectionString))
      {
        Log.DebugFormat("UserCustomerRepository: Upserting eventType{0} userCustomerUid={1}", eventType, userCustomer.UserCustomerID);

        connection.Open();
        var existing = connection.Query<Models.UserCustomer>
          (@"SELECT 
              fk_UserUID, fk_CustomerUID, fk_CustomerID, LastUserUTC
                FROM UserCustomer
                WHERE fk_CustomerUID = @customerUID AND fk_UserUID = @userUID", new { customerUID = userCustomer.fk_CustomerUID, userUID = userCustomer.fk_UserUID }).FirstOrDefault();

        if (eventType == "AssociateCustomerUserEvent")
        {
          upsertedCount = AssociateCustomerUser(connection, userCustomer, existing);
        }

        if (eventType == "DissociateCustomerUserEvent")
        {
          upsertedCount = DissociateCustomerUser(connection, userCustomer, existing);
        }

        Log.DebugFormat("UserCustomerRepository: upserted {0} rows", upsertedCount);
        connection.Close();
      }
      return upsertedCount;
    }

    public int AssociateCustomerUser(MySqlConnection connection, Models.UserCustomer userCustomer, Models.UserCustomer existing)
    {
      if (existing == null)
      {
        var customer = connection.Query<VSS.Customer.Data.Models.Customer>
          (@"SELECT *
              FROM Customer
              WHERE CustomerUID = @customerUID", new { customerUID = userCustomer.fk_CustomerUID }).FirstOrDefault();

        if (customer == null || customer.CustomerID <= 0) return 0;

        userCustomer.fk_CustomerID = customer.CustomerID;

        const string insert =
          @"INSERT UserCustomer
            (fk_UserUID, fk_CustomerUID, fk_CustomerID, LastUserUTC)
            VALUES
            (@fk_UserUid, @fk_CustomerUID, @fk_CustomerID, @LastUserUTC)";

        return connection.Execute(insert, userCustomer);
      }

      Log.DebugFormat("UserCustomerRepository: can't create as already exists newActionedUTC {0}", userCustomer.LastUserUTC);
      return 0;
    }

    public int DissociateCustomerUser(MySqlConnection connection, Models.UserCustomer userCustomer, Models.UserCustomer existing)
    {
      if (existing != null)
      {
        if (userCustomer.LastUserUTC >= existing.LastUserUTC)
        {
          const string delete =
            @"DELETE 
              FROM UserCustomer                
              WHERE fk_CustomerUID = @fk_CustomerUID AND fk_UserUID = @fk_UserUID";
          return connection.Execute(delete, userCustomer);
        }
        
        Log.DebugFormat("UserCustomerRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
          existing.LastUserUTC, userCustomer.LastUserUTC);
      }
      else
      {
        Log.DebugFormat("UserCustomerRepository: can't delete as none existing newActionedUTC {0}",
          userCustomer.LastUserUTC);
      }
      return 0;
    }

    public Models.UserCustomer GetUserCustomer(int UserCustomerID)
    {
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        var userCustomer = connection.Query<Models.UserCustomer>
          (@"SELECT *
              FROM UserCustomer
              WHERE UserCustomerID = @userCustomerID", new { userCustomerID = UserCustomerID }).FirstOrDefault();

        connection.Close();

        return userCustomer;
      }

    }

    public IEnumerable<Models.UserCustomer> GetUserCustomers()
    {
      IEnumerable<Models.UserCustomer> userCustomers;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        userCustomers = connection.Query<Models.UserCustomer>
          (@"SELECT 
                   UserCustomerID, fk_UserUID, fk_CustomerUID, fk_CustomerID, LastUserUTC
                FROM Customer");

        connection.Close();
      }

      return userCustomers;
    }

  }
}
