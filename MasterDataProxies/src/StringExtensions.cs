namespace VSS.MasterData.Proxies
{
  public static class StringExtensions
  {
    /// <summary>
    /// Truncates a string
    /// </summary>
    /// <param name="str">The string to truncate</param>
    /// <param name="maxLength">The maximum length of the string.</param>
    /// <returns></returns>
    public static string Truncate(this string str, int maxLength)
    {
      return string.IsNullOrEmpty(str) || maxLength <= 0 || str.Length <= maxLength ? str : str.Substring(0, maxLength);    
    }
  }
}
