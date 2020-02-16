using System.Configuration;

namespace VSS.Nighthawk.DeviceCapabilityService.Configuration
{
  public class EndpointConfigElement : ConfigurationElement
  {
    [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
    public string Name
    {
      get
      {
        return this["name"] as string;
      }
      set
      {
        this["name"] = value;
      }
    }
  }  
}