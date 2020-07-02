using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.CoordinateSystems.Executors
{
  /// <summary>
  /// Contains the business logic for adding a coordinate system to a project
  /// </summary>
  public class AddCoordinateSystemExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddCoordinateSystemExecutor>();

    /// <summary>
    /// Adds the given coordinate system to the identified project by placing the coordinate system
    /// into the mutable non spatial cache for the project. This will then be propagated to the immutable
    /// non spatial cache for the project
    /// Additionally, it notifies listeners of the coordinate system change.
    /// </summary>
    public bool Execute(Guid projectID, string CSIB)
    {
      // todo: Enrich return value to encode or provide additional information relating to failures

      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectID, true);
        siteModel.SetCSIB(csib);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred adding coordinate system to project");
        Console.WriteLine(e);
        throw;
      }

      return true;
    }
  }
}
