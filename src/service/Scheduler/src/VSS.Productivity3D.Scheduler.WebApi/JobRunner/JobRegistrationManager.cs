using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public class JobRegistrationManager : IJobRegistrationManager
  {
    public Dictionary<Guid, Type> vssJobs { get { if (_vssJobs == null) LoadJobs(); return _vssJobs; } private set { _vssJobs = value; } }

    private Dictionary<Guid, Type> _vssJobs = null;
    private const string JobIdFieldName = "VSSJOB_UID";

    private readonly ILogger log;

    public JobRegistrationManager (ILoggerFactory loggerFactory)
    {
      log = loggerFactory.CreateLogger<JobRegistrationManager>();
    }

    public Dictionary<Guid, Type> ResolveVssJobs ()
    {
      return vssJobs;
    }

    public string GetQueueName(Type t)
    {
      return t.Name.ToLowerInvariant();
    }

    public string GetQueueName(Guid guid)
    {
      if (vssJobs.ContainsKey(guid))
        return GetQueueName(vssJobs[guid]);

      throw new ArgumentOutOfRangeException("Job Guid is not registered");
    }

    public string GetJobName(Guid jobGuid)
    {
      if (vssJobs.ContainsKey(jobGuid))
        return ResolveVssJobs()[jobGuid].Name;
      else
        throw new ArgumentOutOfRangeException($"Requested VSSJob {jobGuid} is not registered.");
    }


    /// <summary>
    /// Register a VSS job.
    /// </summary>
    /// <param name="uid">Unique id for the job</param>
    /// <param name="type">The type of the job for instantiation</param>
    public void RegisterJob(Guid uid, Type type)
    {
      if (vssJobs == null)
        LoadJobs();

      if (!typeof(IJob).IsAssignableFrom(type))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Type '{type.FullName}' does not implement the {nameof(IJob)} interface, and cannot be registered"));
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
        .Where(x => typeof(IJob).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
        .ToList();

      foreach (var jobClassType in jobClassTypes)
      {
        //Find the VSS job uid and register the job
        var fieldInfo = jobClassType.GetTypeInfo().GetDeclaredField(JobIdFieldName);

        if (fieldInfo != null && fieldInfo.IsStatic && fieldInfo.FieldType == typeof(Guid))
        {
          var v = fieldInfo.GetValue(null);
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
