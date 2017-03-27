using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Filters.Validation;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.JsonConverters;
using VSS.Raptor.Service.Common.ResultHandling;



namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// Raptor data model/project identifier.
  /// </summary>
  public class ProjectID : IValidatable
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
    [ValidProjectID]
    public long? projectId { get; set; }

    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    [ValidProjectUID]    
    public Guid? projectUid { get; protected set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    protected ProjectID()
    {
        // ...
    }

    /// <summary>
    /// ProjectID sample instance.
    /// </summary>
    /// 
    public static ProjectID HelpSample
    {
      get { return new ProjectID() { projectId = 1, projectUid = new Guid()}; }
    }

    /// <summary>
    /// Creates an instance of the ProjectID class.
    /// </summary>
    /// <param name="projectId">The Raptor datamodel & legacy project identifier.</param>
    /// <param name="projectUid">The project UID.</param>
    /// <returns></returns>
    public static ProjectID CreateProjectID(long projectId, Guid? projectUid = null)
    {
      return new ProjectID()
      {
        projectId = projectId,
        projectUid = projectUid
      };
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public virtual void Validate()
    {
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
        var validator = new DataAnnotationsValidator();
        ICollection<ValidationResult> results;
        validator.TryValidate(this, out results);
        if (results.Any())
        {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,results.FirstOrDefault().ErrorMessage));
        }

        if (!projectId.HasValue && !projectUid.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "At least one of the project identifiers must be defined!"));
        }

    }

    public static long GetProjectId(string customerUid, Guid? projectUid, IAuthenticatedProjectsStore authProjectsStore)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }
      if (authProjectsStore == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing authenticated projects store"));
      }
      var projectsByUid = authProjectsStore.GetProjectsByUid(customerUid);
      if (!projectsByUid.ContainsKey(projectUid.ToString()))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing Project or project does not belong to specified customer"));
      }
      long projectId = projectsByUid[projectUid.ToString()].projectId;
      if (projectId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
      }
      return projectId;
    }
  }
}
