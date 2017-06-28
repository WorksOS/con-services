using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Contracts
{
  public interface IProfileProductionDataContract
  {
      ProfileResult Post([FromBody]ProfileProductionDataRequest request);
  }
}