using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VSS.BaseEvents
{
  public class Point
  {
    public double x;
    public double y;

    public Point()
    {
    }

    public Point(double latitude, double longitude)
    {
      this.x = longitude;
      this.y = latitude;
    }

    public double Latitude
    {
      get { return y; }
    }

    public double Longitude
    {
      get { return x; }
    }

    public Point(XElement element)
    {
      Parse(element);
    }

    public XElement ToXElement(string elementName)
    {
      XElement element = new XElement(elementName);
      element.Add(new XElement("Latitude", y));
      element.Add(new XElement("Longitude", x));
      return element;
    }

    public void Parse(XElement element)
    {
      double? doubleElement = element.GetDoubleElement("Latitude");
      y = doubleElement.HasValue ? doubleElement.Value : double.NaN;
      doubleElement = element.GetDoubleElement("Longitude");
      x = doubleElement.HasValue ? doubleElement.Value : double.NaN;
    }

  }

  public static class Extensions
  {

    public static double? GetDoubleElement(this XElement parent, XName element)
    {
      double result = double.NaN;

      IEnumerable<XElement> desc = parent.Descendants(element);

      return (desc.Count() > 0 && double.TryParse(desc.First().Value, out result)) ? result : (double?)null;
    }
  }
}
