
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace VSS.Raptor.Service.Common.Models
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
    [Required]
    public long assetID { get; protected set; }

    /// <summary>
    /// The textual name of the machine. This is the human readable machine name from the machine control display, and written in tagfiles.
    /// </summary>
    [MaxLength(MAX_MACHINE_NAME)] 
    [JsonProperty(PropertyName = "machineName", Required = Required.Always)]
    [Required]
    public string machineName { get; protected set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    [JsonProperty(PropertyName = "isJohnDoe", Required = Required.Always)]
    [Required]
    public bool isJohnDoe { get; protected set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    protected MachineDetails()
    {}

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
    /// Create example instance of MachineDetails to display in Help documentation.
    /// </summary>
    public static MachineDetails HelpSample
    {
      get
      {
        return new MachineDetails()
        {
          assetID = 1137642418461469,
          machineName = "VOLVO G946B",
          isJohnDoe = false
        };
      }
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
