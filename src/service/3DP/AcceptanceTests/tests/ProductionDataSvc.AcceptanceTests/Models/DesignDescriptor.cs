namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Description to identify a design file either by id or by its location in TCC.
  /// </summary>
  public class DesignDescriptor
  {
    /// <summary>
    /// The id of the design file
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// The description of where the file is located.
    /// </summary>
    public FileDescriptor file { get; set; }

    /// <summary>
    /// The offset in meters to use for a reference surface. The surface in the file will be offset by this amount.
    /// Only applicable when the file is a surface design file.
    /// </summary>
    public double offset { get; set; }
  }
}
