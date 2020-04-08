using System;
using System.Collections.Generic;
using CustomerWebApi.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using VSS.MasterData.Models.Models;

namespace CustomerWebApi
{
  public class ReadMySqlDb
  {
    private string ConnectionString;

    public ReadMySqlDb(string connectionString, ILogger logger)
    {
      ConnectionString = connectionString;
      //logger.LogInformation(ConnectionString);
    }

    private List<CustomerData> CustomerListQuery(string queryString, MySqlConnection dbCon)
    {
      var mySqlCommand = new MySqlCommand(queryString, dbCon);
      var mySqlDataReader = mySqlCommand.ExecuteReader();
      var customerDataList = new List<CustomerData>();
      while (mySqlDataReader.Read())
      {
        var singleCustomer = new CustomerData
        {
          uid = mySqlDataReader[0].ToString(),
          name = mySqlDataReader[1].ToString(),
          type = mySqlDataReader[2].ToString() == "1" ? "Customer" : "Dealer" 
        };

        customerDataList.Add(singleCustomer);
      }
      mySqlDataReader.Close();
      return customerDataList;
    }


    private List<Customers> AccountListQuery(string queryString, MySqlConnection dbCon)
    {
      var mySqlCommand = new MySqlCommand(queryString, dbCon);
      var mySqlDataReader = mySqlCommand.ExecuteReader();
      var customerDataList = new List<Customers>();
      while (mySqlDataReader.Read())
      {
        var singleCustomer = new Customers
        {
          CustomerUID = mySqlDataReader[0].ToString(),
          Name = mySqlDataReader[1].ToString(),
          CustomerType = mySqlDataReader[2].ToString() == "1" ? "Customer" : "Dealer",
          CustomerCode = mySqlDataReader[3].ToString(),
          DisplayName = mySqlDataReader[1].ToString()
        };

        customerDataList.Add(singleCustomer);
      }
      mySqlDataReader.Close();
      return customerDataList;
    }

    private User UserQuery(string queryString, MySqlConnection dbCon)
    {
      var mySqlCommand = new MySqlCommand(queryString, dbCon);
      var mySqlDataReader = mySqlCommand.ExecuteReader();
      while (mySqlDataReader.Read())
      {
        var user = new User
        {  //SELECT UserUID,UserName,Email,FirstName,LastName from User where UserUID=' '
          UserUID = mySqlDataReader[0].ToString(),
          UserName = mySqlDataReader[1].ToString(),
          Email = mySqlDataReader[2].ToString(),
          FirstName = mySqlDataReader[3].ToString(),
          LastName = mySqlDataReader[4].ToString()
        };
        mySqlDataReader.Close();
        return user;
      }

      return null;
    }

    public List<CustomerData> GetAllCustomersForUserUid(string userUid)
    {
      using (var connection = new MySqlConnection(ConnectionString))
      {
        connection.Open();
        var getCustomersCmd = "select c.CustomerUID , c.CustomerName,c.fk_CustomerTypeID from Customer c inner join UserCustomer u on c.CustomerUID = u.fk_CustomerUID where u.fk_UserUID = '" + userUid + "'";
        var result = CustomerListQuery(getCustomersCmd, connection);
        connection.Close();
        return result;
      }
    }

    public List<Customers> GetAllAccountsForUserUid(string userUid)
    {
      using (var connection = new MySqlConnection(ConnectionString))
      {
        connection.Open();
        var getCustomersCmd = "select c.CustomerUID , c.CustomerName,c.fk_CustomerTypeID,c.CustomerCode from Customer c inner join UserCustomer u on c.CustomerUID = u.fk_CustomerUID where u.fk_UserUID = '" + userUid + "'";
        var result = AccountListQuery(getCustomersCmd, connection);
        connection.Close();
        return result;
      }
    }



    public CustomerData GetCustomerDetails(string customerUid)
    {
      using (var connection = new MySqlConnection(ConnectionString))
      {
        connection.Open();
        var getCustomersCmd = "SELECT c.CustomerUID as `uid`,c.CustomerName as `name`,c.fk_CustomerTypeID as `type` FROM Customer as c where CustomerUID = '" + customerUid + "'";
        var result = CustomerListQuery(getCustomersCmd, connection);
        connection.Close();
        return result.FindLast(c => c.uid == customerUid);
      }
    }

    public User GetUserDetails(string userUid)
    {
      using (var connection = new MySqlConnection(ConnectionString))
      {
        connection.Open();
        var cmd = "SELECT UserUID,UserName,Email,FirstName,LastName from User where UserUID='" + userUid + "'";
        var result = UserQuery(cmd, connection);
        connection.Close();
        return result;
      }
    }

    public int CreateUserDetails(string customerUid, string userUid, string userEmail)
    {
      var cmd = "INSERT INTO User (UserUID,Email,UserName,FirstName,LastName) VALUES ('" + userUid + "','" + userEmail+ "',' ',' ',' ')";

      using (var connection = new MySqlConnection(ConnectionString))
      {
        connection.Open();
        var mySqlCommand = new MySqlCommand(cmd, connection);
        var mySqlDataNQ = mySqlCommand.ExecuteNonQuery();

        cmd = "INSERT INTO UserCustomer (fk_UserUID`,fk_CustomerUID,LastUserUTC`) VALUES ('" + userUid + "','" + customerUid + "',CURRENT_TIMESTAMP(6))";
        mySqlDataNQ = mySqlCommand.ExecuteNonQuery();

        connection.Close();
        return mySqlDataNQ;
      }
    }
  }
}
