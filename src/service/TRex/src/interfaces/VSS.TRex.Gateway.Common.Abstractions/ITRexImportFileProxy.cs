using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  public interface ITRexImportFileProxy
  {
    Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType? importedFileType, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IDictionary<string, string> customHeaders = null);
  }
}
