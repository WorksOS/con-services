using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request parameters for the CCA color palette
  /// </summary>
  public class CCAColorPaletteTrexRequest
  {
    public Guid ProjectUid { get; set; }
    public FilterResult Filter { get; set; }

    public CCAColorPaletteTrexRequest(Guid projectUid, FilterResult filter)
    {
      ProjectUid = projectUid;
      Filter = filter;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing ProjectUid"));
      }

      if (Filter == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing parameters"));
      }
      if (Filter.AssetIDs?.Count == 0 && Filter?.ContributingMachines?.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing Asset"));
      }
      if (Filter.StartUtc.HasValue || Filter.EndUtc.HasValue)
      {
        if (Filter.StartUtc.HasValue && Filter.EndUtc.HasValue)
        {
          if (Filter.StartUtc.Value > this.Filter.EndUtc.Value)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Start date must be earlier than end date!"));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using a date range both dates must be provided"));
        }
      }
    }
  }
}
