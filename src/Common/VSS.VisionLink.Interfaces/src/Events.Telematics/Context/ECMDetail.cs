namespace VSS.VisionLink.Interfaces.Events.Telematics.Context
{
  public class EcmDetail
  {
    public string Datalink { get; set; }
    public string Identifier { get; set; }  // could be MID or J1939 name
    public int? SourceAddress { get; set; } // if we have this, but not Identifier, ECM Address Claim processing is required
  }
}
