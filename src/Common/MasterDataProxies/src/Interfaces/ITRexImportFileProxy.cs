using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITRexImportFileProxy
  {
    Task<DesignListResult> GetDesignsForProject(Guid projectUid, IDictionary<string, string> customHeaders = null);
    Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType importedFileType, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);

  }
}