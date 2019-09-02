using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The representation of a alignment design station range request.
  /// </summary>
  public class DesignDataRequest : ProjectID
  {
    /// <summary>
    /// The unique identifier of the alignment design to to get station range from.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid DesignUid { get; private set; }

    /// <summary>
    /// The design file name.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string FileName { get; private set; }

    public DesignDataRequest(Guid projectUid, Guid designUid, string fileName = "")
    {
      ProjectUid = projectUid;
      DesignUid = designUid;
      FileName = fileName;
    }

    public override void Validate()
    {
      base.Validate();

      if (DesignUid == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The Design UID must be provided."));
    }
  }
}
