using System;

namespace VSS.MasterData.WebAPI.Utilities.Extensions
{
	public static class GuidExtensions
	{
		#region Public Static Methods

		public static string ToStringWithoutHyphens(this Guid meGuid)
		{
			return meGuid.ToString("N");
		}
		public static bool ValidateForEmpty(this Guid meGuid)
		{
			return meGuid == Guid.Empty || meGuid == null;
		}
		public static bool ValidateForEmpty(this Guid? meGuid)
		{
			return meGuid == Guid.Empty || meGuid == null;
		}
		public static string ToStringAndWrapWithUnhex(this Guid meGuid)
		{
			return $"UNHEX('{meGuid.ToString("N")}')";
		}
		#endregion
	}
}
