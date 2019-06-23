using System;
using System.Text.RegularExpressions;

namespace VSS.MasterData.Proxies
{
  public static class LogContent
  {
    private static readonly Regex _regexLineEndings = new Regex(@"\r\n?|\n", RegexOptions.Compiled);

    private static readonly Func<string, bool, string> ReplaceLineEndings = (str, replace) => replace ? _regexLineEndings.Replace(str, @"\r\n") : str;

    /// <summary>
    /// Truncates a string to the max length provided.
    /// </summary>
    /// <param name="str">The string to truncate</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <param name="replaceLineEndings">Replace line endings with '\r\n'. Useful if the output needs to be treated as a sinle line, e.g. FluentD.</param>
    public static string Truncate(this string str, int maxLength, bool replaceLineEndings = true)
    {
      if (string.IsNullOrEmpty(str)) return str;

      return str.Length <= maxLength || maxLength <= 0
        ? ReplaceLineEndings(str, replaceLineEndings)
        : ReplaceLineEndings(str.Substring(0, maxLength) + "...", replaceLineEndings);
    }
  }
}
