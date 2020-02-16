using System.Xml.Serialization;

namespace VSS.Nighthawk.ExternalDataTypes.Enumerations
{
  public enum Monitored
  {
		[XmlEnum(Name = "Always")]
    Always = 0,
		[XmlEnum(Name = "Key Off Engine Off")]
    KeyOffEngineOff = 1,
		[XmlEnum(Name = "Key On Engine Off")]
    KeyOnEngineOff = 2,
		[XmlEnum(Name = "Key On Engine On")]
    KeyOnEngineOn = 3 
  }
}
