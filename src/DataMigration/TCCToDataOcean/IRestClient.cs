namespace TCCToDataOcean
{
  public interface IRestClient
  {
    string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, string mediaType, string customerUid);
  }
}
