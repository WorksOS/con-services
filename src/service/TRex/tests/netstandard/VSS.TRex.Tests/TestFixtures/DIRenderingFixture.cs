using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DataSmoothing;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DIRenderingFixture : DITAGFileAndSubGridRequestsWithIgniteFixture
  {
    public ConvolutionMaskSize smootherMaskSize = ConvolutionMaskSize.Mask3X3;
    public NullInfillMode smootherNullInfillMode = NullInfillMode.InfillNullValues;
    public bool smoothingActive = true;

    private IDataSmoother TileRenderingSmootherFactoryMethod(DisplayMode key)
    {
      return key switch
      {
        DisplayMode.Height => new ElevationArraySmoother(new ConvolutionTools<float>(), smootherMaskSize, smootherNullInfillMode),
        DisplayMode.CutFill => new ElevationArraySmoother(new ConvolutionTools<float>(), smootherMaskSize, smootherNullInfillMode),
        _ => null
      };
    }

    public DIRenderingFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))
        .Add(x => x.AddSingleton<Func<DisplayMode, IDataSmoother>>(provider => TileRenderingSmootherFactoryMethod))
        .Complete();

      var config = DIContext.Obtain<IConfigurationStore>();

      smoothingActive = config.GetValueBool("TILE_RENDERING_DATA_SMOOTHING_ACTIVE", Consts.TILE_RENDERING_DATA_SMOOTHING_ACTIVE);
      smootherMaskSize = (ConvolutionMaskSize)config.GetValueInt("TILE_RENDERING_DATA_SMOOTHING_MASK_SIZE", (int)Consts.TILE_RENDERING_DATA_SMOOTHING_MASK_SIZE);
      smootherNullInfillMode = (NullInfillMode)config.GetValueInt("TILE_RENDERING_DATA_SMOOTHING_NULL_INFILL_MODE", (int)Consts.TILE_RENDERING_DATA_SMOOTHING_NULL_INFILL_MODE);
    }
  }
}
