using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// 
  /// </summary>
  public class TIDCustomPrincipal : ClaimsPrincipal
  {
    private readonly IProjectListProxy ProjectProxy;
    private readonly IDictionary<string, string> ContextHeaders;


    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="T:VSS.Productivity3D.Filter.WebApi.Filters.TIDCustomPrincipal" /> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="customerName"></param>
    /// <param name="isApplication">if set to <c>true</c> [is application].</param>
    /// <param name="projectProxy">Project proxy to use</param>
    public TIDCustomPrincipal(ClaimsIdentity identity, string customerUid, string emailAddress, string customerName,
      IProjectListProxy projectProxy, IDictionary<string, string> contextHeaders, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      EmailAddress = emailAddress;
      IsApplication = isApplication;
      CustomerName = customerName;
      ProjectProxy = projectProxy;
      ContextHeaders = contextHeaders;
    }

    /// <summary>
    /// Gets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string CustomerUid { get; }

    /// <summary>
    /// Gets the customer name.
    /// </summary>
    /// <value>
    /// The customer name.
    /// </value>
    public string CustomerName { get; }


    /// <summary>
    /// Gets the email address.
    /// </summary>
    /// <value>
    /// The email address.
    /// </value>
    public string EmailAddress { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is application.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is application; otherwise, <c>false</c>.
    /// </value>
    public bool IsApplication { get; }


    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectUid">The project ID</param>
    /// <returns>Project descriptor</returns>
    public ProjectData GetProject(string projectUid)
    {
      var project = ProjectProxy.GetProjectForCustomer(CustomerUid, projectUid, ContextHeaders).Result;

      if (project != null) { return project; }
  
      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to customer {CustomerUid}:{EmailAddress} or don't have access to the project {projectUid}"));
    }

  }
}