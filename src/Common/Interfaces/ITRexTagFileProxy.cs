using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

// todo this needs to be moved into Sandbox
namespace VSS.Productivity3D.Common.Interfaces
{
  public interface ITRexTagFileProxy
  {
    Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
