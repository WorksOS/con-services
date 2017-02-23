using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Models
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
      if (this.MinTargetMachineSpeed > this.MaxTargetMachineSpeed)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Target speed minimum must be less than target speed maximum"));
      }
    }

    public static MachineSpeedTarget CreateMachineSpeedTarget(ushort min, ushort max)
    {
      return new MachineSpeedTarget() { MinTargetMachineSpeed = min, MaxTargetMachineSpeed = max };
    }

    /// <summary>
    /// Create example instance of LiftBuildSettings to display in Help documentation.
    /// </summary>
    public static MachineSpeedTarget HelpSample
    {
      get
      {
        return new MachineSpeedTarget()
        {
          MinTargetMachineSpeed = 10,
          MaxTargetMachineSpeed = 55
        };
      }
    }

  }
}