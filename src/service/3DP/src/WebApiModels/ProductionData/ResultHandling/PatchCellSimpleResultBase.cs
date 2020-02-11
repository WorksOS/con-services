using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  [ProtoContract(SkipConstructor = true), ProtoInclude(10, typeof(PatchCellSimpleResult))]
  public abstract class PatchCellSimpleResultBase
  {
  }
}
