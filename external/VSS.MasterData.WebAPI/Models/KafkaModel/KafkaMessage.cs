using System;

namespace KafkaModel
{
	public class KafkaMessage
	{
		public object Message { get; set; }

		public string Topic { get; set; }

		public string Key { get; set; }
	}
}
