using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.Data.MySql
{
	public interface IQueryBuilder
	{
		string Build<T>(List<T> values) where T : class;
	}
}            