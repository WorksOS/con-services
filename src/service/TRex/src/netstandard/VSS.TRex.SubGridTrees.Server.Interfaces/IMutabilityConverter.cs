using System.IO;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface IMutabilityConverter
  {
    /// <summary>
    /// Primary method for performing mutability conversion to immutable. It accepts a stream and an indication of the type of stream
    /// then delegates to conversion responsibility based on the stream type.
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, object source, out MemoryStream immutableStream);
  }
}

