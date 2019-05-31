using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request parameters for the CCA color palette
  /// </summary>
  public class ColorPaletteRequest
  {
    public Guid ProjectUid { get; set; }
    public Guid AssetUid { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public int? LiftId { get; set; }

    public ColorPaletteRequest(Guid projectUid, Guid assetUid, DateTime? startUtc, DateTime? endUtc, int? liftId)
    {
      ProjectUid = projectUid;
      AssetUid = assetUid;
      StartUtc = startUtc;
      EndUtc = endUtc;
      LiftId = liftId;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing ProjectUid"));
      }
      if (AssetUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing AssetUid"));
      }
      if (StartUtc.HasValue || EndUtc.HasValue)
      {
        if (StartUtc.HasValue && EndUtc.HasValue)
        {
          if (StartUtc.Value > this.EndUtc.Value)
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
