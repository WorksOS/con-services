namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// This is the Proxy result for Get3dPmSchedulerBearerToken().
  /// </summary>
  public class TPaasOauthResult : BaseDataResult
  {

    public TPaasOauthRawResult tPaasOauthRawResult;
  }

  public class TPaasOauthRawResult 
  {

    /// <summary>
    ///   Bearer
    /// </summary>
    public string token_type;

    /// <summary>
    ///   Time, in seconds that this access_token will be current
    ///      50400 seconds = 14 hours
    /// </summary>
    public int expires_in;

    /// <summary>
    ///   Bearer token to use when calling 3dpmService
    /// </summary>
    /// 
    public string access_token;
  }
}