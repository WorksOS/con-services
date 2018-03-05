using Newtonsoft.Json;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Raptor data model/project identifier.
  /// </summary>
  public class ProjectUID
  {
    /// <summary>
    /// The id of the projectUid whose settings are to be upserted
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public string projectUid { get; set; }


    /// <summary>
    /// ProjectID sample instance.
    /// </summary>
    /// 
    public static ProjectUID HelpSample => new ProjectUID { projectUid = new Guid().ToString() };

    /// <summary>
    /// Creates an instance of the ProjectID class.
    /// </summary>
    /// <param name="projectId">The Raptor datamodel & legacy project identifier.</param>
    /// <param name="projectUid">The project UID.</param>
    /// <returns></returns>
    public static ProjectUID CreateProjectUID(string projectUid)
    {
      return new ProjectUID
      {
        projectUid = projectUid
      };
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    public virtual void Validate()
    {
      if (string.IsNullOrEmpty(projectUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2005, "Missing ProjectUID."));
      }
    }
  }
}
