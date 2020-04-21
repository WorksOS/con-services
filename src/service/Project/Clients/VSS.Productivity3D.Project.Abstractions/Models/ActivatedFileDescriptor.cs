namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class ActivatedFileDescriptor
  {
    /// <summary>
    /// Gets or sets the ImportedFile uid.
    /// </summary>
    /// <value>
    /// Is Activated
    /// </value>
    public string ImportedFileUid { get; set; }

    /// <summary>
    /// Gets or sets the Activation State of the imported file.
    /// </summary>
    public bool IsActivated { get; set; }
  }
}
