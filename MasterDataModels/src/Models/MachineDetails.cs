using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.FIlters;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A representation of a machine in a Raptor project
  /// </summary>
  public class MachineDetails
  {
    /// <summary>
    /// The ID of the machine/asset. This is the unique identifier, used by Raptor.
    /// </summary>
    [JsonProperty(PropertyName = "assetID", Required = Required.Always)]
    public long assetID { get; protected set; }

    /// <summary>
    /// The textual name of the machine. This is the human readable machine name from the machine control display, and written in tagfiles.
    /// </summary>
    [MaxLength(MAX_MACHINE_NAME)]
    [NameValidation]
    [JsonProperty(PropertyName = "machineName", Required = Required.Always)]
    public string machineName { get; protected set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    [JsonProperty(PropertyName = "isJohnDoe", Required = Required.Always)]
    public bool isJohnDoe { get; protected set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    protected MachineDetails()
    { }

    /// <summary>
    /// Create instance of MachineDetails
    /// </summary>
    public static MachineDetails CreateMachineDetails(
        long assetID,
        string machineName,
        bool isJohnDoe
        )
    {
      return new MachineDetails
      {
        assetID = assetID,
        machineName = machineName,
        isJohnDoe = isJohnDoe
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }

    private const int MAX_MACHINE_NAME = 256;
  }
}