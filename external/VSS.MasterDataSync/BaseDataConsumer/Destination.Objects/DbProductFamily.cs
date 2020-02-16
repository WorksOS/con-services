using System;
using System.Diagnostics.CodeAnalysis;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.Destination.Objects.Interfaces;

namespace VSS.Messaging.BaseDataConsumer.Destination.Objects
{
	[ExcludeFromCodeCoverage]
	public class DbProductFamily : IDbTable
	{
		public static readonly string TableName = "ProductFamily";
		public static readonly string IdNames = "ProductFamilyUID";
		public static readonly string IgnoreColumns = "ReceivedUTC";

		public string Name { get; set; }
		public string Description { get; set; }
		public Guid ProductFamilyUID { get; set; }
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
			return typeof(ProductFamilyCommand);
		}
	}
}