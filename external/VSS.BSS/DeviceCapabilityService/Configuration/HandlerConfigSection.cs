using System.Configuration;

namespace VSS.Nighthawk.DeviceCapabilityService.Configuration
{
  public class HandlerConfigSection : ConfigurationSection
  {
    [ConfigurationProperty("handlers")]
    public HandlerConfigCollection HandlerConfigs
    {
      get { return this["handlers"] as HandlerConfigCollection; }
    }
  }
}
