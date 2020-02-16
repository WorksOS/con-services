using System.Configuration;
using System.Linq;

namespace VSS.Nighthawk.DeviceCapabilityService.Configuration
{
  public class EndpointConfigCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new EndpointConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((EndpointConfigElement)element).Name;
    }

    public override ConfigurationElementCollectionType CollectionType
    {
      get { return ConfigurationElementCollectionType.BasicMap; }
    }

    protected override string ElementName
    {
      get { return "endpoint"; }
    }

    public EndpointConfigElement this[int index]
    {
      get { return (EndpointConfigElement)BaseGet(index); }
      set
      {
        if (BaseGet(index) != null)
        {
          BaseRemoveAt(index);
        }
        BaseAdd(index, value);
      }
    }

    new public EndpointConfigElement this[string name]
    {
      get { return (EndpointConfigElement)BaseGet(name); }
    }

    public bool ContainsKey(string key)
    {
      var keys = BaseGetAllKeys();
      return keys.Any(obj => (string) obj == key);
    }  
  }
}
