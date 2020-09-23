namespace CCSS.WorksOS.Healthz.Models
{
  public class Service
  {
    public string Identifier { get; }
    public string Endpoint { get; }

    public Service(string identifier, string endpoint)
    {
      Identifier = identifier;
      Endpoint = endpoint;
    }
  }
}
