using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIRenderingFixture : IDisposable
  {
    private static object Lock = new object();

    public DIRenderingFixture()
    {
      lock (Lock)
      {
        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))
          .Complete();
      }
    }

    public void Dispose()
    {
    } // Nothing needing doing   }
  }
}
