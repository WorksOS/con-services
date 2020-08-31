using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Entitlements.Common.Models;

namespace VSS.Productivity3D.Entitlements.Common.Executors
{
  public class GetEntitlementsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var model = CastRequestObjectTo<GetEntitlementsRequest>(item, errorCode: 1);

      //TODO: Ask Steve: return ContractExecutionResult or throw ServiceException and handle in Controller
      var user = model.User;
      if (user == null)
      {
        var message = "User is null.";
        log.LogWarning(message);
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, message);
      }
      //TODO: do we still need this email stuff?
      if (string.IsNullOrEmpty(user.UserEmail))
      {
        var message = "User email is empty.";
        log.LogWarning(message);
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, message);
      }

      var userUid = user.Identity.Name;
      if (string.IsNullOrEmpty(userUid))
      {
        var message = "User UID is empty.";
        log.LogWarning(message);
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, message);
      }

      //TODO: Ditto
      var request = model.Request;
      if (string.Compare(user.UserEmail, request.UserEmail, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        log.LogWarning($"Provided email {request.UserEmail} does not match JWT TID email: {user.UserEmail}");
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Provided email does not match JWT.");
      }

      if (string.Compare(userUid, request.UserUid, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        log.LogWarning($"Provided uuid {request.UserUid} does not match JWT TID uuid: {userUid}");
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Provided uuid does not match JWT.");
      }

      /*
      //TODO: This is supposed to be optional. Check with Steve.
      if (string.IsNullOrEmpty(request.OrganizationIdentifier))
      {
        log.LogWarning("No Organization Identifier provided");
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "No Organization Identifier provided");
      }
      */

      var customerUid = string.IsNullOrEmpty(request.OrganizationIdentifier) ? (Guid?) null : Guid.Parse(request.OrganizationIdentifier);
      var statusCode = await emsClient.GetEntitlements(Guid.Parse(request.UserUid), customerUid, "wos_sku", "wos_feature", CustomHeaders);
      /*
      // Temporary until we have an external endpoint
      if (string.Compare(request.Feature, "worksos", StringComparison.InvariantCultureIgnoreCase) != 0)
        return new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "User is not entitled");

      //TODO: Is this email stuff still required?
      var isEmailAccepted = model.AcceptedEmails.Contains(request.UserEmail.ToLower());
      log.LogInformation($"{request.UserEmail} {(isEmailAccepted ? "is an accepted email" : "is not in the allowed list")}");

      if (!model.EnableEntitlementCheck)
      {
        log.LogInformation($"Entitlement checking is disabled, allowing the request.");
        isEmailAccepted = true;
      }

      if (isEmailAccepted)
        return new ContractExecutionResult();
      return new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "User is not entitled");

      */
      if (statusCode == HttpStatusCode.Accepted)
        return new ContractExecutionResult();
      if (statusCode == HttpStatusCode.NoContent)
        return new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "User is not entitled");

      //Shouldn't happen but...
      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Unexpected response {statusCode} from EMS");
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    public IHeaderDictionary CustomHeaders => 
      new HeaderDictionary
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {authn.GetApplicationBearerToken()}"}
      };
  }
}
