using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileForwarder
{
  public interface ITagFileForwarder
  {
    bool IsEnabled { get; }
    Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> SendTagFileNonDirect(CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
