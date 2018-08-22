using System.Net.Http;

namespace TestUtility
{
  public struct ImportOptions
  {
    public HttpMethod HttpMethod;
    public string[] QueryParams;
    public bool UseFlowJs;
    
    public ImportOptions(HttpMethod httpMethod = null, string[] queryParams = null, bool useFlowJs = true)
    {
      HttpMethod = httpMethod ?? HttpMethod.Post;
      QueryParams = queryParams;
      UseFlowJs = useFlowJs;
    }
  }
}
