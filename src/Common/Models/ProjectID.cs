using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Validation;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;



namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// Raptor data model/project identifier.
  /// </summary>
  ///
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
    /// <param name="projectId">The datamodel/project identifier.</param>
    /// <returns></returns>
    /// 
    public static ProjectID CreateProjectID(long projectId, Guid? projectUid = null, IProjectProxy projectProxy = null, IDictionary<string, string> customHeaders = null)
    {
      CheckProjectId(projectUid, ref projectId, projectProxy, customHeaders);

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

    public static void CheckProjectId(Guid? projectUid, ref long projectId, IProjectProxy projectProxy = null, IDictionary<string, string> customHeaders = null)
    {
      if (projectUid.HasValue)
      {
        if (projectProxy == null)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
           new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
             "Missing project proxy to get legacy project ID for project UID"));
        }
        projectId = projectProxy.GetProjectId(projectUid.ToString(), customHeaders);
      }
    }
  }
}
