namespace VSS.MasterData.WebAPI.Repository.Device.Helpers
{
	public static class StringExtensions
	{
		#region  Public Methods
		public static string WrapWithUnhex(this string val)
		{
			return $"UNHEX('{val}')";
		}
		#endregion  Public Methods
	}
}
