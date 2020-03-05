using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbPreferenceKey : IDbTable
	{
		public Guid PreferenceKeyUID { get; set; }

		public string PreferenceKeyName { get; set; }

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
			return "md_preference_PreferenceKey";
		}
	}
}