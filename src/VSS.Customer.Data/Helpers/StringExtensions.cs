namespace VSS.Customer.Data.Helpers
{
	public static class StringExtensions
	{

		public static string WrapWithHex(this string val)
		{
			return string.Format("HEX('{0}')", val);
		}

		public static string WrapWithUnhex(this string val)
		{
			return string.Format("UNHEX('{0}')", val);
		}

		public static string WrapWithGeomFromText(this string val)
		{
			return string.Format("ST_GeomFromText('{0}')", val);
		}
	}
}
