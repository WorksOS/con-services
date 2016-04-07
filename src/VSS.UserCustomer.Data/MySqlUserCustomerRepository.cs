using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using MySql.Data.MySqlClient;
using VSS.UserCustomer.Data.Interfaces;
using VSS.UserCustomer.Data.Models;
using LandfillService.Common.Repositories;

namespace VSS.UserCustomer.Data
{
  public class MySqlUserCustomerRepository : RepositoryBase, IUserCustomerService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    //private readonly string _connectionString;

    //public MySqlUserCustomerRepository()
    //{
    //  _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    //}

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
        userCustomer.LastActionedUTC = customerEvent.ActionUTC;
        
        eventType = "AssociateCustomerUserEvent";
      }
      else if (evt is DissociateCustomerUserEvent)
      {
        var customerEvent = (DissociateCustomerUserEvent)evt;
        userCustomer.fk_CustomerUID = customerEvent.CustomerUID.ToString();
        userCustomer.fk_UserUID = customerEvent.UserUID.ToString();
        userCustomer.LastActionedUTC = customerEvent.ActionUTC;

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

      PerhapsOpenConnection();

      Log.DebugFormat("UserCustomerRepository: Upserting eventType{0} CustomerUid={1}, UserUID={2}", 
        eventType, userCustomer.fk_CustomerUID, userCustomer.fk_UserUID);

      var existing = Connection.Query<Models.UserCustomer>
        (@"SELECT 
            fk_UserUID, fk_CustomerUID, LastActionedUTC
              FROM CustomerUser
              WHERE fk_CustomerUID = @customerUID AND fk_UserUID = @userUID", new { customerUID = userCustomer.fk_CustomerUID, userUID = userCustomer.fk_UserUID }).FirstOrDefault();

      if (eventType == "AssociateCustomerUserEvent")
      {
        upsertedCount = AssociateCustomerUser(userCustomer, existing);
      }

      if (eventType == "DissociateCustomerUserEvent")
      {
        upsertedCount = DissociateCustomerUser(userCustomer, existing);
      }

      Log.DebugFormat("UserCustomerRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateCustomerUser(Models.UserCustomer userCustomer, Models.UserCustomer existing)
    {
      if (existing == null)
      {
        //TODO: May need to dummy this like projects and customers due to out of order events

        var customer = Connection.Query<VSS.Customer.Data.Models.Customer>
          (@"SELECT *
              FROM Customer
              WHERE CustomerUID = @customerUID", new { customerUID = userCustomer.fk_CustomerUID }).FirstOrDefault();

        if (customer == null || customer.CustomerId <= 0) return 0;


        const string insert =
          @"INSERT CustomerUser
            (fk_UserUID, fk_CustomerUID, LastActionedUTC)
            VALUES
            (@fk_UserUid, @fk_CustomerUID, @LastActionedUTC)";

        return Connection.Execute(insert, userCustomer);
      }

      Log.DebugFormat("UserCustomerRepository: can't create as already exists newActionedUTC={0}", userCustomer.LastActionedUTC);
      return 0;
    }

    private int DissociateCustomerUser(Models.UserCustomer userCustomer, Models.UserCustomer existing)
    {
      if (existing != null)
      {
        if (userCustomer.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE 
              FROM CustomerUser               
              WHERE fk_CustomerUID = @fk_CustomerUID AND fk_UserUID = @fk_UserUID";
          return Connection.Execute(delete, userCustomer);
        }
        
        Log.DebugFormat("UserCustomerRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
          existing.LastActionedUTC, userCustomer.LastActionedUTC);
      }
      else
      {
        Log.DebugFormat("UserCustomerRepository: can't delete as none existing newActionedUTC {0}",
          userCustomer.LastActionedUTC);
      }
      return 0;
    }


  }
}
