using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Contracts
{
  public interface IProfileProductionDataContract
  {
      ProfileResult Post([FromBody]ProfileProductionDataRequest request);
  }
}