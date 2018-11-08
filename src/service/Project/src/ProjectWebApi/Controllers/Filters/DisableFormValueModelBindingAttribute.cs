using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VSS.MasterData.Project.WebAPI.Controllers.Filters
{
  /// <summary>
  /// Allows us to prevent the method from trying to bind and validate the input automatically which would preemptively read the input stream.
  /// This deferral lets us later use MultipartReader to freely parse the request body.
  /// See https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.1.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
  {
    /// <inheritdoc />
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
      var factories = context.ValueProviderFactories;
      factories.RemoveType<FormValueProviderFactory>();
      factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    /// <inheritdoc />
    public void OnResourceExecuted(ResourceExecutedContext context)
    { }
  }
}
