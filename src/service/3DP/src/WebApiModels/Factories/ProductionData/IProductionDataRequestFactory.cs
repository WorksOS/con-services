using System;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Factories.ProductionData
{
  public interface IProductionDataRequestFactory
  {
    T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new();
    ProductionDataRequestFactory Headers(IHeaderDictionary headers);
    ProductionDataRequestFactory ProjectId(long projectId);
    ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings);
  }
}
