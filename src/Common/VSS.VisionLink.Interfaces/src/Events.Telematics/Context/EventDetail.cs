namespace VSS.VisionLink.Interfaces.Events.Telematics.Context
{
  public class EventDetail
  {
    public string Description { get; set; }
    public string Identifier { get; set; } // Changed type from int to string so we can handle alphanumeric Event Identifiers.
    public int Occurrences { get; set; }
    public string Severity { get; set; }
  }
}
