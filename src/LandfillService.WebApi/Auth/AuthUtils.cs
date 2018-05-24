using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Common.Models;
using Common.Repository;
using VSP.MasterData.Customer.WebAPI.Models;
using VSS.Authentication.JWT;

public class AuthUtilities
{

  public AuthUtilities()
  {
  }

  public AssociatedCustomer GetContext(HttpRequestHeaders headers, out string errorMessage, out string userId)
  {
    string token = String.Empty;
    return GetContext(headers, out errorMessage, out userId, out token);
  }

  public AssociatedCustomer GetContext(HttpRequestHeaders headers, out string errorMessage, out string userId, out string token)
  {
    userId = String.Empty;
    token = String.Empty;
    try
    {
      var jwt = TryGetJwtToken(headers, out token);
      if (jwt)
      {
        var jwtToken = new TPaaSJWT(token);
        userId = jwtToken.UserUid.ToString();
        var customerUid = headers.GetValues("X-VisionLink-CustomerUid").ElementAt(0);

        var customer = LandfillDb.GetCustomer(Guid.Parse(customerUid));

        if (customer == null)
        {
          errorMessage = $"No customer with ID: {customerUid}";
          return null;
        }

        var customerbyUser = LandfillDb.GetAssociatedCustomerbyUserUid(Guid.Parse(userId));

        if (customerbyUser == null || customerbyUser.All(cui => cui.CustomerUID != customer.CustomerUID))
        {
          errorMessage = $"No customer associated with user ID: {userId}";
          return null;
        }

        AssociatedCustomer associatedCustomer = new AssociatedCustomer()
        {
          CustomerUID = Guid.Parse(customer.CustomerUID),
          CustomerName = customer.CustomerName,
          CustomerType = customer.CustomerType
        };

        errorMessage = "";
        return associatedCustomer;
  
      }
      errorMessage = "No token";
      return null;
    }
    catch (Exception ex)
    {
      errorMessage = $"Can not retrieve cusomer context: Invalid token. Exception: {ex.Message}";
      return null;
    }
  }

  public IEnumerable<Project> GetLandfillProjectsForUser(string userUid)
  {
    try
    {
      return LandfillDb.GetLandfillProjectsForUser(userUid);
    }
    catch (Exception)
    {
      return null;
    }
  }

  /// <summary>
  ///   This method is used to get the Jwt Assertion Token string from the HTTP Request Header
  /// </summary>
  /// <param name="httpRequestHeaders">Incoming Request Headers</param>
  /// <param name="jwtToken">Output parameter - Jwt Assetion Token string</param>
  /// <returns>true, if Http Headers contain Jwt; false, otherwise</returns>
  private bool TryGetJwtToken(HttpRequestHeaders httpRequestHeaders, out string jwtToken)
  {
    return TryGetHeader(httpRequestHeaders, "X-Jwt-Assertion", out jwtToken);
  }

  /// <summary>
  ///   This method is used to get the Jwt Assertion Token string from the HTTP Request Header
  /// </summary>
  /// <param name="httpRequestHeaders">Incoming Request Headers</param>
  /// <param name="headerName"></param>
  /// <param name="headerValue"></param>
  /// <returns>true, if Http Headers contain Jwt; false, otherwise</returns>
  private bool TryGetHeader(HttpRequestHeaders httpRequestHeaders, string headerName, out string headerValue)
  {
    headerValue = null;
    try
    {
      if (httpRequestHeaders.Contains(headerName))
      {
        //if present read the first element from HTTP Request Header headerName
        headerValue = httpRequestHeaders.GetValues(headerName).FirstOrDefault();
        return true;
      }
      //If no headerName header in the request, then return false with null headerValue output param
      return false;
    }
    catch
    {
      //If any exceptions in getting headerName header, then return false with null headerValue output param
      return false;
    }
  }

}