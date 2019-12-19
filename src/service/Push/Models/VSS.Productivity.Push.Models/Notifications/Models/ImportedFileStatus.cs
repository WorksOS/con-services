using System;
using System.Net;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity.Push.Models.Notifications.Models
{
  public class ImportedFileStatus : ContractExecutionResult
  {
    public Guid ProjectUid { get; protected set; }

    public Guid? FileUid { get; protected set; }

    public ImportedFileStatus(Guid projectUid, Guid? fileUid = null, int code = (int) HttpStatusCode.OK, string message = "success" )
    {
      ProjectUid = projectUid;
      FileUid = fileUid;
      Code = code;
      Message = message;
    }
  }
}
