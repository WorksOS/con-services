using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using WebApiModels.Notification.Models;

namespace VSS.Raptor.Service.WebApiModels.Notification.Models
{
  /// <summary>
  /// Request representation fior file notifications
  /// </summary>
  public class ProjectFileDescriptor : ProjectID, IValidatable
  {
    /// <summary>
    /// The file details
    /// </summary>
    [JsonProperty(PropertyName = "file", Required = Required.Always)]
    public FileDescriptor File { get; private set; }

    /// <summary>
    /// Project coordinate system file name
    /// </summary>
    [JsonProperty(PropertyName = "coordSystemFileName", Required = Required.Default)]
    public string CoordSystemFileName { get; private set; }

    /// <summary>
    /// User units preference
    /// </summary>
    [JsonProperty(PropertyName = "userPreferenceUnits", Required = Required.Default)]
    public UnitsTypeEnum UserPreferenceUnits { get; private set; }

    /// <summary>
    /// A unique file identifier
    /// </summary>
    [JsonProperty(PropertyName = "fileId", Required = Required.Always)]
    public long FileId { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectFileDescriptor()
    {
    }

    /// <summary>
    /// Create instance of ProjectFileDescriptor using ID
    /// </summary>
    public static ProjectFileDescriptor CreateProjectFileDescriptor
    (
      long? projectId,
      Guid? projectUId,
      FileDescriptor file,
      string coordSystemFileName,
      UnitsTypeEnum userUnits,
      long fileId
    )
    {
      return new ProjectFileDescriptor
      {
        projectId = projectId,
        projectUid = projectUId,
        File = file,
        CoordSystemFileName = coordSystemFileName,
        UserPreferenceUnits = userUnits,
        FileId = fileId
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      base.Validate();
      this.File.Validate();

      if (FileId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File Id is required"));
      }
    }
  }
}
