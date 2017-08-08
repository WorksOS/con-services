using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.Common.Filters.Validation
{
  /// <summary>
  /// Attribute enabling obligatory validation of domain objects upon reception.
  /// </summary>
  public class ValidationFilterAttribute : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked. Validates the object.
    /// </summary>
    /// <param name="actionContext">The http action context.</param>
    /// <exception cref="ServiceException">Thrown when validation is not successfull.</exception>
    /// <exception cref="ContractExecutionResult">Built when exception is thrown.</exception>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      if (actionContext.ModelState.IsValid)
      {
        return;
      }

      //Extract the errors. This is to handle the validation ServiceExceptions being thrown to get the real error message to return. 
      var modelStateErrors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .Select(x => new { x.Key, x.Value.Errors })
        .ToArray();

      var errors = new List<KeyValuePair<string, string>>();

      foreach (var mse in modelStateErrors)
      {
        var key = mse.Key;
        var value = string.Empty;

        foreach (var error in mse.Errors)
        {
          if (error.Exception != null)
          {
            if (error.Exception is ServiceException ex)
            {
              value += ex.GetContent;
            }
            else
            {
              value += error.Exception.Message;
            }
          }
          else
          {
            value += error.ErrorMessage;
          }
          value += "\r\n";
        }
        errors.Add(new KeyValuePair<string, string>(key, value));
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
          JsonConvert.SerializeObject(errors)));
    }
  }
}