using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// A representation of an edit applied to production data.
  /// </summary>
  public class TRexEditData
  {
    /// <summary>
    /// The id of the machine whose data is overridden. 
    /// </summary>
    [JsonProperty(PropertyName = "assetUid", Required = Required.Always)]
    public Guid AssetUid { get; set; }

    /// <summary>
    /// Start of the period with overridden data. 
    /// </summary>
    [JsonProperty(PropertyName = "startUtc", Required = Required.Always)]
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// End of the period with overridden data. 
    /// </summary>
    [JsonProperty(PropertyName = "endUtc", Required = Required.Always)]
    public DateTime EndUtc { get; set; }


    /// <summary>
    /// The design name used for the specified override period. 
    /// </summary>
    [JsonProperty(PropertyName = "machineDesignName", Required = Required.Default)]
    public string MachineDesignName { get; set; }

    /// <summary>
    /// The lift number used for the specified override period.
    /// </summary>
    [JsonProperty(PropertyName = "liftNumber", Required = Required.Default)]
    public int? LiftNumber { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public TRexEditData(
      Guid assetUid,
      DateTime startUtc,
      DateTime endUtc,
      string machineDesignName,
      int? liftNumber
      )
    {
      AssetUid = assetUid;
      StartUtc = startUtc;
      EndUtc = endUtc;
      MachineDesignName = machineDesignName;
      LiftNumber = liftNumber;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public virtual void Validate()
    {
      if (AssetUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing AssetUid"));
      }

      if (StartUtc == DateTime.MinValue || EndUtc == DateTime.MinValue || StartUtc >= EndUtc)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Invalid override date range"));
      }

      if (string.IsNullOrEmpty(MachineDesignName) && !LiftNumber.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Nothing to edit"));
      }
    }
  }
}
