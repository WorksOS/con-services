using System.Configuration;
using System.Linq;

namespace VSS.Nighthawk.DeviceCapabilityService.Configuration
{
  public class HandlerConfigCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new HandlerConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((HandlerConfigElement)element).Name;
    }

    public override ConfigurationElementCollectionType CollectionType
    {
      get { return ConfigurationElementCollectionType.BasicMap; }
    }

    protected override string ElementName
    {
      get { return "handler"; }
    }

    public HandlerConfigElement this[int index]
    {
      get { return (HandlerConfigElement)BaseGet(index); }
      set
      {
        if (BaseGet(index) != null)
        {
          BaseRemoveAt(index);
        }
        BaseAdd(index, value);
      }
    }

    new public HandlerConfigElement this[string name]
    {
      get { return (HandlerConfigElement)BaseGet(name); }
    }

    public bool ContainsKey(string key)
    {
      var keys = BaseGetAllKeys();
      return keys.Any(obj => (string) obj == key);
    }
  } 
}
