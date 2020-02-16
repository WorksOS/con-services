using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using LegacyApiUserProvisioning.WebApi.Filters;
using log4net;
using LegacyApiUserProvisioning.WebApi.Models;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon;
using VSS.UserAuthorization.Attributes;

namespace LegacyApiUserProvisioning.WebApi.Controllers
{
	[RoutePrefix("1.0/LegacyApiUsers")]
    [SupportAppAuthorization]
    public class LegacyApiUsersController : ApiController
    {
        private readonly IUserManager _userManager;
        private readonly ILog _logger;

        public LegacyApiUsersController(IUserManager userManager, ILog logger)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("CreateApiUser")]
        [AuthorizeAPI(Action = "SupportCreate", Resource = "User")]
        public HttpResponseMessage CreateUser(UserCreationRequest request)
        {
            _logger.IfDebugFormat($"LegacyApiUsersController.CreateUser started with request {request}");

            //work around for api cloud issue - when request comes through tpaas api, custom body objects do not come through
            var userCreationRequest = FetchRequest(request);

            var response = _userManager.CreateUser(userCreationRequest);
            if (string.IsNullOrEmpty(response.Error))
            {
                return Request.CreateResponse(HttpStatusCode.Created, response);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, response.Error);
        }

        [HttpPut]
        [Route("EditApiUser")]
        [AuthorizeAPI(Action = "SupportEdit", Resource = "User")]
        public HttpResponseMessage EditUser(UserEditRequest request)
        {
            _logger.IfDebugFormat($"LegacyApiUsersController.CreateUser started with request {request}");

            //work around for api cloud issue - when request comes through tpaas api, custom body objects do not come through
            var userEditRequest = FetchRequest(request);

            var response = _userManager.EditUser(userEditRequest);
            if (string.IsNullOrEmpty(response.Error))
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest, response.Error);
        }

        [HttpPut]
        [Route("DeleteApiUser")]
        [AuthorizeAPI(Action = "SupportDelete", Resource = "User")]
        public HttpResponseMessage DeleteUser(UserDeleteRequest request)
        {
            _logger.IfDebugFormat($"LegacyApiUsersController.DeleteUser started with request {request}");
            //work around for api cloud issue - when request comes through tpaas api, custom body objects do not come through
            try
            {
                var userDeleteRequest = FetchRequest(request);
            
                var response = _userManager.DeleteUsers(userDeleteRequest);
                if (response.Success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, response.Error);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUsersOfCustomer")]
        [AuthorizeAPI(Action = "SupportView", Resource = "User")]
        public HttpResponseMessage GetUsers(string customerUid)
        {
            _logger.IfDebugFormat($"LegacyApiUsersController.GetUsers started with customerUid {customerUid}");
            var usersByOrganization = _userManager.GetUsersByOrganization(customerUid);
            if (usersByOrganization != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, usersByOrganization);
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpGet]
        [Route("GetApiFeaturesByUserName")]
        [AuthorizeAPI(Action = "SupportView", Resource = "User")]
        public HttpResponseMessage GetApiFeatures(string useName)
        {
            _logger.IfDebugFormat($"LegacyApiUsersController.GetUser started with username {useName}");
            var feature = _userManager.GetApiFeaturesByUserName(useName);
            if (feature!= null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, feature);
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPost]
		[Route("UpdateCustomerOfApiUsers")]
		[AuthorizeAPI(Action = "Migrate", Resource = "User")]
		public HttpResponseMessage UpdateApiUserCustomer(MigrateUsersRequest migrateUsers)
		{
			migrateUsers = FetchRequest<MigrateUsersRequest>(migrateUsers);
			_logger.IfDebugFormat($"LegacyApiUsersController.UpdateApiUserCustomer started with customerUid {migrateUsers.CustomerUid}");
			return Request.CreateResponse(_userManager.UpdateCustomerOfApiUsers(migrateUsers));
		}

		public T FetchRequest<T>(T request)
		{
			if (request == null)
			{
				var requestJson = Request.Content.ReadAsStringAsync().Result;
				request = JsonConvert.DeserializeObject<T>(requestJson);
			}
			if (request != null)
			{
				_logger.IfInfoFormat(MethodBase.GetCurrentMethod().Name + JsonConvert.SerializeObject(request));
				return request;
			}

			_logger.IfInfoFormat("Request body content is null");
			throw new Exception();
		}
	}
}