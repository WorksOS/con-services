using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.Foundation.Coordinators
{
  /// <summary>
  /// Base class used by all Analytics style operations. It defines common state and behaviour for those requests 
  /// at the client context level.
  /// </summary>
  public abstract class BaseAnalyticsCoordinator<TArgument, TResponse> : IBaseAnalyticsCoordinator<TArgument, TResponse> where TArgument : BaseApplicationServiceRequestArgument
      where TResponse : BaseAnalyticsResponse, new()
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    // Warn on analytics requests that take more than this time to service (20 seconds)
    private static readonly TimeSpan _analyticsRequestTimeSpanWarnLimit = new TimeSpan(0, 0, 20);

    /// <summary>
    /// The SiteModel context for computing the result of the request
    /// </summary>
    public ISiteModel SiteModel { get; set; }

    /// <summary>
    /// Request descriptor used to track this request in different parts of the cluster compute
    /// </summary>
    public Guid RequestDescriptor { get; set; }

    /// <summary>
    /// Execution method for the derived coordinator to override
    /// </summary>
    public async Task<TResponse> ExecuteAsync(TArgument arg)
    {
      _log.LogInformation("In: Executing Coordination logic");
      TResponse response = default;

      var requestStopWatch = Stopwatch.StartNew();

      try
      {
        response = new TResponse();

        RequestDescriptor = Guid.NewGuid();
        SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

        if (SiteModel == null)
        {
          response.ResultStatus = RequestErrorStatus.NoSuchDataModel;
          return response;
        }

        using var aggregator = ConstructAggregator(arg);
        var computor = ConstructComputor(arg, aggregator);

        if (await computor.ComputeAnalytics(response))
        {
          // Instruct the aggregator to perform any finalisation logic before returning results
          aggregator.Finalise();

          ReadOutResults(aggregator, response);
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred while coordinating analytics function");
        return default;
      }
      finally
      {
        _log.LogInformation($"Out: Executing Coordination logic, elapsed time = {requestStopWatch}");

        // Flag tile renders that take more than 20 seconds to render...
        if (requestStopWatch.Elapsed > _analyticsRequestTimeSpanWarnLimit)
        {
          _log.LogInformation($"Analytics request required more than {_analyticsRequestTimeSpanWarnLimit} to complete");
        }
      }

      return response;
    }

    /// <summary>
    /// Constructs the aggregator to be used as the reduction function for the MapReduceReduce computation
    /// </summary>
    public abstract AggregatorBase ConstructAggregator(TArgument argument);

    /// <summary>
    /// Constructs the computor responsible for orchestrating information requests, essentially the map part of the MapReduceReduce computation
    /// </summary>
    public abstract AnalyticsComputor ConstructComputor(TArgument argument, AggregatorBase aggregator);

    /// <summary>
    /// Transcribes the results of the computation from the internal response type to the external response type
    /// </summary>
    public abstract void ReadOutResults(AggregatorBase aggregator, TResponse response);
  }
}
