using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using log4net;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using LegacyApiUserProvisioning.WebApi.IdentityManager.Common.Errors;
using VSS.Authentication.JWT;
using VSS.Hosted.VLCommon;

namespace LegacyApiUserProvisioning.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SupportAppAuthorization : AuthorizeAttribute
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string PermittedEngineeringOperationsCustomerUid = ConfigurationManager.AppSettings["EngineeringOperationsCustomerUid"];
        private static readonly string[] PermittedCallerApps = ConfigurationManager.AppSettings["PermittedCallerAppNames"].Replace(" ", string.Empty).Split(';');

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            const string classMethod = "VlAuthorizationAttribute.AuthorizeROnAuthorization";
            var request = actionContext.Request;

        #if DEBUG
            if (request.RequestUri.ToString().Contains("http://localhost"))
            {
                Logger.IfDebugFormat($"{classMethod} in Debug Mode with http .. returning now");
                return;
            }
                    
        #endif

            Logger.IfDebugFormat($"{classMethod} {request.Method.Method}");
            if (!ValidateJwt(actionContext, request.Headers, out var userUid, out var customerUid))
            {
                Logger.IfDebugFormat($"{classMethod} jwt is not valid ... returning now");
                return;
            }

            request.Headers.Add(IdentityConstants.USERUID_API, userUid.ToString());
            request.Headers.Add(IdentityConstants.VISIONLINK_CUSTOMERUID, customerUid.ToString());

            Logger.IfInfoFormat($"{classMethod} Support App Authorization is successful");

        }

        private static bool ValidateJwt(HttpActionContext actionContext, HttpRequestHeaders headers,
            out Guid userUid, out Guid customerUid)
        {
            const string classMethod = "VlAuthorizationAttribute.ValidateJwt";

            userUid = Guid.Empty;
            customerUid = Guid.Empty;

            Logger.IfDebugFormat($"{classMethod} jwt headers {headers}");

            TPaaSJWT jwtToken = null;
            try
            {
                jwtToken = new TPaaSJWT(headers);
            }
            catch (Exception exp)
            {
                Logger.IfErrorFormat(classMethod, exp);
                HandleUnauthorizedRequest(actionContext, string.Format(Messages.Not_Found, "User"), 500102,
                    HttpStatusCode.InternalServerError);
                return false;
            }

            userUid = GetUserUid(jwtToken, headers);
            if (userUid == Guid.Empty)
            {
                Logger.IfDebugFormat($"{classMethod} Bad Request: UserUID is Null/Empty");
                HandleUnauthorizedRequest(actionContext, string.Format(Messages.Null_Message, "user_uid"), 400111,
                    HttpStatusCode.BadRequest);
                return false;
            }

            customerUid = GetCustomerUid(headers);
            if (customerUid == Guid.Empty)
            {
                Logger.IfDebugFormat($"{classMethod}, Bad Request: CustomerUID is Null/Empty");
                HandleUnauthorizedRequest(actionContext, string.Format(Messages.Null_Message, "customer_UID"), 400112,
                    HttpStatusCode.BadRequest);
                return false;
            }


            var isEngineeringOperationsCustomer = string.Equals(customerUid.ToString().Replace("-", ""),
                PermittedEngineeringOperationsCustomerUid.Replace("-", ""),
                StringComparison.InvariantCultureIgnoreCase);

            if (!isEngineeringOperationsCustomer)
            {
                Logger.IfDebugFormat($"{classMethod}, user does not belong to Engineering Operations account");
                HandleUnauthorizedRequest(actionContext, string.Format(Messages.Not_Found, "user and Engineering Operations relation"), 400112,
                    HttpStatusCode.Unauthorized);
                return false;
            }

            if (!IsCalledFromSupportedApps(jwtToken))
            {
                Logger.IfErrorFormat($"{classMethod}, is not called from Unified support");
								HandleUnauthorizedRequest(actionContext, string.Format(Messages.Not_Found, "Request is not from Unified support"), 400112,
										HttpStatusCode.Unauthorized);
				return false;
            }

            return true;
        }

        private static bool IsCalledFromSupportedApps(TPaaSJWT jwtToken)
        {
            const string classMethod = "VlAuthorizationAttribute.IsCalledFromUnifiedSupport";
            
            foreach (var permittedCallerApp  in PermittedCallerApps)
            {

                if (string.Equals(jwtToken.ApplicationName, permittedCallerApp, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            
            Logger.IfDebugFormat($"{classMethod} call from jwt.ApplicationName = {jwtToken.ApplicationName} is not allowed");
            return false;
        }

        private static void HandleUnauthorizedRequest(HttpActionContext actionContext, string errorMessage, int errorCode,
            HttpStatusCode httpStatusCode)
        {
            if (actionContext.Response == null)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(httpStatusCode,
                        new ErrorInfo {Message = errorMessage, ErrorCode = errorCode, StatusCode = httpStatusCode});
            }
        }

        private static Guid GetUserUid(TPaaSJWT jwtToken, HttpHeaders headers)
        {
            const string classMethod = "VLAuthorization.GetUserUid";
            var userUid = jwtToken.IsApplicationUserToken ? jwtToken.UserUid.ToString() : string.Empty;
            if (string.IsNullOrEmpty(userUid))
            {
                if (headers.Contains(IdentityConstants.VISIONLINK_USERUID))
                {
                    userUid = headers.GetValues(IdentityConstants.VISIONLINK_USERUID).FirstOrDefault();
                }
            }

            if (!Guid.TryParse(userUid, out var userGuid))
            {
                Logger.Error($"{classMethod}, UserUID is invalid");
            }

            return userGuid;
        }

        private static Guid GetCustomerUid(HttpHeaders headers)
        {
            const string classMethod = "VLAuthorization.GetCustomerUid";
            var customerUid = string.Empty;
            if (headers.Contains(IdentityConstants.VISIONLINK_CUSTOMERUID))
            {
                customerUid = headers.GetValues(IdentityConstants.VISIONLINK_CUSTOMERUID).FirstOrDefault();
            }

            if (!Guid.TryParse(customerUid, out var customerGuid))
            {
                Logger.Error($"{classMethod}, customerUID is invalid");
            }

            return customerGuid;
        }
    }
}