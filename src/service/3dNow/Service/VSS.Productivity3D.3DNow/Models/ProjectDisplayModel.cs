using System.Collections.Generic;

namespace VSS.Productivity3D.Now3D.Models
{
  /// <summary>
  /// Represents a 3D Productivity Project
  /// </summary>
  public class ProjectDisplayModel
  {
    public ProjectDisplayModel()
    {
      Files = new List<FileDisplayModel>();
    }
    
    /// <summary>
    /// Unique Identifier for the project
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Project Name as displayed in 3D Productivity
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// List of Files attached to this project
    /// </summary>
    public List<FileDisplayModel> Files { get; set; }

    /// <summary>
    /// True if the project is active, false if the project is archived
    /// </summary>
    public bool IsActive { get; set; }
  }
}