namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Class to use for executing requests where the no result returned, apart from the HttpStatusCode.
  /// This is because the GracefulWebRequest has generic methods requiring a type parameter.
  /// </summary>
  public class EmptyModel
  {
  }
}
