using System;
using System.Diagnostics.CodeAnalysis;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.Destination.Objects.Interfaces;

namespace VSS.Messaging.BaseDataConsumer.Destination.Objects
{
	[ExcludeFromCodeCoverage]
	public class DbSalesModel : IDbTable
	{
		public static readonly string TableName = "SalesModel";
		public static readonly string IdNames = "SalesModelUID";
		public static readonly string IgnoreColumns = "ReceivedUTC,IsDelete";

		public string ModelCode { get; set; }
		public string SerialNumberPrefix { get; set; }
		public long? StartRange { get; set; }
		public long? EndRange { get; set; }
		public string Description { get; set; }
		public Guid? IconUID { get; set; }
		public Guid? ProductFamilyUID { get; set; }
		public Guid SalesModelUID { get; set; }
		public bool IsDelete { get; set; }
		public DateTime ReceivedUTC { get; set; }

		public string ColumnsToIgnoreOnUpsert()
		{
			return IgnoreColumns;
		}

		public string GetDbTableName()
		{
			return TableName;
		}

		public string GetIdName()
		{
			return IdNames;
		}

		public static Type GetCommand()
		{
			return typeof(SalesModelCommand);
		}
	}
}