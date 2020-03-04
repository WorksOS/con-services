using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Common.Helpers
{
	public static class GuidExtensions
    {
        public static string ToStringWithoutHyphens(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static string ToStringWithoutHyphens(this List<Guid> guids)
        {
            var stringWithoutHypens = new StringBuilder();
            guids.ForEach(guid => stringWithoutHypens.Append(guid.ToString("N") + ","));
            stringWithoutHypens.Remove(stringWithoutHypens.Length - 1, stringWithoutHypens.Length);
            return stringWithoutHypens.ToString();
        }
    }
}
