using System.Collections.Generic;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class ActivatedImportFilesRequest
    {
      public IEnumerable<ActivatedFileDescriptor> ImportedFileDescriptors { get; set; }
    }
}