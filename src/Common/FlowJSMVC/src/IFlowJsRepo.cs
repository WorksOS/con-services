using Microsoft.AspNetCore.Http;

namespace VSS.FlowJSHandler
{
  public interface IFlowJsRepo
  {
    FlowJsPostChunkResponse PostChunk(HttpRequest request, string folder, FlowValidationRules validationRules = null);
    bool ChunkExists(string folder, HttpRequest request);
  }
}
