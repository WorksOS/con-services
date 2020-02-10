using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceConfigRepository.Helpers
{
    public static class MySqlExtensions
    {
        public static string ToStringWithoutHyphens(this Guid meGuid)
        {
            return meGuid.ToString("N");
        }

		public static string ToMySQLDateTime(this DateTime meDateTime)
		{
			return meDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
		}

	}
}
