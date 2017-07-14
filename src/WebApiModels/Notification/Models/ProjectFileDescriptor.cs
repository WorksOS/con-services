using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiModels.Notification.Models
{
  /// <summary>
  /// Request representation fior file notifications
  /// </summary>
  public class ProjectFileDescriptor : ProjectID
  {
    /// <summary>
    /// The file details
    /// </summary>
    [JsonProperty(PropertyName = "file", Required = Required.Always)]
    [Required]
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
    [Required]
    public long FileId { get; private set; }

    /// <summary>
    /// Type of the imported file
    /// </summary>
    [JsonIgnore]
    public ImportedFileType FileType { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProjectFileDescriptor()
    { }

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
      long fileId,
      ImportedFileType fileType = ImportedFileType.DesignSurface
    )
    {
      return new ProjectFileDescriptor
      {
        projectId = projectId,
        projectUid = projectUId,
        File = file,
        CoordSystemFileName = coordSystemFileName,
        UserPreferenceUnits = userUnits,
        FileId = fileId,
        FileType = fileType
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      File.Validate();

      if (FileId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File Id is required"));
      }
    }
  }
}