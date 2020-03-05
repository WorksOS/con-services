using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Common.Constants
{
	public class Constants
	{
		public const string VISIONLINK_CUSTOMERUID = "X-VisionLink-CustomerUid";
		public const string VISIONLINK_USERUID = "X-VisionLink-UserUid";
		public const string USERUID_API = "UserUID_IdentityAPI";
	}

	//public static class Extensions
	//{
	//	public static string ToStringWithoutHyphens(this Guid guid)
	//	{
	//		return guid.ToString("N");
	//	}

	//	public static string ToStringWithoutHyphens(this string guid)
	//	{
	//		return new Guid(guid).ToString("N");
	//	}

	//	public static string WrapWithUnhex(this string val)
	//	{
	//		return string.Format("UNHEX('{0}')", val);
	//	}
	//}
}
