using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class CSVExportExecutor : BaseExecutor
  {
    public CSVExportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CSVExportExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionVetaExportRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<CompactionVetaExportRequest>();
        return null; // to keep compiler happy
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      // redundant?
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, request.Filter);

      // todoJeannie should this be a matching of VSS assetName with machineName?
      //     if so, we probably need iin machinesList AssetUID + VSS assetName
      var machinesListGuids = CSVExportHelper.GetRequestedMachines(siteModel, request.MachineNames);

      var filter = ConvertFilter(request.Filter, siteModel);
      // todoJeannie is this the correct place to put this date range?
      filter.AttributeFilter.StartTime = startEndDate.Item1;
      filter.AttributeFilter.EndTime = startEndDate.Item2;

      // todoJeannie what if existing contributing machines in filter?
      filter.AttributeFilter.MachinesList = machinesListGuids;
      
      var tRexRequest = new CSVExportRequest();
      var csvExportRequestArgument = AutoMapperUtility.Automapper.Map<CSVExportRequestArgument>(request);
      csvExportRequestArgument.Filters = new FilterSet(filter);

      var response = tRexRequest.Execute(csvExportRequestArgument);

      // todoJeannie veta vs passcount? setup Headers, sort and string together?
      
      return new CSVExportResult(new byte[]{});
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
