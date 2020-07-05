using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps
{
  /// <summary>
  /// A static class facade around requests for existence maps related to designs and surveyed surfaces
  /// </summary>
  public class ExistenceMaps : IExistenceMaps
  {
    private readonly GetSingleExistenceMapRequest getSingleRequest = new GetSingleExistenceMapRequest();
    private readonly GetCombinedExistenceMapRequest getCombinedRequest = new GetCombinedExistenceMapRequest();

    public ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID) => getSingleRequest.Execute(siteModelID, descriptor, ID);
    public ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys) => getCombinedRequest.Execute(siteModelID, keys);

    /// <summary>
    /// Adds the DI context elements relevant to existence maps interaction
    /// </summary>
    private static void AddDIEntries()
    {
      DIBuilder.Continue()
        .Add(x => x.AddSingleton<IExistenceMapServer>(factory => new Servers.ExistenceMapServer()))
        .Add(x => x.AddSingleton<IExistenceMaps>(factory => new ExistenceMaps()));
    }

    /// <summary>
    /// If the calling context is directly using an IServiceCollection then obtain the DIBuilder based on it before adding...
    /// </summary>
    /// <param name="services"></param>
    public static void AddExistenceMapFactoriesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries());
    }
  }
}
