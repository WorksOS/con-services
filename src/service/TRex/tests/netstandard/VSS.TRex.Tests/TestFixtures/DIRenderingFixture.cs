using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DataSmoothing;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIRenderingFixture : DITAGFileAndSubGridRequestsWithIgniteFixture
  {
    private static IDataSmoother TileRenderingSmootherFactoryMethod(DisplayMode key, NullInfillMode nullInfillMode)
    {
      switch (key)
      {
        case DisplayMode.Height:
          return new ElevationArraySmoother(new ConvolutionTools<float>(), ConvolutionMaskSize.Mask3X3, nullInfillMode);
        default:
          return null;
      }
    }

    public DIRenderingFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))
        .Add(x => x.AddSingleton<Func<DisplayMode, NullInfillMode, IDataSmoother>>(provider => TileRenderingSmootherFactoryMethod))
        .Complete();
    }
  }
}
