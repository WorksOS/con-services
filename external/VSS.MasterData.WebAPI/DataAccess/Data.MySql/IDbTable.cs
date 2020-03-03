using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public interface IDbTable
	{
		string GetTableName();
		string GetIdColumn();
		string GetIgnoreColumnsOnUpdate();
		//Dictionary<string,string> GetColumnNames(); //Please return null
	}
}
