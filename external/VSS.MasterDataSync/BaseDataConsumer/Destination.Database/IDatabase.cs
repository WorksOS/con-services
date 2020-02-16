using System.Data;
using System.Data.SqlClient;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public interface IDatabase
	{
		void GetConnectionAndTransaction();
		int ExecuteProcedure(DataTable recordsToSave, DataTable recordsToDelete, string storedProcName);
		int NoOfRecordsAlreadyExists(SqlCommand sqlCommand);
		int InsertOrUpdate(SqlCommand sqlCommand);
		void Commit();
		void Dispose();
		void Rollback();
	}
}