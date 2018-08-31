using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class CutFillDetailsRequest : ProjectID
  {
    /// <summary>
    /// The collection of cut-fill tolerances to use. There must be 7 of them - 
    /// 3 cut values greater than zero, on-grade equal to zero and 3 fill values less than zero.
    /// Values are in meters.
    /// </summary>
    public double[] CutFillTolerances { get; private set; }
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// The descriptor for the design for which to to generate the cut-fill data.
    /// </summary>
    public DesignDescriptor designDescriptor { get; private set; }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="tolerances"></param>
    /// <param name="filter"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="designDescriptor"></param>
    public CutFillDetailsRequest(long projectId, double[] tolerances, FilterResult filter, LiftBuildSettings liftBuildSettings, DesignDescriptor designDescriptor)
    {
      ProjectId = projectId;
      CutFillTolerances = tolerances;
      this.filter = filter;
      this.liftBuildSettings = liftBuildSettings;
      this.designDescriptor = designDescriptor;
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="tolerances"></param>
    /// <param name="filter"></param>
    /// <param name="designDescriptor"></param>
    public CutFillDetailsRequest(Guid projectUid, double[] tolerances, FilterResult filter, DesignDescriptor designDescriptor)
    {
      ProjectUid = projectUid;
      CutFillTolerances = tolerances;
      this.filter = filter;
      this.designDescriptor = designDescriptor;
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (filter != null)
      {
        filter.Validate();  
      }

      liftBuildSettings?.Validate();

      if (designDescriptor != null)
      {
        designDescriptor.Validate();
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
