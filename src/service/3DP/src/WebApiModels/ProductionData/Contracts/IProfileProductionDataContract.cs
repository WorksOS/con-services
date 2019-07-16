using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  public interface IProfileProductionDataContract
  {
      Task<ProfileResult> Post([FromBody] ProfileProductionDataRequest request);
  }
}
