using System.Text;

namespace CoreX.Wrapper.Extensions
{
  public static class StringExtensions
  {
    public static string DecodeFromBase64(this string encodedStr) => Encoding.UTF8.GetString(System.Convert.FromBase64String(encodedStr));
  }
}
