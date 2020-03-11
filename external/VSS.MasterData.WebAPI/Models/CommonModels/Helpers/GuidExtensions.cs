using System;
using System.Collections.Generic;
using System.Text;

namespace CommonModel.Helpers
{
    public static class GuidExtensions
    {
        public static byte[] GetByteArrayFromGuid(this Guid guid)
        {
            return FlipEndian(guid.ToByteArray());
        }

        public static Guid GetGuidFromByteArray(byte[] bytes)
        {
            return new Guid(FlipEndian(bytes));
        }

        private static byte[] FlipEndian(IList<byte> oldBytes)
        {
            var newBytes = new byte[16];
            for (var i = 8; i < 16; i++)
                newBytes[i] = oldBytes[i];

            newBytes[3] = oldBytes[0];
            newBytes[2] = oldBytes[1];
            newBytes[1] = oldBytes[2];
            newBytes[0] = oldBytes[3];
            newBytes[5] = oldBytes[4];
            newBytes[4] = oldBytes[5];
            newBytes[6] = oldBytes[7];
            newBytes[7] = oldBytes[6];

            return newBytes;
        }

        public static string ToStringWithoutHyphens(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static string ToStringWithoutHyphens(this string guid)
        {
            return guid.Replace("-", string.Empty);
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
