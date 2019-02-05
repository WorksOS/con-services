using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Now3D.Models
{
  /// <summary>
  /// Represents a File attached to a project in 3D Productivity
  /// </summary>
  public class FileDisplayModel
  {
    /// <summary>
    /// Unique Identifier for the file
    /// </summary>
    public string FileUid { get; set; }

    /// <summary>
    /// File name for the file
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Imported File Type Enumeration
    /// </summary>
    public ImportedFileType FileType { get; set; }

    /// <summary>
    /// Display string for the Imported File Type Enumeration
    /// </summary>
    public string FileTypeName { get; set; }
  }
}