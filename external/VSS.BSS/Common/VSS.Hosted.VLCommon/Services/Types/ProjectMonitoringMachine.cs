using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
  public class ProjectMonitoringMachine
  {
    public long AssetID;
    public string MachineName;
    public bool IsJohnDoe;

    public ProjectMonitoringMachine()
    {
    }
    
    public ProjectMonitoringMachine(XElement element)
    {
      Parse(element);
    }

    public XElement ToXElement(string elementName)
    {
      XElement element = new XElement(elementName);
      element.Add(new XElement("AssetID", AssetID));
      element.Add(new XElement("MachineName", MachineName));
      element.Add(new XElement("IsJohnDoe", IsJohnDoe));
      return element;
    }

    public void Parse(XElement element)
    {
      long? assetID = element.GetLongElement("AssetID");
      AssetID = assetID.HasValue ? assetID.Value : -1;
      MachineName = element.GetStringElement("MachineName");
      bool? isJohnDoe = element.GetBooleanElement("IsJohnDoe");
      IsJohnDoe = isJohnDoe.HasValue ? isJohnDoe.Value : false;
    }
  }
}
