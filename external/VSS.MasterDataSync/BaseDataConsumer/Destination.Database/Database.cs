using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	[ExcludeFromCodeCoverage]
	public class Database : IDatabase
	{
		private SqlConnection connection;
		private SqlTransaction transaction;
		private IConfiguration configuration;
		private string connectionString;

		public Database(IConfiguration configuration)
		{
			this.configuration = configuration;
			connectionString = configuration["connectionString"];
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new ArgumentNullException("Connection string should not be Empty");
			}
			connectionString = connectionString.Replace("XXXServerXXX", configuration["writeDatabaseServer"]);
			connectionString = connectionString.Replace("XXXUSERXXX", configuration["databaseUser"]);
			connectionString = connectionString.Replace("XXXPASSWORDXXX", configuration["databasePassword"]);
		}

		public virtual int ExecuteProcedure(DataTable recordsToSave, DataTable recordsToDelete, string storedProcName)
		{
			GetConnectionAndTransaction();
			SqlCommand dbCommand = new SqlCommand(storedProcName, connection, transaction);
			dbCommand.CommandType = CommandType.StoredProcedure;
			if (recordsToSave != null)
				dbCommand.Parameters.AddWithValue("@upsertList", recordsToSave);
			if (recordsToDelete != null)
				dbCommand.Parameters.AddWithValue("@deleteList", recordsToDelete);
			return dbCommand.ExecuteNonQuery();
		}

		public virtual int NoOfRecordsAlreadyExists(SqlCommand command)
		{
			command.Connection = connection;
			command.Transaction = transaction;
			return (int)command.ExecuteScalar();
		}

		public virtual int InsertOrUpdate(SqlCommand command)
		{
			command.Connection = connection;
			command.Transaction = transaction;
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Accepts a null or open connection - uses existing or creates new, and begins the transaction (if not already begun)
		/// </summary>
		/// <param name="dbConnection"></param>
		/// <param name="dbTransaction"></param>
		public virtual void GetConnectionAndTransaction()
		{
			if (connection == null)
			{
				connection = new SqlConnection(connectionString);
			}

			if (connection.State == ConnectionState.Closed)
			{
				connection.Open();
			}

			if (transaction == null)
			{
				transaction = connection.BeginTransaction(IsolationLevel.Serializable);
			}
		}

		public virtual void Commit()
		{
			try
			{
				transaction?.Commit();
			}
			finally
			{
				transaction?.Dispose();
				transaction = null;

				connection?.Dispose();
				connection = null;
			}
		}

		public virtual void Dispose()
		{
			try
			{
				transaction?.Dispose();
			}
			finally
			{
				transaction = null;

				connection?.Dispose();
				connection = null;
			}
		}

		public virtual void Rollback()
		{
			try
			{
				transaction?.Rollback();
			}
			finally
			{
				transaction?.Dispose();
				transaction = null;

				connection?.Dispose();
				connection = null;
			}
		}
	}
}