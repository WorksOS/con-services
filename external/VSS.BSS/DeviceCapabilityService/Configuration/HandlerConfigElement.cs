using System.Configuration;

namespace VSS.Nighthawk.DeviceCapabilityService.Configuration
{
  public class HandlerConfigElement : ConfigurationElement
  {
    public HandlerConfigElement(string name)
    {
      Name = name;
    }

    public HandlerConfigElement()
    {
      Name = "UnknownDeviceHandler";
    }

    [ConfigurationProperty(
      name:"name", 
      IsRequired = true, 
      IsKey = true)]
    public string Name
    {
        get
        {
            return (string)this["name"];
        }
        set
        {
            this["name"] = value;
        }
    }

    [ConfigurationProperty("outboundEndpoints")]
    public EndpointConfigCollection OutboundEndpoints
    {
      get
      {
        var endpoints = (EndpointConfigCollection)base["outboundEndpoints"];
        return endpoints;
      }
    }
  }
}
