namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// A representation of a machine in a Raptor project.
  /// </summary>
  public class MachineDetails
  {
    /// <summary>
    /// The ID of the machine/asset
    /// </summary>
    public long AssetId { get; set; }

    /// <summary>
    /// The textual name of the machine
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    public bool IsJohnDoe { get; set; }
  }
}
