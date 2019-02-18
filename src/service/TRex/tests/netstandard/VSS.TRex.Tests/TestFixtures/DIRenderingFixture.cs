using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIRenderingFixture : DITAGFileAndSubGridRequestsWithIgniteFixture
  {
    public DIRenderingFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))
        .Complete();
    }
  }
}
