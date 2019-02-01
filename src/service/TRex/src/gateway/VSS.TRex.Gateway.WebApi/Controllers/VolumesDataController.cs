using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data for volumes statistics requests.
  /// </summary>
  public class VolumesDataController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public VolumesDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<VolumesDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <returns></returns>
    [Route("api/v1/volumes/summary")]
    [HttpPost]
    public SummaryVolumesResult PostSummaryVolumes([FromBody] SummaryVolumesDataRequest summaryVolumesRequest)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {Request.QueryString}");

      summaryVolumesRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryVolumesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(summaryVolumesRequest) as SummaryVolumesResult);
    }


    /// <summary>
    /// Get the summary volumes profile report for two surfaces.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <returns></returns>
    [Route("api/v1/volumes/summary/profile")]
    // todo change to proper request with summaryVolumesProfileRequest obj
//    [HttpPost]
  //  public SummaryVolumesProfileResult PostSummaryVolumesProfile([FromBody] SummaryVolumesProfileDataRequest summaryVolumesProfileRequest)

    [HttpGet]
    public SummaryVolumesProfileResult GetSummaryVolumesProfile()
    {


      // Debugging code only
      Guid projectUid = Guid.Parse("00000000-0000-0000-0000-000000000011");
      FilterResult baseFilter = new FilterResult();
      FilterResult topFilter = new FilterResult(); 
      Guid? referenceDesignUid = null; 
      VolumesType volumeCalcType = VolumesType.Between2Filters;
      baseFilter.ReturnEarliest = true;
      topFilter.ReturnEarliest = false;
      var summaryVolumesProfileRequest = new SummaryVolumesProfileDataRequest(projectUid, baseFilter, topFilter, referenceDesignUid,volumeCalcType, true, 2744.3073344000004, 1165.0196288000002, 2744.3073344000004, 1162.9496000000001);

      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {Request.QueryString}");

      summaryVolumesProfileRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryVolumesProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(summaryVolumesProfileRequest) as SummaryVolumesProfileResult);
    }


  }
}
