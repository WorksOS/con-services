using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {
    /// <summary>
    /// Validates if there any subscriptions available for the request create project event
    /// </summary>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="customHeaders"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="projectRepo"></param>
    /// <param name="projectUid"></param>
    /// <param name="projectType"></param>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public static async Task<string> AssociateProjectSubscriptionInSubscriptionService(string projectUid,
      ProjectType projectType, string customerUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> customHeaders,
      ISubscriptionProxy subscriptionProxy, ISubscriptionRepository subscriptionRepo, IProjectRepository projectRepo,
      bool isCreate)
    {
      string subscriptionUidAssigned = null;
      if (projectType == ProjectType.LandFill || projectType == ProjectType.ProjectMonitoring)
      {
        subscriptionUidAssigned = (await subscriptionRepo
            .GetFreeProjectSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date)
            .ConfigureAwait(false))
          .FirstOrDefault(s => s.ServiceTypeID == (int) projectType.MatchSubscriptionType())
          ?.SubscriptionUID;

        if (String.IsNullOrEmpty(subscriptionUidAssigned))
        {
          log.LogInformation($"There are no free subscriptions for project type {projectType}");
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 37);
        }

        //Assign the new project to a subscription
        try
        {
          // rethrows any exception
          await subscriptionProxy.AssociateProjectSubscription(Guid.Parse(subscriptionUidAssigned),
            Guid.Parse(projectUid), customHeaders).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          if (isCreate)
            await ProjectRequestHelper
              .DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), Guid.Parse(projectUid), log, projectRepo)
              .ConfigureAwait(false);

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "SubscriptionProxy.AssociateProjectSubscriptionInSubscriptionService", e.Message);
        }
      }

      return subscriptionUidAssigned;
    }
  }
}
