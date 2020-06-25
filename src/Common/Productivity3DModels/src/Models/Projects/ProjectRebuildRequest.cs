using System;
using System.Net;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.Models.Projects
{
  public class ProjectRebuildRequest
  { 
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    [ValidProjectUID]
    public Guid ProjectUid { get; set; }

    [JsonProperty(PropertyName = "dataOrigin", Required = Required.Always)]
    public TransferProxyType DataOrigin { get; set; }

    [JsonProperty(PropertyName = "archiveTagFiles", Required = Required.Always)]
    public bool ArchiveTagFiles { get; set; }

    private ProjectRebuildRequest()
    {
    }

    public ProjectRebuildRequest(Guid projectUid, TransferProxyType dataOrigin, bool archiveTagFiles)
    {
      ProjectUid = projectUid;
      DataOrigin = dataOrigin;
      ArchiveTagFiles = archiveTagFiles;
    }

    public void Validate()
    {
      if (!Guid.TryParseExact(ProjectUid.ToString(), "D", out _) || ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "ProjectUid must be provided"));
      }

      if (DataOrigin != TransferProxyType.TAGFiles)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Data origin must be ''TAGFiles'' (enum TransferProxyType.TAGFiles)"));
      }
    }
  }
}
