using System.Collections.Generic;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class ActivatedImportFilesRequest
  {
    public IEnumerable<ActivatedFileDescriptor> ImportedFileDescriptors { get; set; }
  }
}
