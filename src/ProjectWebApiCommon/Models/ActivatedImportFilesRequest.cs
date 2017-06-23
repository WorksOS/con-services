using System.Collections.Generic;

namespace ProjectWebApiCommon.Models
{
  public class ActivatedImportFilesRequest
    {
      public IEnumerable<ActivatedFileDescriptor> ImportedFileDescriptors { get; set; }
    }
}