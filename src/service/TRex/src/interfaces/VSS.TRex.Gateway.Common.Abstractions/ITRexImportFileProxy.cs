using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  public interface ITRexImportFileProxy
  {
    Task<DesignListResult> GetDesignsOfTypeForProject(Guid projectUid, ImportedFileType? importedFileType, IHeaderDictionary customHeaders = null);
    Task<ContractExecutionResult> AddFile(DesignRequest designRequest, IHeaderDictionary customHeaders = null);
    Task<ContractExecutionResult> UpdateFile(DesignRequest designRequest, IHeaderDictionary customHeaders = null);
    Task<ContractExecutionResult> DeleteFile(DesignRequest designRequest, IHeaderDictionary customHeaders = null);
  }
}
