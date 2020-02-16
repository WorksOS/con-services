using System;

namespace VSS.Messaging.BaseDataConsumer.Destination.Objects
{
	public class ProductFamilyDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Guid ProductFamilyUID { get; set; }
	}
}