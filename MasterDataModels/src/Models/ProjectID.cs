using System;
using Newtonsoft.Json;

namespace MasterDataModels.Models
{
  /// <summary>
  /// Raptor data model/project identifier.
  /// </summary>

  public class ProjectID
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
    public long? projectId { get; set; }
    
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
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
      throw new NotImplementedException();
    }
  }
}
