using System;
using System.Linq;

namespace Infrastructure.Common.Helpers
{
	public static class stringExtensions
    {
        public static string WrapCommaSeperatedStringsWithUnhex(this string AssetUIDs)
        {
            string finalstring = string.Empty;
            if (!string.IsNullOrEmpty(AssetUIDs))
            {
                AssetUIDs.Split(',').ToList().ForEach(str => finalstring += "UNHEX('" + Guid.Parse(str).ToStringWithoutHyphens() + "'),");
                return finalstring.Remove(finalstring.Length - 1);
            }
            return finalstring;
        }

        public static string WrapWithUnhex(this string val)
        {
            return string.Format("UNHEX('{0}')", val.Replace("-", ""));
        }

        public static string WrapWithUnhex(this Guid val)
        {
            return string.Format("UNHEX('{0}')", val.ToStringWithoutHyphens());
        }
    }
}
