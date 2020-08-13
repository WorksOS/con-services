using System;
using Apache.Ignite.Core.Binary;
using Force.DeepCloner;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Common.Serialisation
{
  /// <summary>
  /// Provides a class that is registered with Ignite to enforce exclusive of IBinarizable based serialization
  /// </summary>
  public class BinarizableSerializer : IBinarySerializer
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<BinarizableSerializer>();

    public void WriteBinary(object obj, IBinaryWriter writer)
    {
      // Add a global exception trapper for binarizable serialization to provide visibility via C# stack traces in the log
      // to complement the Java stack trace
      try
      {
        switch (obj)
        {
          case IBinarizable bin:
            bin.WriteBinary(writer);
            return;
          case Exception e:
            writer.WriteObject("Exception", e);
            return;
          default:
            _log.LogCritical($"Not IBinarizable on WriteBinary: { obj.GetType()}");
            break;
        }
      }
      catch (Exception e)
      {
        _log.LogCritical(e, "WriteBinary failure");
        // Don't rethrow the exception as this can lead to unhandle-able SEHExceptions in DotNet
      }
    }

    public void ReadBinary(object obj, IBinaryReader reader)
    {
      // Add a global exception trapper for binarizable serialization to provide visibility via C# stack traces in the log
      // to compliment the Java stack trace
      try
      {
        switch (obj)
        {
          case IBinarizable bin:
            bin.ReadBinary(reader);
            return;
          case Exception e:
            var res = reader.ReadObject<Exception>("Exception");
            res.ShallowCloneTo(e);
            return;
          default:
            _log.LogCritical($"Not IBinarizable on ReadBinary: { obj.GetType()}");
            break;
        }
      }
      catch (Exception e)
      {
        _log.LogCritical(e, "ReadBinary failure");
        // Don't rethrow the exception as this can lead to unhandle-able SEHExceptions in DotNet
      }
    }
  }
}
