using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;

namespace VSS.Productivity3D.Models.Models
{
  public class TRexCutFillDetailsRequest : TRexBaseRequest
  {
    /// <summary>
    /// The collection of cut-fill tolerances to use. There must be 7 of them - 
    /// 3 cut values greater than zero, on-grade equal to zero and 3 fill values less than zero.
    /// Values are in meters.
    /// </summary>
    public double[] CutFillTolerances { get; private set; }

    /// <summary>
    /// The descriptor for the design for which to to generate the cut-fill data.
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; private set; }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TRexCutFillDetailsRequest(
      Guid projectUid,
      double[] tolerances,
      FilterResult filter,
      DesignDescriptor designDescriptor,
      OverridingTargets overrides,
      LiftSettings liftSettings)
    {
      ProjectUid = projectUid;
      CutFillTolerances = tolerances;
      Filter = filter;
      DesignDescriptor = designDescriptor;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (DesignDescriptor != null)
      {
        DesignDescriptor.Validate();
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Design must be specified for cut-fill details"));
      }
    }
  }
}

