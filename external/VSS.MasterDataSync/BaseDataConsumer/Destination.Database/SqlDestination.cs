using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.Messaging.Common;
using VSS.Messaging.Common.Interfaces;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public class SqlDestination : IDestination
	{
		private IDatabase database;

		public SqlDestination(IDatabase database)
		{
			this.database = database;
		}

		public void Commit()
		{
			database.Commit();
		}

		public void Dispose()
		{
			database.Dispose();
		}

		public void HandleDestinationRecords(ObjectsByType destinationMessagesByType, CancellationToken cancellationToken)
		{
			try
			{
				foreach ((Type TypeOfObjectsInList, List<object> List) typeAndList in destinationMessagesByType)
				{
					var type = typeAndList.TypeOfObjectsInList;
					var list = typeAndList.List;
					var GetCommand = type.GetMethod("GetCommand", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);

					if (GetCommand == null)
					{
						Processor.logger.LogError("Unable to Cast Message to DB : Missing CallCommand or Invalid Messages");
						continue;
					}

					var commandType = (Type)GetCommand.Invoke(null, null);

					if (typeof(ICommand).IsAssignableFrom(commandType))
					{
						object[] args = { database };
						var obj = Activator.CreateInstance(commandType, database);
						((ICommand)obj).SaveDestinationRecordsToDB(list);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void Rollback()
		{
			database.Rollback();
		}
	}
}