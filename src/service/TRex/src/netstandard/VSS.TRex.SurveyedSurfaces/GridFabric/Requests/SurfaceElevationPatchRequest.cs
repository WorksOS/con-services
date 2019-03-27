﻿using Apache.Ignite.Core.Compute;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  public class SurfaceElevationPatchRequest : DesignProfilerRequest<ISurfaceElevationPatchArgument, IClientLeafSubGrid>, ISurfaceElevationPatchRequest
  {
    //private static readonly ILogger Log = Logging.Logger.CreateLogger<SurfaceElevationPatchRequest>();

    /// <summary>
    /// Reference to the general sub grid result cache
    /// </summary>
    private readonly ITRexSpatialMemoryCache _cache;

    /// <summary>
    /// The cache context to be used for all requests made through this request instance
    /// </summary>
    private readonly ITRexSpatialMemoryCacheContext _context;

    /// <summary>
    /// Reference to the client sub grid factory
    /// </summary>

    private readonly IClientLeafSubGridFactory ClientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// The compute function used for surface elevation patch requests
    /// </summary>
    private readonly IComputeFunc<ISurfaceElevationPatchArgument, byte[]> _computeFunc = new SurfaceElevationPatchComputeFunc();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SurfaceElevationPatchRequest(ITRexSpatialMemoryCache cache, ITRexSpatialMemoryCacheContext context) : this()
    {
      _cache = cache;
      _context = context;
    }

    public SurfaceElevationPatchRequest()
    {
    }

    public override IClientLeafSubGrid Execute(ISurfaceElevationPatchArgument arg)
    {
      IClientLeafSubGrid clientResult = null;

      // Check the item is available in the cache
      if (_context?.Get(arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY) is IClientLeafSubGrid cacheResult)
      {
        clientResult = ClientLeafSubGridFactory.GetSubGrid(arg.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime);
        clientResult.Assign(cacheResult);

        return clientResult;
      }

      // Always request the full sub grid from the surveyed surface engine
      arg.ProcessingMap.Fill();

      byte[] result = Compute.Apply(_computeFunc, arg);

      if (result != null)
      {
        clientResult = ClientLeafSubGridFactory.GetSubGrid(arg.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime);
        clientResult.FromBytes(result);

        // Fow now, only cache non-composite elevation sub grids
        if (arg.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations && _context != null)
          _cache?.Add(_context, clientResult);
      }

      return clientResult;
    }
  }
}
