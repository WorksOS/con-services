using System;
using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.Helpers;

namespace VSS.Productivity3D.WebApi.Factories.ProductionData
{
  public interface IProductionDataRequestFactory
  {
    T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new();
    ProductionDataRequestFactory ExcludedIds(List<long> excludedIds);
    ProductionDataRequestFactory Headers(IDictionary<string, string> headers);
    ProductionDataRequestFactory ProjectId(long projectId);
    ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings);
  }
}