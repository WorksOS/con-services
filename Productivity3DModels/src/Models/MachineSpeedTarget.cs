using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Specifies target values for SPeed Summary request
  /// </summary>
  public class MachineSpeedTarget 
  {
    /// <summary>
    /// Sets the minimum target machine speed. The value should be specified in cm\sec
    /// </summary>
    /// <value>
    /// The minimum target machine speed.
    /// </value>
    [Range(0, 65535)]
    [JsonProperty(PropertyName = "MinTargetMachineSpeed", Required = Required.Always)]
    [Required]
    public ushort MinTargetMachineSpeed { get; private set; }

    /// <summary>
    /// Sets the maximum target machine speed. The value should be specified in cm\sec
    /// </summary>
    /// <value>
    /// The maximum target machine speed.
    /// </value>
    [Range(0, 65535)]
    [JsonProperty(PropertyName = "MaxTargetMachineSpeed", Required = Required.Always)]
    [Required]
    public ushort MaxTargetMachineSpeed { get; private set; }


    public void Validate()
    {
      if (MinTargetMachineSpeed > MaxTargetMachineSpeed)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Target speed minimum must be less than target speed maximum"));
      }
    }

    public static MachineSpeedTarget CreateMachineSpeedTarget(ushort min, ushort max)
    {
      return new MachineSpeedTarget { MinTargetMachineSpeed = min, MaxTargetMachineSpeed = max };
    }
  }
}