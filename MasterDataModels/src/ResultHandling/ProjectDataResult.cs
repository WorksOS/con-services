using System.Collections.Generic;
using MasterDataModels.Models;

namespace MasterDataModels.ResultHandling
{
  public class ProjectDataResult
  {
    /// <summary>
    ///   Defines machine-readable code.
    /// </summary>
    /// <value>
    ///   Result code.
    /// </value>
    public int Code { get; protected set; }

    /// <summary>
    ///   Defines user-friendly message.
    /// </summary>
    /// <value>
    ///   The message string.
    /// </value>
    public string Message { get; protected set; }

    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public List<ProjectData> ProjectDescriptors { get; set; }
  }
}
