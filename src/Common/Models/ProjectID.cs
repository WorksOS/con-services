using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
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
    /// ProjectID sample instance.
    /// </summary>
    /// 
    public static ProjectID HelpSample => new ProjectID { projectId = 1, projectUid = new Guid() };

    /// <summary>
    /// Creates an instance of the ProjectID class.
    /// </summary>
    /// <param name="projectId">The Raptor datamodel & legacy project identifier.</param>
    /// <param name="projectUid">The project UID.</param>
    /// <returns></returns>
    public static ProjectID CreateProjectID(long projectId, Guid? projectUid = null)
    {
      return new ProjectID
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
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }

      if (!projectId.HasValue && !projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one of the project identifiers must be defined!"));
      }
    }
  }
}