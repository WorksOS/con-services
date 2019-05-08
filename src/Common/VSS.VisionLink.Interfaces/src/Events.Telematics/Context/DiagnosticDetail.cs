namespace VSS.VisionLink.Interfaces.Events.Telematics.Context
{
  public class DiagnosticDetail
  {
    public string Description { get; set; }
    public int Occurrences { get; set; }
    public string PrimaryIdentifier { get; set;}    // FMI for CDL, SPN for J1939
    public string SecondaryIdentifier { get; set; } // CID for CDL, PGN for J1939
    public string SensorIdentifier { get; set; }    // A quick way to reference what sensor is reporting the diagnostic, CID for CDL.
    public string Severity { get; set; }
  }
}
