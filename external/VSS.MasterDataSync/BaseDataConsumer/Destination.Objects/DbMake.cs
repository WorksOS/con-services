using System;
using System.Diagnostics.CodeAnalysis;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.Destination.Objects.Interfaces;

namespace VSS.Messaging.BaseDataConsumer.Destination.Objects
{
	[ExcludeFromCodeCoverage]
	public class DbMake : IDbTable
	{
		public static readonly string TableName = "Make";
		public static readonly string IdNames = "MakeUID,MakeCode";
		public static readonly string IgnoreColumns = "ReceivedUTC";

		public Guid MakeUID { get; set; }
		public string MakeCode { get; set; }
		public string MakeDesc { get; set; }
		public DateTime ActionUTC { get; set; }
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
			return typeof(MakeCommand);
		}
	}
}