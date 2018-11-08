using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiTests
{
  [TestClass, Ignore("Temporary script to geneate .proto file for PatchResult while development is ongoing.")]
  public class GeneratePatchResultProtoFile
  {
    [TestMethod]
    public void GenerateProtoFile()
    {
      // After .proto file is created generate .cs client schema using:
      // $ protogen --proto_path=C:\temp PatchResult.proto --csharp_out=C:\temp

      var fileString = Serializer.GetProto<PatchResult>();
    }
  }
}
