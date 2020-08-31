using Apache.Ignite.Core.Binary;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;

namespace VSS.TRex.Designs
{
  public static class DesignDescriptorExtensions
  {
    public static void ToBinary(this DesignDescriptor descriptor, IBinaryRawWriter writer)
    {
      writer.WriteGuid(descriptor.FileUid);
      writer.WriteLong(descriptor.Id);
      writer.WriteDouble(descriptor.Offset);
      writer.WriteString(descriptor.File.FilespaceId);
      writer.WriteString(descriptor.File.FileName);
      writer.WriteString(descriptor.File.Path);
    }

    public static DesignDescriptor ReadDescriptor(IBinaryRawReader reader)
    {
      var fileUid = reader.ReadGuid();
      var id = reader.ReadLong();
      var offset = reader.ReadDouble();
      var fileSpaceId = reader.ReadString();
      var filename = reader.ReadString();
      var path = reader.ReadString();

      return new DesignDescriptor(id, FileDescriptor.CreateFileDescriptor(fileSpaceId, path, filename), offset, fileUid);
    }
  }
}
