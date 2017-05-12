using System;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

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
    public string UserPreferenceUnits { get; private set; }

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
      string userUnits
    )
    {
      return new ProjectFileDescriptor
      {
        projectId = projectId,
        projectUid = projectUId,
        File = file,
        CoordSystemFileName = coordSystemFileName,
        UserPreferenceUnits = userUnits
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      base.Validate();
      this.File.Validate();
    }
  }
}
