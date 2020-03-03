using System;

namespace VSS.MasterData.WebAPI.Utilities.Extensions
{
	public static class DateTimeExtensions
	{
		public static string ToDateTimeStringWithYearMonthDayFormat(this DateTime dt)
		{
			return dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
		}

		public static DateTime? ToMySqlDateTimeOverflowCorrection(this DateTime? dt)
		{
			if (dt.HasValue)
			{
				var endDate = new DateTime(9999, 12, 31, 23, 59, 59);
				return (dt > endDate) ? endDate : dt;
			}
			return dt;
		}
	}
}