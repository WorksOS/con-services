using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbUserPreference : IDbTable
	{
		public Guid? fk_UserUID { get; set; }

		public long fk_PreferenceKeyID { get; set; }

		public string PreferenceValue { get; set; }

		public string SchemaVersion { get; set; }

		public DateTime InsertUTC { get; set; }

		public DateTime UpdateUTC { get; set; }

		public string GetIdColumn()
		{
			return null;
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return "InsertUTC";
		}

		public string GetTableName()
		{
			return "md_preference_PreferenceUser";
		}
	}
}