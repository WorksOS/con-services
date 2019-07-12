using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Queues
{
  /// <summary>
  /// The key used to identify site model change in the buffer queue
  /// </summary>
  public class SiteModelChangeBufferQueueKey : ISiteModelChangeBufferQueueKey, IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project to process that TAG file into.
    /// This field also provides the affinity key mapping to the nodes in the mutable data grid
    /// </summary>
    public Guid ProjectUID { get; set; }

    public long InsertUTCTicks { get; set; }

    public SiteModelChangeBufferQueueKey()
    { }

    /// <summary>
    /// Site model change map queue key constructor taking project and  insert date
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="insertUTC"></param>
    public SiteModelChangeBufferQueueKey(Guid projectID, DateTime insertUTC)
    {
      if (insertUTC.Kind != DateTimeKind.Utc)
      {
        throw new ArgumentException("Date for site model change set is not in UTC as expected");
      }

      ProjectUID = projectID;
      InsertUTCTicks = insertUTC.Ticks;
    }

    /// <summary>
    /// Provides string representation of the state of the key
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      var dateTime = new DateTime(InsertUTCTicks, DateTimeKind.Utc);

      return $"Project: {ProjectUID}, InsertUTC: {dateTime}";
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUID);
      writer.WriteLong(InsertUTCTicks);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      InsertUTCTicks = reader.ReadLong();
    }
  }
}

