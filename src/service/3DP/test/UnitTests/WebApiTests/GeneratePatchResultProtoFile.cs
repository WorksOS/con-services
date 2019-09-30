using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiTests
{
  [TestClass]
  //[TestClass, Ignore("Temporary script to generate .proto file for PatchResult while development is ongoing.")]
  public class GeneratePatchResultProtoFile
  {
    [TestMethod]
    public void GenerateProtoFile()
    {
      // After .proto file is created generate .cs client schema using:
      // $ protogen --proto_path=C:\temp PatchResult.proto --csharp_out=C:\temp
      // .\protogen --proto_path=.\ 3dpPatchSubgridsResult.proto--csharp_out =.\

      // var fileString = Serializer.GetProto<PatchSimpleListResult>();
      var fileString = Serializer.GetProto<PatchSubgridsProtobufResult>();
    }
  }
}
