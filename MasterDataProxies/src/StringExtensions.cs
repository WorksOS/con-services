namespace VSS.MasterData.Proxies
{
  public static class StringExtensions
  {
    /// <summary>
    /// Truncates a string
    /// </summary>
    /// <param name="str">The string to truncate</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <param name="withEllipsis">Flag to indicate if ellipsis should be added to the truncated string</param>
    /// <returns></returns>
    public static string Truncate(this string str, int maxLength, bool withEllipsis=true)
    {
      var ellipsis = withEllipsis ? "..." : string.Empty;
      return string.IsNullOrEmpty(str) || maxLength <= 0 || str.Length <= maxLength ? str : $"{str.Substring(0, maxLength-ellipsis.Length)}{ellipsis}";    
    }
  }
}
