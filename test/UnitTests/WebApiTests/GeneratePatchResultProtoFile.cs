using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiTests
{
  [TestClass, Ignore("Tempoary script to geneate .proto file for PatchResult while development is ongoing.")]
  public class GeneratePatchResultProtoFile
  {
    [TestMethod]
    public void GenerateProtoFile()
    {
      var fileString = Serializer.GetProto<PatchResult>();
    }
  }
}
