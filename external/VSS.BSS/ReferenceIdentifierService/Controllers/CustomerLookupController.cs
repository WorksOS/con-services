using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class CustomerLookupController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ICustomerLookupManager _worker;

    public CustomerLookupController(ICustomerLookupManager worker)
    {
      _worker = worker;
    }

    [HttpGet]
    public HttpResponseMessage FindDealers(HttpRequestMessage request, long storeId, string svOrgIdentifiers)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2} and OrgIdentifiers {3}", GetType().Name, "FindDealers", storeId, svOrgIdentifiers);
      var response = new LookupResponse<IList<IdentifierDefinition>>();

      try
      {
        var identifierDefinitions = new List<IdentifierDefinition>();

        if (string.IsNullOrWhiteSpace(svOrgIdentifiers))
        {
          return request.CreateResponse(HttpStatusCode.NoContent, response);
        }

        string[] orgIdentifiers = svOrgIdentifiers.Trim(new[] {'\'', '"', ' '}).Split(';');
        
        if (string.IsNullOrWhiteSpace(orgIdentifiers[0]))
        {
          return request.CreateResponse(HttpStatusCode.NoContent, response);
        }

        foreach (var orgIdentifier in orgIdentifiers)
        {
          string[] idf = orgIdentifier.Split(',');
          if (!string.IsNullOrWhiteSpace(idf[0]) && !string.IsNullOrWhiteSpace(idf[1]))
          {
            identifierDefinitions.Add(new IdentifierDefinition {Alias = idf[0], Value = idf[1]});
          }
        }

        response.Data = _worker.FindDealers(identifierDefinitions, storeId);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding Dealers", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage FindCustomerGuidByCustomerId(HttpRequestMessage request, long customerId)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindCustomerGuidByCustomerId", customerId);
      var response = new LookupResponse<Guid?>();

      try
      {
        response.Data = _worker.FindCustomerGuidByCustomerId(customerId);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding CustomerGuid By CustomerId", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage FindAllCustomersForService(HttpRequestMessage request, Guid serviceUid)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindAllCustomersForService", serviceUid);
      var response = new LookupResponse<List<Guid?>>();

      try
      {
        response.Data = _worker.FindAllCustomersForService(serviceUid);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding All Customers for Service", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage FindCustomerParent(HttpRequestMessage request, Guid childUid, string parentCustomerTypeString)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindParentGuid", childUid);
      var response = new LookupResponse<Guid?>();

      try
      {
        var parentCustomerType = (CustomerTypeEnum)Enum.Parse(typeof(CustomerTypeEnum), parentCustomerTypeString);
        response.Data = _worker.FindCustomerParent(childUid, parentCustomerType);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding parent for customer", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage FindAccountsForDealer(HttpRequestMessage request, Guid dealerUid)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindAccountsForDealer", dealerUid);
      var response = new LookupResponse<IList<AccountInfo>>();
      try
      {
        response.Data = _worker.FindAccountsForDealer(dealerUid);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding parent for customer", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}
