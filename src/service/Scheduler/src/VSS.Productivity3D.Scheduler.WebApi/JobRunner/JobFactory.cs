using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{

  /// <summary>
  /// For managing VSS jobs
  /// </summary>
  public class JobFactory : IJobFactory
  {
    private readonly IServiceProvider serviceProvider;
    private readonly IJobRegistrationManager jobManager;

    /// <summary>
    /// Constructor
    /// </summary>
    public JobFactory(IServiceProvider serviceProvider, IJobRegistrationManager jobManager)
    {
      this.serviceProvider = serviceProvider;
      this.jobManager = jobManager;
    }

    /// <summary>
    /// Gets the VSS job with given UID. JOb must be registered first.
    /// </summary>
    public IJob GetJob(Guid jobUid)
    {
      if (!jobManager.ResolveVssJobs().ContainsKey(jobUid))
      {
        throw new ArgumentException($"Job ID {jobUid} is not registered.");
      }

      return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, jobManager.ResolveVssJobs()[jobUid]) as IJob;
    }
  }
}
