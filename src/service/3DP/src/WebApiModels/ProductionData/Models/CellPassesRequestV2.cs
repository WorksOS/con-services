using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class CellPassesRequestV2 : CellPassesRequest, IValidatable
  {
    /// <summary>
    /// Filter UID, if set will replace the Filter Model with the associated Filter
    /// </summary>
    [JsonProperty(PropertyName = "filterUid", Required = Required.Default)]
    public Guid? FilterUid { get; set; }

    public void SetFilter(FilterResult filter)
    {
      this.filter = filter;
    }

    public override void Validate()
    {
      base.Validate();

      if (FilterUid.HasValue && filterId.HasValue )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Cannot set FilterUID with FilterID"));
      }
    }
  }
}
