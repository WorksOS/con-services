namespace VSS.MasterData.WebAPI.Utilities.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// use while reading value: converts blob to guid 
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string WrapWithHex(this string val)
		{
			return $"HEX('{val}')";
		}

		/// <summary>
		/// use in inserts and in where clauses: converts guid to blob
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string WrapWithUnhex(this string val)
		{
			return $"UNHEX('{val}')";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string WrapWithGeomFromText(this string val)
		{
			return $"ST_GeomFromText('{val}')";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static string EscapeQuotesFromText(this string val)
		{
			var valueReplacedWithSlash = val.Replace(@"\", @"\\");
			return valueReplacedWithSlash.Replace("'", "''");
		}
	}
}