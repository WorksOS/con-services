using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class CalculateDesignProfileResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public List<XYZS> Profile { get; set; } = new List<XYZS>();

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(Profile?.Count ?? 0);

      if (Profile != null)
      {
        foreach (var pt in Profile)
        {
          writer.WriteDouble(pt.X);
          writer.WriteDouble(pt.Y);
          writer.WriteDouble(pt.Z);
          writer.WriteDouble(pt.Station);
          writer.WriteInt(pt.TriIndex);
        }
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      var count = reader.ReadInt();
      Profile = new List<XYZS>(count);
      for (int i = 0; i < count; i++)
      {
        Profile.Add(new XYZS
        {
          X = reader.ReadDouble(),
          Y = reader.ReadDouble(),
          Z = reader.ReadDouble(),
          Station = reader.ReadDouble(),
          TriIndex = reader.ReadInt()
        });
      }
    }
  }
}
