using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace VSS.TRex.DI
{
  public static class DefaultHttpClientBuilderExtensions
  {
    public static DIBuilder AddHttpMessageHandler<THandler>(this DefaultHttpClientBuilder builder) where THandler : DelegatingHandler
    {
      if (builder == null)
      {
        throw new ArgumentNullException(nameof(builder));
      }

      builder.Services.AddTransient<IConfigureOptions<HttpClientFactoryOptions>>((Func<IServiceProvider, IConfigureOptions<HttpClientFactoryOptions>>)(services => (IConfigureOptions<HttpClientFactoryOptions>)new ConfigureNamedOptions<HttpClientFactoryOptions>(builder.Name, (Action<HttpClientFactoryOptions>)(options => options.HttpMessageHandlerBuilderActions.Add((Action<HttpMessageHandlerBuilder>)(b => b.AdditionalHandlers.Add((DelegatingHandler)services.GetRequiredService<THandler>())))))));

      return builder.Instance;
    }
  }
}
