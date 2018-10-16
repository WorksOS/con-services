using System.IO;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface IMutabilityConverter
  {
    /// <summary>
    /// Converts the structure of the global latext cells structure into an immutable form
    /// </summary>
    /// <returns></returns>
    ISubGridCellLatestPassDataWrapper ConvertLatestPassesToImmutable(ISubGridCellLatestPassDataWrapper LatestPasses);

    /// <summary>
    /// Primary method for performing mutability conversion to immutable. It accepts a stream and an indication of the type of stream
    /// then delegates to conversion responsibility based on the stream type.
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, out MemoryStream immutableStream);

    /// <summary>
    /// Converts a subgrid directory into its immutable form
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    bool ConvertSubgridDirectoryToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream);

    /// <summary>
    /// Converts a subgrid segment into its immutable form
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    bool ConvertSubgridSegmentToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream);

    ///// <summary>
    ///// Convert an event list to it's immutable form. 
    ///// Currently this is a no-op - the original stream is returned as there is not yet an immutable description for events
    ///// </summary>
    ///// <param name="mutableStream"></param>
    ///// <param name="immutableStream"></param>
    ///// <returns></returns>
    //bool ConvertEventListToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream);
  }

}
