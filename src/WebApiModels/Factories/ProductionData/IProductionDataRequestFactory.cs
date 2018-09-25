using System;
using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Factories.ProductionData
{
  public interface IProductionDataRequestFactory
  {
    T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new();
    ProductionDataRequestFactory Headers(IDictionary<string, string> headers);
    ProductionDataRequestFactory ProjectId(long projectId);
    ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings);
  }
}
