using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceConfigRepository.Helpers
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
