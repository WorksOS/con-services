using System.Xml.Serialization;

namespace VSS.Nighthawk.ExternalDataTypes.Enumerations
{
  public enum SwitchState
  {
		[XmlEnum(Name = "Not Installed")]
    NotInstalled = 0x00,
		[XmlEnum(Name = "Not Configured")]
		NotConfigured = 0x01,
		[XmlEnum(Name = "Normally Open")]
    NormallyOpen = 0x02,
		[XmlEnum(Name = "Normally Closed")]
    NormallyClosed = 0x04
  }
}
