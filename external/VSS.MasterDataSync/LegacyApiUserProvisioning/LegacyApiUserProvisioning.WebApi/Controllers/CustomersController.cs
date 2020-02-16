using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using log4net;
using LegacyApiUserProvisioning.CustomerData.Interfaces;
using VSS.Hosted.VLCommon;
using VSS.UserAuthorization.Attributes;
using System.Web.Http;
using LegacyApiUserProvisioning.WebApi.Filters;

namespace LegacyApiUserProvisioning.WebApi.Controllers
{
    [RoutePrefix("1.0/LegacyApiUsers")]
    [SupportAppAuthorization]
    public class CustomersController : ApiController
    {
        private readonly ICustomerService _customerService;
        private readonly ILog _logger;

        public CustomersController(ICustomerService customerService, ILog logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetInactiveCustomers")]
        [AuthorizeAPI(Action = "SupportView", Resource = "User")]
        public HttpResponseMessage GetInactiveCustomer(string filter, int maxResults = 20)
        {
            const string classMethod = "CustomersController.GetInactiveCustomer";
            _logger.IfDebug($"{classMethod} called with {filter}");
            var customers = _customerService.GetInActiveCustomers(filter.Trim(), maxResults).ToList();
            if (customers.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, customers);
            }

            _logger.IfDebug($"{classMethod}, {filter} not found");
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("GetActiveCustomers")]
        [AuthorizeAPI(Action = "SupportView", Resource = "User")]
        public HttpResponseMessage GetActiveCustomers(string filter, int maxResults = 20)
        {
            const string classMethod = "CustomersController.GetActiveCustomers";
            _logger.IfDebug($"{classMethod} called with {filter}");
            var customers = _customerService.GetActiveCustomers(filter.Trim(), maxResults).ToList();
            if (customers.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, customers);
            }

            _logger.IfDebug($"{classMethod}, {filter} not found");
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }


        [HttpGet]
        [Route("GetCustomers")]
        [AuthorizeAPI(Action = "SupportView", Resource = "User")]
        public HttpResponseMessage GetActiveInactiveCustomers(string filter, int maxResults = 20)
        {
            const string classMethod = "CustomersController.GetActiveCustomers";
            _logger.IfDebug($"{classMethod} called with {filter}");
            var customers = _customerService.GetActiveInactiveCustomers(filter.Trim(), maxResults).ToList();
            if (customers.Any())
            {
            }

            _logger.IfDebug($"{classMethod}, {filter} not found");
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

     [HttpGet]
     [Route("GetActiveCustomersByBssid")]
     [AuthorizeAPI(Action = "SupportView", Resource = "User")]
     public HttpResponseMessage GetActiveCustomersByBssid(string filter, bool exactMatch = true, int maxResults = 20)
     {
         const string classMethod = "CustomersController.GetActiveCustomersByBssid";
         _logger.IfDebug($"{classMethod} called with {filter}");
         var customers = _customerService.GetCustomersByBssid(filter.Trim(), true, exactMatch, maxResults).ToList();
         if (customers.Any())
         {
             return Request.CreateResponse(HttpStatusCode.OK, customers);
         }

         _logger.IfDebug($"{classMethod}, {filter} not found");
         return Request.CreateResponse(HttpStatusCode.NoContent);
     }

     [HttpGet]
     [Route("GetInActiveCustomersByBssid")]
     [AuthorizeAPI(Action = "SupportView", Resource = "User")]
     public HttpResponseMessage GetInActiveCustomersByBssid(string filter, bool exactMatch = true, int maxResults = 20)
     {
         const string classMethod = "CustomersController.GetInActiveCustomerByBssid";
         _logger.IfDebug($"{classMethod} called with {filter}");
         var customers = _customerService.GetCustomersByBssid(filter.Trim(), false, exactMatch, maxResults).ToList();
         if (customers.Any())
         {
             return Request.CreateResponse(HttpStatusCode.OK, customers);
         }

         _logger.IfDebug($"{classMethod}, {filter} not found");
         return Request.CreateResponse(HttpStatusCode.NoContent);
     }
    }
}