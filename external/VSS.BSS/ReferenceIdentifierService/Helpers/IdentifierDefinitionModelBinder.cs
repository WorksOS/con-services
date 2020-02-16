using System;
using log4net;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Helpers
{
  public class IdentifierDefinitionModelBinder : IModelBinder
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
      var identifierDefinition = new IdentifierDefinition();

      var storeId = bindingContext.ValueProvider.GetValue("storeId");
      var alias = bindingContext.ValueProvider.GetValue("alias");
      var value = bindingContext.ValueProvider.GetValue("value");
      var uid = bindingContext.ValueProvider.GetValue("uid");

      if (uid != null && !string.IsNullOrEmpty(uid.AttemptedValue))
      {
        Log.IfDebugFormat("Query contains uid {0}", uid.AttemptedValue);
        Guid uidValue;
        if (Guid.TryParse(uid.AttemptedValue, out uidValue))
        {
          identifierDefinition.UID = uidValue;
        }
        else
        {
          Log.IfWarn("Invalid request: query string does not have valid uid");
          actionContext.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
            "Invalid request: query string does not have valid uid");
          return false;
        }
      }

      long storeIdValue;
      if (storeId != null && !string.IsNullOrEmpty(storeId.AttemptedValue) &&
          long.TryParse(storeId.AttemptedValue, out storeIdValue))
      {
        Log.IfDebugFormat("Query contains storeid {0}", storeIdValue);
        identifierDefinition.StoreId = storeIdValue;
      }
      else
      {
        Log.IfWarn("Invalid request: query string does not have valid storeId");
        actionContext.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
          "Invalid request: query string does not have valid storeId");
        return false;
      }

      if (alias != null && !string.IsNullOrEmpty(alias.AttemptedValue))
      {
        identifierDefinition.Alias = alias.AttemptedValue;
      }
      else
      {
        Log.IfWarn("Invalid request: query string does not have valid alias");
        actionContext.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
          "Invalid request: query string does not have valid alias");
        return false;
      }

      if (value != null && !string.IsNullOrEmpty(value.AttemptedValue))
      {
        identifierDefinition.Value = value.AttemptedValue;
      }
      else
      {
        Log.IfWarn("Invalid request: query string does not have valid value");
        actionContext.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
          "Invalid request: query string does not have valid value");
        return false;
      }

      bindingContext.Model = identifierDefinition;
      return true;
    }
  }
}
