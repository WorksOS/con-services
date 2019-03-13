using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// For managing VSS jobs
  /// </summary>
  public class VSSJobFactory : IVSSJobFactory
  {
    private readonly ILogger log;
    private readonly IServiceProvider serviceProvider;
    private Dictionary<Guid, Type> vssJobs;

    /// <summary>
    /// Constructor
    /// </summary>
    public VSSJobFactory(ILoggerFactory logger, IServiceProvider serviceProvider)
    {
      log = logger.CreateLogger<VSSJobFactory>();
      this.serviceProvider = serviceProvider;
    }


    /// <summary>
    /// Gets the VSS job with given UID. JOb must be registered first.
    /// </summary>
    public IVSSJob GetJob(Guid jobUid)
    {
      if (vssJobs == null)
        LoadJobs();

      if (!vssJobs.ContainsKey(jobUid))
      {
        throw new ArgumentException($"Job ID {jobUid} is not registered.");
      }

      return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, vssJobs[jobUid]) as IVSSJob;
    }


    /// <summary>
    /// Register a VSS job.
    /// </summary>
    /// <param name="uid">Unique id for the job</param>
    /// <param name="type">The type of the job for instantiation</param>
    public void RegisterJob(Guid uid, Type type)
    {
      if(vssJobs == null)
        LoadJobs();

      if (!typeof(IVSSJob).IsAssignableFrom(type))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Type '{type.FullName}' does not implement the {nameof(IVSSJob)} interface, and cannot be registered"));
      }

      if (vssJobs.ContainsKey(uid))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Job {uid} is already registered"));
      }

      log.LogInformation($"Registered Job Type {type.Name} with ID {uid}");

      vssJobs.Add(uid, type);
    }


    /// <summary>
    /// Loads all VSS jobs and registers them
    /// </summary>
    private void LoadJobs()
    {
      vssJobs = new Dictionary<Guid, Type>();

      var jobClassTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => typeof(IVSSJob).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
        .ToList();

      foreach (var jobClassType in jobClassTypes)
      {
        //Find the VSS job uid and register the job
        var field = jobClassType.GetTypeInfo().GetField("VSSJobUid", BindingFlags.Public | BindingFlags.Static);
        if (field != null)
        {
          var v = field.GetValue(null);
          try
          {
            RegisterJob((Guid)v, jobClassType);
          }
          catch (ServiceException se)
          {
            log.LogWarning($"Failed to load VSS job {v} from {jobClassType.Name}");
          }
        }
        else
        {
          log.LogWarning($"VSS job type {jobClassType.Name} not loaded due to missing VSSJobUid");
        }
      }
    }
  }
}