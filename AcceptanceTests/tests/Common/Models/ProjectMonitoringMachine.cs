namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  /// A representation of a machine in a Raptor project
  /// This is copied from ...\RaptorServicesCommon\Models\ProjectMonitoringMachine.cs 
  /// </summary>
  public class ProjectMonitoringMachine
  {
    /// <summary>
    /// The ID of the machine/asset
    /// </summary>
    public long assetID { get; set; }

    /// <summary>
    /// The textual name of the machine
    /// </summary>
    public string machineName { get; set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    public bool isJohnDoe { get; set; }
  }
}