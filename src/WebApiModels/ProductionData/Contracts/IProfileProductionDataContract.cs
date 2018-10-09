using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  public interface IProfileProductionDataContract
  {
      ProfileResult Post([FromBody]ProfileProductionDataRequest request);
  }
}