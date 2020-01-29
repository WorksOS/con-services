using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.TagFileSplitter.WebAPI.Middleware
{
  /// <summary>
  /// Project authentication middleware
  /// </summary>
  public class TagFileSplitterAuthentication : TIDAuthentication
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TagFileSplitterAuthentication"/> class.
    /// </summary>
    public TagFileSplitterAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
    }

    /// <summary>
    /// 3dpm specific logic for requiring customerUid
    ///    The TAG file submission end points do not require a customer UID to be provided
    /// </summary>
    public override bool RequireCustomerUid(HttpContext context)
    {
      var isTagFile = context.Request.Path.Value.ToLower().Contains("api/v2/tagfiles");
      
      var containsCustomerUid = context.Request.Headers.ContainsKey("X-VisionLink-CustomerUid");
      if (isTagFile && context.Request.Method == "POST" && !containsCustomerUid)
      {
        log.LogDebug($"{nameof(RequireCustomerUid)} Tagfiles request doesn't require customerUid. path: {context.Request.Path}");
        return false;
      }
      
      return true;
    }
  }
}
