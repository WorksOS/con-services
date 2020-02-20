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
    private static IDataSmoother TileRenderingSmootherFactoryMethod(DisplayMode key, ConvolutionMaskSize convolutionMaskSize, NullInfillMode nullInfillMode)
    {
      switch (key)
      {
        case DisplayMode.Height:
          return new ElevationArraySmoother(new ConvolutionTools<float>(), convolutionMaskSize, nullInfillMode);
        default:
          return null;
      }
    }

    public DIRenderingFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))
        .Add(x => x.AddSingleton<Func<DisplayMode, ConvolutionMaskSize, NullInfillMode, IDataSmoother>>(provider => TileRenderingSmootherFactoryMethod))
        .Complete();
    }
  }
}
