using System.Collections.Generic;

namespace VSS.Productivity3D.ProjectWebApiCommon.Models
{
  public class ActivatedImportFilesRequest
    {
      public IEnumerable<ActivatedFileDescriptor> ImportedFileDescriptors { get; set; }
    }
}