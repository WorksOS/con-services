using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public class MakeCommand : ICommand
	{
		private IDatabase database;

		public MakeCommand(IDatabase database)
		{
			this.database = database;
		}

		public void SaveDestinationRecordsToDB(List<object> list)
		{
			List<DbMake> saveToDBRecords = new List<DbMake>();

			list.ForEach(item =>
			{
				if (!saveToDBRecords.Exists(i => i.MakeUID == ((DbMake)item).MakeUID))
				{
					saveToDBRecords.Add((DbMake)item);
				}
			}
			);

			ExecuteMakeQueries(saveToDBRecords);
		}

		public void ExecuteMakeQueries(List<DbMake> recordsToSave)
		{
			database.GetConnectionAndTransaction();
			foreach (var saveRecords in recordsToSave)
			{
				SqlCommand sqlCommand = GetCommandWithParameters(saveRecords);
				string selectMakeQuery = "Select Count(*) from Make where Code=@makeCode OR MakeUID=@makeUID";
				sqlCommand.CommandText = selectMakeQuery;
				int noOfRowsAffected = database.NoOfRecordsAlreadyExists(sqlCommand);
				if (noOfRowsAffected == 1)
				{
					string updateMakeQuery = "Update Make set Name=@makeDesc,UpdateUTC=GETUTCDATE() where Code=@makeCode AND MakeUID=@makeUID";
					sqlCommand.CommandText = updateMakeQuery;
					if (database.InsertOrUpdate(sqlCommand) == 0)
						Processor.logger.LogDebug("MakeEvent trying to Persist Duplicate : MakeUID {0} or Code {1}", saveRecords.MakeUID, saveRecords.MakeCode);
				}
				else if (noOfRowsAffected == 0)
				{
					string insertMakeQuery = "Insert into Make(Code, Name, MakeUID, UpdateUTC) values(@makeCode,@makeDesc,@makeUID,GETUTCDATE())";
					sqlCommand.CommandText = insertMakeQuery;
					database.InsertOrUpdate(sqlCommand);
				}
				else
				{
					Processor.logger.LogDebug("Make Already Exists : MakeUID {0} Code {1}", saveRecords.MakeUID, saveRecords.MakeCode);
				}
			}
		}

		private SqlCommand GetCommandWithParameters(DbMake make)
		{
			SqlCommand command = new SqlCommand();
			command.Parameters.Add("@makeCode", SqlDbType.VarChar);
			command.Parameters["@makeCode"].Value = make.MakeCode;
			command.Parameters.Add("@makeUID", SqlDbType.UniqueIdentifier);
			command.Parameters["@makeUID"].Value = make.MakeUID;
			command.Parameters.Add("@makeDesc", SqlDbType.NVarChar);
			command.Parameters["@makeDesc"].Value = make.MakeDesc;
			return command;
		}
	}
}