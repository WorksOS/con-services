using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request to edit production data
  /// </summary>
  public class TRexEditDataRequest : TRexEditData
  {
    /// <summary>
    /// The id of the machine whose data is overridden. 
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public TRexEditDataRequest(
      Guid projectUid,
      Guid assetUid,
      DateTime startUtc,
      DateTime endUtc,
      string machineDesignName,
      int? liftNumber
    ) : base (assetUid, startUtc, endUtc, machineDesignName, liftNumber)
    {
      ProjectUid = projectUid;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing ProjectUid"));
      }
    }

  }
}
