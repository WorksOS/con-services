using System;

namespace VSS.MasterData.WebAPI.Repository.Device.Helpers
{
	public static class MySqlExtensions
	{
		#region  Public Methods

		public static string ToStringWithoutHyphens(this Guid meGuid)
		{
			return meGuid.ToString("N");
		}

		#endregion  Public Methods
	}
}
