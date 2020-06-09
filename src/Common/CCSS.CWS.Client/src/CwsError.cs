namespace CCSS.CWS.Client
{
  /// <summary>
  /// Errors returned from CWS
  /// </summary>
  public class CwsError
  {
    public int status { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public string moreInfo { get; set; }
    //public string timestamp { get; set; }//some timestamp values are ticks and some are date strings so can't deserialize this property. Ignore it.
  }
}
