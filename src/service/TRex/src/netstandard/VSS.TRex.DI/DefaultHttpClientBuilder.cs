using Microsoft.Extensions.DependencyInjection;

namespace VSS.TRex.DI
{
  public class DefaultHttpClientBuilder : IHttpClientBuilder
  {
    public readonly DIBuilder Instance;

    public DefaultHttpClientBuilder(IServiceCollection services, string name, DIBuilder instance)
    {
      Services = services;
      Name = name;
      Instance = instance;
    }

    public string Name { get; }

    public IServiceCollection Services { get; }
  }
}
