using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Specifies target values for SPeed Summary request
  /// </summary>
  public class MachineSpeedTarget : IValidatable
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

    /// <summary>
    /// Create example instance of LiftBuildSettings to display in Help documentation.
    /// </summary>
    public static MachineSpeedTarget HelpSample
    {
      get
      {
        return new MachineSpeedTarget
        {
          MinTargetMachineSpeed = 10,
          MaxTargetMachineSpeed = 55
        };
      }
    }

  }
}