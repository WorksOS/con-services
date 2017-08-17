using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Extensions
{
  /// <summary>
  /// Extension methods for the <see cref="FileData"/> type.
  /// </summary>
  public static class FileDataExtensions
  {
    /// <summary>
    /// Validates the <see cref="FileData.ImportedFileType"/> is supported by the profiler.
    /// </summary>
    /// <param name="fileData">The reciever object to validate <see cref="FileData.ImportedFileType"/> against.</param>
    /// <returns>Boolean value reflecting whether the input <see cref="ImportedFileType"/> is supported or not.</returns>
    public static bool IsProfileSupportedFileType(this FileData fileData)
    {
      switch (fileData.ImportedFileType)
      {
        case ImportedFileType.DesignSurface:
        case ImportedFileType.SurveyedSurface:
          return true;
        default:
          return false;
      }
    }
  }
}