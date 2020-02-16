using System;

namespace VSS.Messaging.BaseDataConsumer.Destination.Objects
{
	public class SalesModelDto
	{
		public string ModelCode { get; set; }
		public string SerialNumberPrefix { get; set; }
		public long? StartRange { get; set; }
		public long? EndRange { get; set; }
		public string Description { get; set; }
		public Guid? IconUID { get; set; }
		public Guid? ProductFamilyUID { get; set; }
		public Guid SalesModelUID { get; set; }
	}
}