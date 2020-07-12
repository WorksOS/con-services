using System;
using System.Text;

namespace CoreX.Wrapper.Extensions
{
  public static class StringExtensions
  {
    public static string DecodeFromBase64(this string inputStr) =>
      Convert.TryFromBase64String(inputStr, new Span<byte>(new byte[inputStr.Length]), out var _)
        ? Encoding.UTF8.GetString(Convert.FromBase64String(inputStr))
        : inputStr;
  }
}
