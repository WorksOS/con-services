using Newtonsoft.Json;
using System;


namespace VSS.TagFileAuth.Service.WebApiModels.RaptorServicesCommon
{
  /// <summary>
  /// Raptor data model/project identifier.
  /// </summary>
  ///
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
    /// ProjectID sample instance.
    /// </summary>
    /// 
    public static ProjectID HelpSample
    {
      get { return new ProjectID() { projectId = 1, projectUid = new Guid() }; }
    }

    /// <summary>
    /// Creates an instance of the ProjectID class.
    /// </summary>
    /// <param name="projectId">The datamodel/project identifier.</param>
    /// <returns></returns>
    /// 
    public static ProjectID CreateProjectID(long projectId, Guid? projectUid = null)
    {
      // CheckProjectId(projectUid, ref projectId);

      return new ProjectID()
      {
        projectId = projectId,
        projectUid = projectUid
      };
    }

    ///// <summary>
    ///// Validation method.
    ///// </summary>
    //public virtual void Validate()
    //{

    //  //if (results.Any())
    //  //{
    //  //  throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
    //  //}

    //  //if (!projectId.HasValue && !projectUid.HasValue)
    //  //{
    //  //  throw new ServiceException(HttpStatusCode.BadRequest,
    //  //    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
    //  //      "At least one of the project identifiers must be defined!"));
    //  //}

    //}

    //public static void CheckProjectId(Guid? projectUid, ref long projectId)
    //{
    //  //if (projectUid.HasValue)
    //  //  projectId = RaptorDb.GetProjectID(projectUid.ToString());
    //}
  }
}
