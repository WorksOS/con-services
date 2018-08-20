using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems.Executors
{
  /// <summary>
  /// Contains the busness logic for adding a coordinate system to a project
  /// </summary>
  public class AddCoordinateSystemExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AddCoordinateSystemExecutor>();

    /// <summary>
    /// Adds the given coordinate system to the identified project by placing the coordinate system
    /// into the mutable non spatial cache for the project. This will then be propagated to the immutable
    /// non spatial cache for the project
    /// Additionally, it notifies listeners of the coordinate system change.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="CSIB"></param>
    /// <returns></returns>
    public bool Execute(Guid projectID, string CSIB)
    {
      // todo: Enrich return value to encode or provide additional information relating to failures

      try
      {
        // Add the coordinate system to the cache
        IStorageProxy storageProxy = StorageProxy.Instance(StorageMutability.Mutable);

        using (MemoryStream csibStream = new MemoryStream(Encoding.ASCII.GetBytes(CSIB)))
        {
          FileSystemErrorStatus status = storageProxy.WriteStreamToPersistentStore(projectID, CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName,
            FileSystemStreamType.CoordinateSystemCSIB, csibStream);

          if (status != FileSystemErrorStatus.OK)
            return false;
        }

        // Notify listeners of the coordinate system change
        // TODO: Listener end points are not yet implemented
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} occurred adding coordinate system to project");
        Console.WriteLine(e);
        throw;
      }

      return true;
    }
  }
}
