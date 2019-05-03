using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace VSS.VisionLink.Interfaces.Events.Commands.Helpers
{
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