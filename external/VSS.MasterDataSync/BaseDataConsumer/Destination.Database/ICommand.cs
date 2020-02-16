using System.Collections.Generic;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public interface ICommand
	{
		void SaveDestinationRecordsToDB(List<object> list);
	}
}