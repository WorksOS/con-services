namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Machine descriptor.
  /// </summary>
  public struct Machine
  {
    /// <summary>
    /// The machine's asset identifier.
    /// </summary>
    public long AssetID { get; set; }

    /// <summary>
    /// The machine's name.
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// The machine's serial number.
    /// </summary>
    public string SerialNo { get; set; }
  }
}
