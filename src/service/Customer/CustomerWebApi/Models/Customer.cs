using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace CustomerWebApi.Models
{
  public class Customer
  {
    private ReadMySqlDb mysql;
    private readonly string ConnectionString;
    private ILogger Log;
    private Authorization authorization;

    public Customer(string connectionString, ILogger logger, Authorization auth) 
    {
      ConnectionString = connectionString;
      Log = logger;
      authorization = auth;

    }
    public CustomerDataResult getCustomersforUser()
    {
      var result = new CustomerDataResult
      {
        status = 200,
        metadata = new Metadata
        { msg = "success" },
        customer = new List<CustomerData>()
      };

      if (string.IsNullOrEmpty(authorization.customerUid))
      {
        var customerList = GetAllCustomersForAUser(authorization.userUid);
        foreach (var singleCustomer in customerList)
        {
          result.customer.Add(singleCustomer);
        }
      }
      else
      {
        var singleCustomer = GetSingleCustomer(authorization.customerUid);
        result.customer.Add(singleCustomer);
      }
      return result;
    }

    /// <summary>
    /// Create the user from the jwt 
    /// </summary>
    /// <param name="authorization"></param>
    private bool CreateUserForDefaultCustomer(Authorization authorization)
    {
      var user = GetUser(authorization.userUid);
      
      if (user is null) 
      {
        CreateUser(authorization);
        return true;
      }
      // if the user exists then return false
      Log.LogInformation($"User exists: Email{user.Email} Name{user.UserName}");
      return false;

    }

    public AccountsResult getCustomerAccountsforUser()
    {
      var result = new AccountsResult
      {
        UserUID = authorization.userUid
      };
      var IsExists = CreateUserForDefaultCustomer(authorization);
      var customerList = GetAllAccountsForAUser(authorization.userUid);
      foreach (var singleCustomer in customerList)
      {
        result.Customers.Add(singleCustomer);
      }
      return result;
    }

    private List<Customers> GetAllAccountsForAUser(string userUid)
    {
      var mysql = new ReadMySqlDb(ConnectionString, Log);
      var custsResult = mysql.GetAllAccountsForUserUid(userUid);
      return custsResult;
    }

    private List<CustomerData> GetAllCustomersForAUser(string userUid)
    {
      var mysql = new ReadMySqlDb(ConnectionString, Log);
      var custsResult = mysql.GetAllCustomersForUserUid(userUid);
      return custsResult;
    }

    private CustomerData GetSingleCustomer(string customerUid)
    {
      var mysql = new ReadMySqlDb(ConnectionString, Log);
      var custsResult = mysql.GetCustomerDetails(customerUid);
      return custsResult;
    }

    private User GetUser(string userUid)
    {
      var mysql = new ReadMySqlDb(ConnectionString, Log);
      var custsResult = mysql.GetUserDetails(userUid);
      return custsResult;
    }


    private void CreateUser(Authorization authorization)
    {
      var mysql = new ReadMySqlDb(ConnectionString, Log);
      var mysqlresult = mysql.CreateUserDetails("8abcf851-44c5-e311-aa77-00505688274d", authorization.userUid,authorization.userEmail);
      if (mysqlresult != 1)
      {
        Log.LogError("mysql error creating the user. The result is " + mysqlresult);
      }
    }
  }
}
